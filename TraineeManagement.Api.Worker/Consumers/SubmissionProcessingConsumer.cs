using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TraineeManagement.Api.Data.CacheServiceInterface;
using TraineeManagement.Api.Contract.SubmissionProcessingContarct;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.ProcessingJobModel;
using TraineeManagement.Api.Data.SubmissionFileModel;
using TraineeManagement.Api.Data.TaskAssignmentModel;
using TraineeManagement.Api.Data.FileStoreValidation;
using TraineeManagement.Api.Messaging.RabbitMqConnection;
using TraineeManagement.Api.Data.Response;


namespace TraineeManagement.Api.Worker.SubmissionProcessingConsumer;

public class SubmissionProcessingConsumer : BackgroundService
{
    private readonly ILogger<SubmissionProcessingConsumer> _logger;
    private readonly RabbitConnection _connection;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICacheService _cacheService;
    private readonly string _rootPath;
    private IChannel? channel;

    private const int MaxRetryAttempts = 3;
    string QueueName = AppConstants.RabbitMQ.SubmissionProcessing;

    public SubmissionProcessingConsumer(
        ILogger<SubmissionProcessingConsumer> logger,
        RabbitConnection connection,
        IServiceScopeFactory scopeFactory,
        ICacheService cacheService,
        IOptions<CustomFileStoreValidation> fileConfiguration)
    {
        _logger = logger;
        _connection = connection;
        _scopeFactory = scopeFactory;
        _cacheService = cacheService;

        CustomFileStoreValidation config = fileConfiguration.Value ?? throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
        if (string.IsNullOrWhiteSpace(config.RootPath))
        {
            throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
        }

        _rootPath = Path.Combine(config.BasePath, config.RootPath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        IConnection? connection = _connection.Connection!;

        if (connection == null || !connection.IsOpen)
        {
            _logger.LogError("RabbitMQ underlying connection is down or uninitialized.");
            throw new OperationException(CustomResponse.ConnectionNotInitialized);
        }

        await using IChannel _channel = await connection.CreateChannelAsync();
        channel = _channel;

        await _connection.RegisterQueueAsync(QueueName);

        if (channel is null)
        {
            _logger.LogError("RabbitMQ channel failed to initialize for queue: {Queue}", QueueName);
            throw new OperationException(CustomResponse.ChannelNotInitialized);
        }

        await channel.BasicQosAsync(
            prefetchSize: 0,
            prefetchCount: 1,
            global: false,
            cancellationToken: stoppingToken
        );

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(channel);

        consumer.ReceivedAsync += async (object model, BasicDeliverEventArgs ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string json = Encoding.UTF8.GetString(body);

            string? correlationId = ea.BasicProperties.CorrelationId ?? "Unknown";

            SubmissionProcessingContract? message = null;
            try
            {
                message = JsonSerializer.Deserialize<SubmissionProcessingContract>(json)
                        ?? throw new JsonConversionException(CustomResponse.JsonConversionError);

                    _logger.LogInformation("Consume message started. CorrelationId: {CorrelationId}", correlationId);

                    await ProcessSubmissionInternalAsync(message, stoppingToken, correlationId);

                    await channel.BasicAckAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        cancellationToken: stoppingToken
                    );

                    _logger.LogInformation("Consume message acknowledged successfully. CorrelationId: {CorrelationId}", correlationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dependency failure processing asynchronous subscriber execution stream. CorrelationId: {CorrelationId}", correlationId);
                await HandleFailureAsync(ea, message, json, ex, correlationId, stoppingToken);
            }
        };

        await channel.BasicConsumeAsync(
            queue: AppConstants.RabbitMQ.GetQueue(QueueName),
            autoAck: false,
            consumer: consumer,
            cancellationToken: stoppingToken
        );
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task ProcessSubmissionInternalAsync(SubmissionProcessingContract message, CancellationToken stoppingToken, string correlationId)
    {
        IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        ProcessingJob? existingJob = await dbContext.ProcessingJobs
            .FirstOrDefaultAsync(j => j.Id == message.ProcessingJobId, stoppingToken);

        if (existingJob?.Status == ProcessingJobStatus.Completed)
        {
            _logger.LogInformation("State transition: Completed. JobId: {JobId}, Attempt: {Attempt}, CorrelationId: {CorrelationId}", message.ProcessingJobId, existingJob.Attempts, correlationId);
            return;
        }

        existingJob!.Status = ProcessingJobStatus.Processing;
        existingJob.StartedAt = DateTime.UtcNow;
        existingJob.Attempts++;

        _logger.LogInformation("State transition: Processing. JobId: {JobId}, Attempt: {Attempt}, CorrelationId: {CorrelationId}", message.ProcessingJobId, existingJob.Attempts, correlationId);
        await dbContext.SaveChangesAsync(stoppingToken);

        _logger.LogInformation("TaskAssignment={TaskAssignmentId} status map to Submitted. CorrelationId: {CorrelationId}", message.TaskAssignmentId, correlationId);
        TaskAssignment assignment = await dbContext.TaskAssignments
            .FirstOrDefaultAsync(a => a.Id == message.TaskAssignmentId, stoppingToken)
            ?? throw new NotFoundException(CustomResponse.NotFound, "TaskAssignment");

        assignment.Status = TaskAssignmentStatus.Submitted;

        _logger.LogInformation("Evaluating file storage deduplication for SubmissionFileId={FileId}. CorrelationId: {CorrelationId}", message.SubmissionFileId, correlationId);
        SubmissionFile newSubmissionFile = await dbContext.SubmissionFiles
            .FirstOrDefaultAsync(sf => sf.Id == message.SubmissionFileId, stoppingToken)
            ?? throw new NotFoundException(CustomResponse.NotFound, "SubmissionFile");

        IEnumerable<SubmissionFile> duplicateFiles = await dbContext.SubmissionFiles
            .Where(sf => sf.Checksum == newSubmissionFile.Checksum && sf.Id != newSubmissionFile.Id)
            .ToListAsync(stoppingToken);

        foreach (SubmissionFile file in duplicateFiles)
        {
            string filePath = Path.Combine(_rootPath, file.StorageFileName);
            if (File.Exists(filePath))
            {
                string targetPathToDelete = Path.Combine(_rootPath, newSubmissionFile.StorageFileName);

                if (File.Exists(targetPathToDelete))
                {
                    File.Delete(targetPathToDelete);
                }

                newSubmissionFile.StorageFileName = file.StorageFileName;
                _logger.LogInformation("Deduplication Success: Re-mapped file pointer. CorrelationId: {CorrelationId}", correlationId);
                break;
            }
        }

        existingJob.Status = ProcessingJobStatus.Completed;
        existingJob.CompletedAt = DateTime.UtcNow;

        _logger.LogInformation("State transition: Completed. JobId: {JobId}, CorrelationId: {CorrelationId}", existingJob.Id, correlationId);
        await dbContext.SaveChangesAsync(stoppingToken);

        string cacheKey = $"task-assignment:{assignment.Id}";
        await _cacheService.RemoveAsync(cacheKey);
        _logger.LogInformation("Cache Invalid. CorrelationId: {CorrelationId}", correlationId);
    }

    private async Task HandleFailureAsync(
        BasicDeliverEventArgs ea,
        SubmissionProcessingContract? message,
        string json,
        Exception exception,
        string correlationId,
        CancellationToken stoppingToken)
    {
        try
        {
            IServiceScope scope = _scopeFactory.CreateScope();
            AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            bool isPermanentFailure = exception is JsonConversionException ||
                                      exception is NotFoundException ||
                                      exception is JsonException;

            if (message != null)
            {
                ProcessingJob? job = await dbContext.ProcessingJobs
                    .FirstOrDefaultAsync(j => j.Id == message.ProcessingJobId, stoppingToken);

                if (job != null)
                {
                    job.ErrorSummary = exception.Message;

                    if (isPermanentFailure || job.Attempts >= MaxRetryAttempts)
                    {
                        job.Status = ProcessingJobStatus.Failed;
                        job.CompletedAt = DateTime.UtcNow;
                        _logger.LogError("State transition: Failed. Permanent or max retry reached. JobId: {JobId}, Attempt: {Attempt}, CorrelationId: {CorrelationId}", job.Id, job.Attempts, correlationId);
                    }
                    else
                    {
                        job.Status = ProcessingJobStatus.Queued;
                        _logger.LogWarning("State transition: Re-queued. Transient failure. JobId: {JobId}, Attempt: {Attempt}, CorrelationId: {CorrelationId}", job.Id, job.Attempts, correlationId);

                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }

                if (isPermanentFailure)
                {
                    await channel!.BasicNackAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: false,
                        cancellationToken: stoppingToken
                    );
                    _logger.LogWarning("Publish outcome: Dead-lettered/Dropped. DeliveryTag: {DeliveryTag}, CorrelationId: {CorrelationId}", ea.DeliveryTag, correlationId);
                }
                else
                {
                    await channel!.BasicNackAsync(
                        deliveryTag: ea.DeliveryTag,
                        multiple: false,
                        requeue: true,
                        cancellationToken: stoppingToken
                    );
                    _logger.LogWarning("Publish outcome: Re-queued to broker. DeliveryTag: {DeliveryTag}, CorrelationId: {CorrelationId}", ea.DeliveryTag, correlationId);
                }
            }
        }
        catch (Exception)
        {
            _logger.LogCritical("Failure. CorrelationId: {CorrelationId}", correlationId);
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background worker shutting down.");

        if (channel is not null)
        {
            await channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}