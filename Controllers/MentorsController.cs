using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.Mentors.DTOs;
using TraineeManagementApi.Mentors.ServiceInterface;
using TraineeManagementApi.ResponsesBuilder;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Mentors.Controller;

[ApiController]
[Route(AppConstants.Routes.Mentors)]
[Authorize]
public class MentorController : ControllerBase
{
    private readonly IMentorServices _mentorService;
    private readonly ILogger<MentorController> _logger;

    public MentorController(IMentorServices mentorService, ILogger<MentorController> logger)
    {
        _mentorService = mentorService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetMentors()
    {
        _logger.LogDebug("Invoking mentor service to fetch all mentors");

        IEnumerable<MentorResponseDto> mentors = await _mentorService.GetMentorsAsync();

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            mentors
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetMentorById(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking mentor service to retrieve profile for MentorId: {MentorId}", id);

        MentorResponseDto mentor = await _mentorService.GetMentorByIdAsync(id);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            mentor
        );
    }

    [HttpPost]
    public async Task<ActionResult> CreateMentor([FromBody] MentorCreateDto createMentorDto)
    {
        if (!ModelState.IsValid)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking mentor service to establish a new mentor registration");

        MentorResponseDto createdMentor = await _mentorService.CreateMentorAsync(createMentorDto);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeCreated,
            AppConstants.ApiResponse.MsgCreated,
            createdMentor
        );
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMentorById(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking mentor service to delete record for MentorId: {MentorId}", id);

        await _mentorService.DeleteMentorByIdAsync(id);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status204NoContent,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgDeleted
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateMentorById(int id, [FromBody] MentorUpdateDto updateMentorDto)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking mentor service to modify records for MentorId: {MentorId}", id);

        MentorResponseDto updatedMentor = await _mentorService.UpdateMentorByIdAsync(id, updateMentorDto);
        
        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgUpdated,
            updatedMentor
        );
    }
}