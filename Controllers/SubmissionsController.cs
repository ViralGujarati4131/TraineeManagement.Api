using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.Submissions.DTOs;
using TraineeManagementApi.Submissions.ServiceInterface;
using TraineeManagementApi.ResponsesBuilder;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Submissions.Cotroller;

[ApiController]
[Route(AppConstants.Routes.Submissions)]
[Authorize]
public class SubmissionsController : ControllerBase
{
    private readonly ISubmissionService _submissionService;
    private readonly ILogger<SubmissionsController> _logger;

    public SubmissionsController(ISubmissionService submissionService, ILogger<SubmissionsController> logger)
    {
        _submissionService = submissionService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> CreateSubmission([FromBody] SubmissionCreateDto createSubmissionDto)
    {
        if (!ModelState.IsValid)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking submission service to add a new submission");

        SubmissionResponseDto createdSubmission = await _submissionService.CreateSubmissionAsync(createSubmissionDto);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeCreated,
            AppConstants.ApiResponse.MsgCreated,
            createdSubmission
        );
    }

    [HttpGet]
    public async Task<ActionResult> GetSubmissions()
    {
        _logger.LogDebug("Invoking submission service to fetch all submissions");

        IEnumerable<SubmissionResponseDto> submissions = await _submissionService.GetSubmissionsAsync();

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            submissions
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetSubmissionById(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking submission service to retrieve submission for SubmissionId: {SubmissionId}", id);

        SubmissionResponseDto submission = await _submissionService.GetSubmissionByIdAsync(id);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            submission
        );
    }
}