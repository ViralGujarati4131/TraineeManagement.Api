using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagement.Api.FileStorageServiceInterface;
using TraineeManagement.Api.ResponsesBuilder;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.ConstRoute;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.Data.SubmissionFileDTO;
using TraineeManagement.Api.SubmissionFileServiceInterface;
using TraineeManagement.Api.Contract.SubmissionProcessingContarct;
using TraineeManagement.Api.Data.DatabaseContext;

namespace TraineeManagement.Api.SubmissionFileController;

[ApiController]
[Route(CustomConstRoute.SubmissionFiles)]
[Authorize]
public class SubmissionFilesController : ControllerBase
{
    private readonly ILogger<SubmissionFilesController> _logger;

    private readonly ISubmissionFileService _submissionFileService;

    private readonly IFileStorageService _fileStorageService;

    private readonly AppDbContext _context;

    public SubmissionFilesController(ILogger<SubmissionFilesController> logger, IFileStorageService fileStorageService, AppDbContext context, ISubmissionFileService submissionFileService)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
        _context = context;
        _submissionFileService = submissionFileService;
    }

    [HttpPost("/api/submission/{submissionId}/files")]
    public async Task<IActionResult> UploadFile(int submissionId, IFormFile file)
    {
        if (!ModelState.IsValid || submissionId < 1)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }
        
        if (file == null || file.Length == 0)
        {
            _logger.LogWarning("Upload attempt blocked: No files found in the multipart form-data payload request.");
            throw new BadRequestException("No files were attached to the upload request.");
        }

        bool isDuplicates = await _fileStorageService.Exists(submissionId,file);
        if(isDuplicates)
        {
            throw new BadRequestException("This file is already uploaded");
        }
        
        string storedName = await _fileStorageService.SaveAsync(submissionId,file);
        
        SubmissionFileResponseDto submissionFile = await _submissionFileService.AddSubmissionFileMetaDataAsync(submissionId,file,storedName);

        try
        {
            SubmissionPublishResult publishResult = await _submissionFileService.RequestProcessing(submissionId,submissionFile.Id);

            _logger.LogInformation(
                "Published SubmissionProcessingRequested: MessageId={MessageId}, CorrelationId={CorrelationId}, SubmissionId={SubmissionId}, Result={Result}",
                publishResult.MessageId,
                publishResult.CorrelationId,
                submissionId,
                publishResult.Success ? "Success" : "Failed"
            );

            if (!publishResult.Success)
                return StatusCode(503, "Submission saved but could not be queued for processing.");

            return CustomResponseBuilder.CreateSuccessResponse(
                CustomResponse.Successlly_Uploaded,
                publishResult.ProcessingJobId
            ); 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish submission {SubmissionId} to RabbitMQ", submissionId);
            return StatusCode(503, "Submission saved but could not be queued for processing.");
        }       
    }

    [HttpGet("{id}/download")] 
    public async Task<ActionResult> DownloadFile(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }
        var (fileStream, contentType, originalFileName) = await _fileStorageService.OpenAsync(id);
        
        return File(fileStream, contentType, originalFileName);
    }

    [HttpDelete("{id}")] 
    public async Task<IActionResult> DeleteFile(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }

        bool isDeleted = await _fileStorageService.DeleteAsync(id);
        if (!isDeleted)
        {
            throw new BadRequestException("File could not be removed or found.");
        }
        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.NoContent
        );
    }
}