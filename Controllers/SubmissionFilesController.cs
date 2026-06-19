using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.FileStorage.ServiceInterface;
using TraineeManagementApi.Utils.ResponsesBuilder;
using TraineeManagementApi.Constants;
using TraineeManagementApi.Utils.CustomException;

namespace TraineeManagementApi.SubmissionFiles.Cotroller;

[ApiController]
[Route("api/submission-files")]
[Authorize]
public class SubmissionFilesController : ControllerBase
{
    private readonly ILogger<SubmissionFilesController> _logger;

    private readonly IFileStorageService _fileStorageService;

    private readonly long _maxFileSize = 10 * 1024 * 1024;

    private readonly string[] _allowedExtensions = { ".pdf", ".docx", ".png", ".jpg", ".txt", ".zip"};

    public SubmissionFilesController(ILogger<SubmissionFilesController> logger,IFileStorageService fileStorageService)
    {
        _logger = logger;
        _fileStorageService = fileStorageService;
    }

    
    [HttpPost("{submissionId}/files")]
    public async Task<IActionResult> UploadFile(int submissionId, List<IFormFile> files)
    {
        foreach(var file in files)
        {   
            if (file == null || file.Length == 0)
                return BadRequest("Empty file is not allowed.");

            if (file.Length > _maxFileSize)
                return BadRequest($"File exceeds the {_maxFileSize / (1024 * 1024)} MB limit.");

            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedExtensions.Contains(ext))
                return BadRequest("File type not allowed.");
     
            string storageName;
            using (Stream stream = file.OpenReadStream())
            {
                storageName = await _fileStorageService.uploadFileAsync(submissionId,stream, file.FileName, file.ContentType);
            }
        }

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            "viral"
        );        
    }

    [HttpGet("{id}/download")]
    public async Task<IActionResult> DownloadFile(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        Stream stream = await _fileStorageService.downloadFileAsync(id);

        return File(stream, "application/octet-stream");
        // return ResponseBuilder.CreateSuccessResponse(
        //     StatusCodes.Status200OK,
        //     AppConstants.ApiResponse.CodeSuccess,
        //     AppConstants.ApiResponse.MsgSuccess,
        //     File(stream, "application/octet-stream")
        // ); 
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFile(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        await _fileStorageService.deleteFileAsync(id);
        
        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status204NoContent,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgDeleted
        );
    }
}

