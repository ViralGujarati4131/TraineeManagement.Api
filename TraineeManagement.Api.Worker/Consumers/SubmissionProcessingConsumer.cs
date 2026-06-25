using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Text.Json;
using TraineeManagement.Api.Contract.SubmissionProcessingContarct;
using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.TaskAssignmentModel;
using TraineeManagement.Api.Messaging.RabbitMqConnection;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.CacheServiceInterface;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using TraineeManagement.Api.Data.ProcessingJobModel;

namespace TraineeManagement.Api.Worker.SubmissionProcessingConsumer;

public class SubmissionProcessingConsumer : BackgroundService
{
    private readonly ILogger<SubmissionProcessingConsumer> _logger;
    private readonly RabbitConnection _connection;  
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICacheService _cacheService;
    
    private const string TargetQueueName = "submission-processing";
    private const int MaxRetryAttempts = 3;

    public SubmissionProcessingConsumer(
        ILogger<SubmissionProcessingConsumer> logger, 
        RabbitConnection connection,
        IServiceScopeFactory scopeFactory,
        ICacheService cacheService)
    {
        _logger = logger;
        _connection = connection;
        _scopeFactory = scopeFactory;
        _cacheService = cacheService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _connection.InitializeAsync();
        await _connection.RegisterQueueAsync(TargetQueueName);

        if (_connection.Channel is null)
        {
            _logger.LogError("RabbitMQ channel failed to initialize.");
            return;
        }

        await _connection.Channel.BasicQosAsync(prefetchSize: 0, prefetchCount: 1, global: false, cancellationToken: stoppingToken);

        var consumer = new AsyncEventingBasicConsumer(_connection.Channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var json = Encoding.UTF8.GetString(body);

            SubmissionProcessingContract? message = null;
            try
            {
                message = JsonSerializer.Deserialize<SubmissionProcessingContract>(json);
                if (message == null) throw new JsonException("Invalid message contract format.");

                _logger.LogInformation("De-queuing MessageId={MessageId} for validation...", message.MessageId);

                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    var existingJob = await dbContext.ProcessingJobs.FirstOrDefaultAsync(j => j.Id == message.MessageId, stoppingToken);

                    if (existingJob != null && existingJob.Status == 3) 
                    {
                        _logger.LogWarning("Idempotency Alert: MessageId={MessageId} has already been fully processed. Skipping execution.", message.MessageId);
                        await _connection.Channel.BasicAckAsync(ea.DeliveryTag, multiple: false, stoppingToken);
                        return;
                    }

                    if (existingJob == null)
                    {
                        existingJob = new ProcessingJob
                        {
                            Id = message.MessageId,
                            CorrelationId = message.CorrelationId,
                            SubmissionId = message.SubmissionId,
                            Status = 2, 
                            Attempts = 1,
                            StartedAt = DateTime.UtcNow
                        };
                        dbContext.ProcessingJobs.Add(existingJob);
                    }
                    else
                    {
                        existingJob.Status = 2;
                        existingJob.Attempts++;
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Executing background calculation task for SubmissionId={SubmissionId}...", message.SubmissionId);

                    await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                    var submission = await dbContext.Submissions.FirstOrDefaultAsync(s => s.Id == message.SubmissionId, stoppingToken);
                    if (submission == null)
                    {
                        throw new KeyNotFoundException($"Submission entry targeting identity reference {message.SubmissionId} is missing.");
                    }

                    var assignment = await dbContext.TaskAssignments.FirstOrDefaultAsync(a => a.Id == submission.TaskAssignmentId, stoppingToken);
                    if (assignment != null)
                    {
                        assignment.Status = TaskAssignmentStatus.Submitted;
                    }

                    existingJob.Status = 3; 
                    existingJob.CompletedAt = DateTime.UtcNow;
                    
                    await dbContext.SaveChangesAsync(stoppingToken);

                    if (assignment != null)
                    {
                        // Safe key cache clearing post state transition completion
                        await _cacheService.RemoveAsync($"task-assignment:{assignment.Id}");
                    }
                }

                // Explicit manual Acknowledgment confirmation 
                await _connection.Channel.BasicAckAsync(deliveryTag: ea.DeliveryTag, multiple: false, cancellationToken: stoppingToken);
                _logger.LogInformation("Successfully completed and acknowledged processing loop for MessageId={MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing asynchronous job processing routine.");
                await HandleFailureAsync(ea, message, json, ex, stoppingToken);
            }
        };

        await _connection.Channel.BasicConsumeAsync(queue: TargetQueueName, autoAck: false, consumer: consumer, cancellationToken: stoppingToken);
    }

    private async Task HandleFailureAsync(BasicDeliverEventArgs ea, SubmissionProcessingContract? message, string json, Exception exception, CancellationToken stoppingToken)
    {
        try
        {
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                bool isPermanentFailure = exception is JsonException || exception is KeyNotFoundException;

                if (message != null)
                {
                    var job = await dbContext.ProcessingJobs.FirstOrDefaultAsync(j => j.Id == message.MessageId, stoppingToken);
                    if (job != null)
                    {
                        job.ErrorSummary = exception.Message;
                    
                        if (isPermanentFailure || job.Attempts >= MaxRetryAttempts)
                        {
                            job.Status = 4; 
                            job.CompletedAt = DateTime.UtcNow;
                            _logger.LogError("Critical Job Failure: MessageId={MessageId} marked as FAILED permanently.", message.MessageId);
                        }
                        else
                        {
                            job.Status = 1; 
                            _logger.LogWarning("Transient Failure: MessageId={MessageId} scheduled for retry sequence.", message.MessageId);
                        }
                        
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }

                if (isPermanentFailure)
                {
                    // Acknowledge bad payload structure format so it is cleared away immediately without poisonous requeues
                    await _connection.Channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: false, cancellationToken: stoppingToken);
                }
                else
                {
                    // For verified transient infrastructure degradation, bounce message right back up into the loop
                    await _connection.Channel!.BasicNackAsync(ea.DeliveryTag, multiple: false, requeue: true, cancellationToken: stoppingToken);
                }
            }
        }
        catch (Exception criticalDbException)
        {
            _logger.LogCritical(criticalDbException, "Fatal processing handler fault encountered. State tracking boundary compromised.");
        }
    }
}