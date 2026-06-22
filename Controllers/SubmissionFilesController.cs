using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.FileStorage.ServiceInterface;
using TraineeManagementApi.SubmissionFiles.Models;
using TraineeManagementApi.Submissions.Models;
using TraineeManagementApi.Utils.ResponsesBuilder;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Constants;
using TraineeManagementApi.SubmissionFiles.DTOs;
using TraineeManagementApi.SubmissionFiles.ServiceInterface;
using TraineeManagementApi.FileStorage.Configurations;
using Microsoft.Extensions.Options;

namespace TraineeManagementApi.SubmissionFiles.Controller;

[ApiController]
[Route(AppConstants.Routes.SubmissionFiles)]
[Authorize]
public class SubmissionFilesController : ControllerBase
{
    private readonly ILogger<SubmissionFilesController> _logger;

    private readonly ISubmissionFileService _submissionFileService;

    private readonly IFileStorageService _fileStorageService;

    private readonly FileStorageConfiguration _fileConfiguration;

    private readonly AppDbContext _context;

    public SubmissionFilesController(ILogger<SubmissionFilesController> logger, IFileStorageService fileStorageService, AppDbContext context, ISubmissionFileService submissionFileService, IOptions<FileStorageConfiguration> fileConfiguration)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
        _context = context;
        _submissionFileService = submissionFileService;
        _fileConfiguration = fileConfiguration.Value;
    }

    [HttpPost("{submissionId}/files")]
    public async Task<IActionResult> UploadFile(int submissionId, IFormFile file)
    {
        if (!ModelState.IsValid || submissionId < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse();
        }
        
        if (file == null)
        {
            _logger.LogWarning("Upload attempt blocked: No files found in the multipart form-data payload request.");
            throw new BadRequestException("No files were attached to the upload request.");
        }

        Submission? submission = await _context.Submissions.FindAsync(submissionId);
        if (submission == null)
        {
            throw new BadRequestException("The referenced submission profile does not exist.");
        }
            
        using Stream readStream = file.OpenReadStream();
        string? storedName = await _fileStorageService.SaveAsync(submissionId,file);

        if(storedName == null)
        {
            throw new BadRequestException("This file is already uploaded");
        }
        SubmissionFileResponseDto submissionFile = await _submissionFileService.AddSubmissionFileMetaDataAsync(submissionId,file,storedName);
        
        return ResponseBuilder.CreateSuccessResponse(
            AppConstants.ApiResponse.Created,
            submissionFile
        );        
    }

    [HttpGet("{id}/download")] 
    public async Task<IActionResult> DownloadFile(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse();
        }

        SubmissionFile? metadata = await _context.SubmissionFiles.FindAsync(id);
        if (metadata == null)
        {
            throw new ExceptionFileNotFound();
        }

        Stream stream = await _fileStorageService.OpenReadAsync(metadata.StorageFileName);
        return File(stream, metadata.ContentType, metadata.OriginalFileName);
    }

    [HttpDelete("{id}")] 
    public async Task<IActionResult> DeleteFile(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse();
        }

        SubmissionFile? metadata = await _context.SubmissionFiles.FindAsync(id);
        if (metadata == null)
        {
            throw new ExceptionFileNotFound();
        }

        _context.SubmissionFiles.Remove(metadata);
        await _context.SaveChangesAsync();

        await _fileStorageService.DeleteAsync(metadata.StorageFileName);
        
        return ResponseBuilder.CreateSuccessResponse(
            AppConstants.ApiResponse.NoContent
        );
    }
}