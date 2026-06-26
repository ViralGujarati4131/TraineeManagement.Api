using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using TraineeManagement.Api.CacheServiceInterface;
using TraineeManagement.Api.Contract.SubmissionProcessingContarct;
using TraineeManagement.Api.Data.DatabaseContext; 
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.ProcessingJobModel;
using TraineeManagement.Api.Data.SubmissionFileModel;
using TraineeManagement.Api.Data.TaskAssignmentModel;
using TraineeManagement.Api.FileStoreValidation;
using TraineeManagement.Api.Messaging.RabbitMqConnection;


namespace TraineeManagement.Api.Worker.SubmissionProcessingConsumer;

public class SubmissionProcessingConsumer : BackgroundService
{
    private readonly ILogger<SubmissionProcessingConsumer> _logger;
    private readonly RabbitConnection _connection;  
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ICacheService _cacheService;
    private readonly string _rootPath;
    
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

        CustomFileStoreValidation config = fileConfiguration.Value ?? throw new FileStorageConfigurationException();
        if (string.IsNullOrWhiteSpace(config.RootPath))
        {
            throw new FileStorageConfigurationException();
        }
            
        _rootPath = Path.Combine(config.BasePath, config.RootPath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _connection.RegisterQueueAsync(QueueName);

        if (_connection.Channel is null)
        {
            _logger.LogError("RabbitMQ channel failed to initialize for queue: {Queue}", QueueName);
            return;
        }

        await _connection.Channel.BasicQosAsync(
            prefetchSize: 0, 
            prefetchCount: 1, 
            global: false, 
            cancellationToken: stoppingToken
        );

        AsyncEventingBasicConsumer consumer = new AsyncEventingBasicConsumer(_connection.Channel);

        consumer.ReceivedAsync += async (model, ea) =>
        {
            byte[] body = ea.Body.ToArray();
            string json = Encoding.UTF8.GetString(body);

            SubmissionProcessingContract? message = null;
            try
            {
                message = JsonSerializer.Deserialize<SubmissionProcessingContract>(json) 
                        ?? throw new JsonConversionException();

                _logger.LogInformation("De-queuing MessageId={MessageId} from queue '{Queue}'", message.MessageId, QueueName);

                await ProcessSubmissionInternalAsync(message, stoppingToken);

                await _connection.Channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag, 
                    multiple: false, 
                    cancellationToken: stoppingToken
                );
                
                _logger.LogInformation("Successfully completed and acknowledged for MessageId={MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while processing asynchronous task thread.");
                await HandleFailureAsync(ea, message, json, ex, stoppingToken);
            }
        };

        await _connection.Channel.BasicConsumeAsync(
            queue: AppConstants.RabbitMQ.GetQueue(QueueName), 
            autoAck: false,
            consumer: consumer, 
            cancellationToken: stoppingToken
        );
    }

    private async Task ProcessSubmissionInternalAsync(SubmissionProcessingContract message, CancellationToken stoppingToken)
    {
        IServiceScope scope = _scopeFactory.CreateScope();
        AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        ProcessingJob? existingJob = await dbContext.ProcessingJobs
            .FirstOrDefaultAsync(j => j.Id == message.ProcessingJobId, stoppingToken);

        if (existingJob?.Status == ProcessingJobStatus.Completed) 
        {
            _logger.LogWarning("MessageId={MessageId} already processed.", message.MessageId);
            return;
        }

        existingJob!.Status = ProcessingJobStatus.Processing;
        existingJob.StartedAt = DateTime.UtcNow;
        existingJob.Attempts++;
        
        await dbContext.SaveChangesAsync(stoppingToken);

        _logger.LogInformation("TaskAssignment={TaskAssignmentId} status map to Submitted...", message.TaskAssignmentId);
        TaskAssignment assignment = await dbContext.TaskAssignments
            .FirstOrDefaultAsync(a => a.Id == message.TaskAssignmentId, stoppingToken) 
            ?? throw new NotFoundException("TaskAssignment Reference Identity Exception");

        _logger.LogInformation("Evaluating file storage deduplication for SubmissionFileId={FileId}...", message.SubmissionFileId);
        SubmissionFile newSubmissionFile = await dbContext.SubmissionFiles
            .FirstOrDefaultAsync(sf => sf.Id == message.SubmissionFileId, stoppingToken)
            ?? throw new NotFoundException("SubmissionFile Reference Error");

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
                _logger.LogInformation("Deduplication Success: Re-mapped file pointer storage link targets to match physical file trace: '{FileName}'", file.StorageFileName);
                break;
            }
        }

        existingJob.Status = ProcessingJobStatus.Completed; 
        existingJob.CompletedAt = DateTime.UtcNow;
        
        await dbContext.SaveChangesAsync(stoppingToken);

        await _cacheService.RemoveAsync($"task-assignment:{assignment.Id}");
    }

    private async Task HandleFailureAsync(
        BasicDeliverEventArgs ea, 
        SubmissionProcessingContract? message, 
        string json, 
        Exception exception, 
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
                        _logger.LogError("MessageId={MessageId} permanently as FAILED.", message.MessageId);
                    }
                    else
                    {
                        job.Status = ProcessingJobStatus.Queued; 
                        _logger.LogWarning("Transient Failure: MessageId={MessageId} returning back to queue.", message.MessageId);
                    }
                    
                    await dbContext.SaveChangesAsync(stoppingToken);
                }
            }

            if (isPermanentFailure)
            {
                await _connection.Channel!.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag, 
                    multiple: false, 
                    requeue: false, 
                    cancellationToken: stoppingToken
                );
            }
            else
            {
                await _connection.Channel!.BasicNackAsync(
                    deliveryTag: ea.DeliveryTag, 
                    multiple: false, 
                    requeue: true, 
                    cancellationToken: stoppingToken
                );
            }
        }
        catch (Exception criticalDbException)
        {
            _logger.LogCritical(criticalDbException, "Fatal Crash Error: Critical infrastructure database failure tracking boundary collapsed.");
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Background worker shutting down.");
        
        if (_connection.Channel is not null)
        {
            await _connection.Channel.DisposeAsync();
        }

        await base.StopAsync(cancellationToken);
    }
}