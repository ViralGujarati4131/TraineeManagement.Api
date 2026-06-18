using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.TaskAssignments.DTOs;
using TraineeManagementApi.TaskAssignments.ServiceInterface;
using TraineeManagementApi.ResponsesBuilder;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.TaskAssignments.Controller;

[ApiController]
[Route(AppConstants.Routes.TaskAssignments)]
[Authorize]
public class TaskAssignmentsController : ControllerBase
{
    private readonly ITaskAssignmentService _taskAssignmentService;
    private readonly ILogger<TaskAssignmentsController> _logger;

    public TaskAssignmentsController(ITaskAssignmentService taskAssignmentService, ILogger<TaskAssignmentsController> logger)
    {
        _taskAssignmentService = taskAssignmentService;
        _logger = logger;
    }

    [HttpPost]
    public async Task<ActionResult> CreateTaskAssignment([FromBody] TaskAssignmentCreateDto createTaskAssignmentDto)
    {
        if (!ModelState.IsValid)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking task-assignment service to add a new task-assignment");

        TaskAssignmentResponseDto createdTaskAssignment = await _taskAssignmentService.CreateTaskAssignmentAsync(createTaskAssignmentDto);
        
        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeCreated,
            AppConstants.ApiResponse.MsgCreated,
            createdTaskAssignment
        );
    }

    [HttpGet]
    public async Task<ActionResult> GetTaskAssignments()
    {
        _logger.LogDebug("Invoking task-assignment service to fetch all task-assignments");

        IEnumerable<TaskAssignmentResponseDto> taskAssignments = await _taskAssignmentService.GetTaskAssignmentsAsync();

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            taskAssignments
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetTaskAssignmentById(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking task-assignment service to retrieve assignments for AssignmentId: {AssignmentId}", id);

        TaskAssignmentResponseDto taskAssignment = await _taskAssignmentService.GetTaskAssignmentByIdAsync(id);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            taskAssignment
        );
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult> UpdateTaskAssignmentById(int id, [FromBody] TaskAssignmentUpdateDto updateTaskAssignmentDto)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking task-assignment service to modify records for AssignmentId: {AssignmentId}", id);
        
        TaskAssignmentResponseDto updatedTaskAssignment = await _taskAssignmentService.UpdateTaskAssignmentAsync(id, updateTaskAssignmentDto);
        
        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgUpdated,
            updatedTaskAssignment
        );
    }
}