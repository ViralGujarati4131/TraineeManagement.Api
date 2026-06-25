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
using TraineeManagement.Api.Data.ProcessingJobModel;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.SubmissionFileModel;
using TraineeManagement.Api.FileStoreValidation;
using Microsoft.Extensions.Options;

namespace TraineeManagement.Api.Worker.SubmissionProcessingConsumer;

public class SubmissionProcessingConsumer : BackgroundService
{
    private readonly ILogger<SubmissionProcessingConsumer> _logger;

    private readonly string _rootPath;

     private readonly CustomFileStoreValidation _fileConfiguration;

    private readonly RabbitConnection _connection;  

    private readonly IServiceScopeFactory _scopeFactory;

    private readonly ICacheService _cacheService;
    
    private const int MaxRetryAttempts = 3;

    public SubmissionProcessingConsumer(ILogger<SubmissionProcessingConsumer> logger, RabbitConnection connection, IServiceScopeFactory scopeFactory, ICacheService cacheService,IOptions<CustomFileStoreValidation> fileConfiguration)
    {
        _logger = logger;
        _connection = connection;
        _scopeFactory = scopeFactory;
        _cacheService = cacheService;
        _fileConfiguration = fileConfiguration.Value;

         if (string.IsNullOrWhiteSpace(_fileConfiguration.RootPath))
        {
            throw new FileStorageConfigurationException();
        }
            
        string configuredPath = _fileConfiguration.RootPath; 
        string basePath = _fileConfiguration.BasePath;

        _rootPath = Path.Combine(basePath, configuredPath);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await _connection.RegisterQueueAsync(AppConstants.RabbitMQ.SubmissionProcessing);

        if (_connection.Channel is null)
        {
            _logger.LogError("RabbitMQ channel failed to initialize.");
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
                message = JsonSerializer.Deserialize<SubmissionProcessingContract>(json);
                if (message == null) 
                    throw new JsonConversionException();

                _logger.LogInformation("De-queuing MessageId={MessageId} for validation...", message.MessageId);

                using (IServiceScope scope = _scopeFactory.CreateScope())
                {
                    AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    ProcessingJob? existingJob = await dbContext.ProcessingJobs.FirstOrDefaultAsync(j => j.Id == message.ProcessingJobId);

                    if (existingJob != null && existingJob.Status == ProcessingJobStatus.Completed) 
                    {
                        _logger.LogWarning("Alert: MessageId={MessageId} has already been processed.", message.MessageId);
                        await _connection.Channel.BasicAckAsync(
                            ea.DeliveryTag, 
                            multiple: false, 
                            stoppingToken
                        );
                        return;
                    }

                    if (existingJob != null && existingJob.Status == ProcessingJobStatus.Queued)
                    {
                        existingJob.Status = ProcessingJobStatus.Processing;
                        existingJob.StartedAt = DateTime.UtcNow;
                        existingJob.Attempts++;
                    }
                    await dbContext.SaveChangesAsync(stoppingToken);

                    _logger.LogInformation("Executing background task for TaskAssignment={TaskAssignmentId}...", message.TaskAssignmentId);

                    TaskAssignment? assignment = await dbContext.TaskAssignments.FirstOrDefaultAsync(a => a.Id == message.TaskAssignmentId, stoppingToken);
                    
                    if (assignment == null)
                    {
                        throw new NotFoundException("TaskAssignment Referance");
                    }

                    assignment.Status = TaskAssignmentStatus.Submitted;
                    
                    _logger.LogInformation("Executing background task for SubmissionFileId={SubmissionId}...", message.SubmissionFileId);

                    SubmissionFile? newSubmissionFile = await dbContext.SubmissionFiles
                        .Where(sf => sf.Id == message.SubmissionFileId)
                        .FirstOrDefaultAsync();
                    
                    IEnumerable<SubmissionFile?> submissionFile = await dbContext.SubmissionFiles
                        .Where(sf => sf.Checksum == newSubmissionFile!.Checksum && sf.Id != newSubmissionFile.Id)
                        .ToListAsync();

                    foreach(SubmissionFile? file in submissionFile)
                    {
                        string filePath = Path.Combine(_rootPath, file!.StorageFileName);
                        if(File.Exists(filePath))
                        {
                            filePath = Path.Combine(_rootPath,newSubmissionFile!.StorageFileName);
                            File.Delete(filePath);
                            newSubmissionFile!.StorageFileName = file!.StorageFileName;
                            break;
                        }
                    }

                    existingJob!.Status = ProcessingJobStatus.Completed; 
                    existingJob.CompletedAt = DateTime.UtcNow;
                    
                    await dbContext.SaveChangesAsync(stoppingToken);

                    await _cacheService.RemoveAsync($"task-assignment:{assignment.Id}");
                }
                await _connection.Channel.BasicAckAsync(
                    deliveryTag: ea.DeliveryTag, 
                    multiple: false, 
                    cancellationToken: stoppingToken
                );
                _logger.LogInformation("Successfully completed and acknowledged for MessageId={MessageId}", message.MessageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception thrown while executing job processing.");
                await HandleFailureAsync(ea, message, json, ex, stoppingToken);
            }
        };
        await _connection.Channel.BasicConsumeAsync(
            queue: AppConstants.RabbitMQ.SubmissionProcessing, 
            autoAck: false, 
            consumer: consumer, 
            cancellationToken: stoppingToken
        );
    }

    private async Task HandleFailureAsync(BasicDeliverEventArgs ea, SubmissionProcessingContract? message, string json, Exception exception, CancellationToken stoppingToken)
    {
        try
        {
            using (IServiceScope scope = _scopeFactory.CreateScope())
            {
                AppDbContext dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                bool isPermanentFailure = exception is JsonException || exception is KeyNotFoundException;

                if (message != null)
                {
                    ProcessingJob? job = await dbContext.ProcessingJobs.FirstOrDefaultAsync(j => j.Id == message.ProcessingJobId, stoppingToken);
                    
                    if (job != null)
                    {
                        job.ErrorSummary = exception.Message;
                    
                        if (isPermanentFailure || job.Attempts >= MaxRetryAttempts)
                        {
                            job.Status = ProcessingJobStatus.Failed; 
                            job.CompletedAt = DateTime.UtcNow;
                            _logger.LogError("Job Failure: MessageId={MessageId} FAILED permanently.", message.MessageId);
                        }
                        else
                        {
                            job.Status = ProcessingJobStatus.Queued; 
                            _logger.LogWarning("Transient Failure: MessageId={MessageId} scheduled for retry.", message.MessageId);
                        }
                        
                        await dbContext.SaveChangesAsync(stoppingToken);
                    }
                }

                if (isPermanentFailure)
                {
                    await _connection.Channel!.BasicNackAsync(
                        ea.DeliveryTag, 
                        multiple: false, 
                        requeue: false, 
                        cancellationToken: stoppingToken
                    );
                }
                else
                {
                    await _connection.Channel!.BasicNackAsync(
                        ea.DeliveryTag, 
                        multiple: false, 
                        requeue: true, 
                        cancellationToken: stoppingToken
                    );
                }
            }
        }
        catch (Exception criticalDbException)
        {
            _logger.LogCritical(criticalDbException, "processing handler fault occur.");
        }
    }
}