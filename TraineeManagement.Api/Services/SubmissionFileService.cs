using TraineeManagement.Api.Data.SubmissionFileDTO;
using TraineeManagement.Api.SubmissionFileServiceInterface;
using System.Security.Cryptography;
using TraineeManagement.Api.Data.SubmissionFileModel;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Contract.SubmissionProcessingContarct;
using TraineeManagement.Api.Messaging.RabbitMQPublisher;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.ProcessingJobModel;

namespace TraineeManagement.Api.SubmissionFileService;

public class SubmissionFileService : ISubmissionFileService
{
    private readonly AppDbContext _context;
    
    private readonly ILogger<SubmissionFileService> _logger;

    private readonly IHttpContextAccessor _httpContextAccessor;

    private readonly RabbitMqService _rabbitMqService;

    public SubmissionFileService(AppDbContext context, ILogger<SubmissionFileService> logger,IHttpContextAccessor httpContextAccessor,RabbitMqService rabbitMqService)
    {
        _context = context;
        _logger = logger;
        _httpContextAccessor = httpContextAccessor;
        _rabbitMqService = rabbitMqService;
    }

    public async Task<SubmissionPublishResult> RequestProcessing(int submissionId, int submissionFileId)
    {
         ProcessingJob processingJob = new ProcessingJob{
                MessageId = Guid.NewGuid(),
                CorrelationId = Guid.NewGuid(),
                SubmissionId = submissionId,
                SubmissionFileId = submissionFileId,
                Status = ProcessingJobStatus.Queued, 
                Attempts = 0,
                RequestedAt = DateTime.UtcNow
            };
        _context.ProcessingJobs.Add(processingJob);
        await _context.SaveChangesAsync();

        SubmissionProcessingContract message = new SubmissionProcessingContract
        (
            ProcessingJobId: processingJob.Id,
            MessageId: processingJob.MessageId,
            CorrelationId: processingJob.CorrelationId,
            TaskAssignmentId: processingJob.SubmissionId,
            SubmissionFileId: processingJob.SubmissionFileId,
            RequestedAt: DateTimeOffset.UtcNow,
            ContractVersion: "1.0"
        );
        try
        {
            await _rabbitMqService.PublishAsync(message);
            return new SubmissionPublishResult
            (
                processingJob.Id,
                message.MessageId,
                message.CorrelationId,
                true
            );
        }
        catch
        {
            _context.ProcessingJobs.Remove(processingJob);
            return new SubmissionPublishResult
            (
                0,
                message.MessageId,
                message.CorrelationId,
                false
            );
        }
    }

    public async Task<SubmissionFileResponseDto> AddSubmissionFileMetaDataAsync(int submissionId,IFormFile file,string storedName)
    {
        string? userId = _httpContextAccessor.HttpContext?.User?.FindFirst(AppConstants.Security.ClaimId)?.Value;
        
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogError("Access Denied: Attempted to upload file metadata without a valid User ID claim in the bearer token.");
        
            throw new UnauthorizedAccessException("User identification claim is missing or invalid.");
        }
        
        string checksum;
        using (SHA256 sha256 = SHA256.Create())
        {
            using Stream readStream = file.OpenReadStream();
            byte[] hashBytes = await sha256.ComputeHashAsync(readStream);
            checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();    
        } 

        SubmissionFile submissionFile = new SubmissionFile
        {
            SubmissionId = submissionId,
            OriginalFileName = file.FileName,
            StorageFileName = storedName,
            ContentType = file.ContentType,
            Size = file.Length,
            Checksum = checksum,
            UploadedByUserId = userId
        };
         _context.SubmissionFiles.Add(submissionFile);
        await _context.SaveChangesAsync();

        return new SubmissionFileResponseDto(
                submissionFile.Id,
                submissionFile.SubmissionId,
                submissionFile.OriginalFileName,
                submissionFile.ContentType,
                submissionFile.Size,
                submissionFile.UploadedByUserId
        );
    }
}