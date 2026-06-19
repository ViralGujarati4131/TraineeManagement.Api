using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.LearningTasks.DTOs;
using TraineeManagementApi.LearningTasks.ServiceInterface;
using TraineeManagementApi.Utils.ResponsesBuilder;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.LearningTasks.Controller;

[ApiController]
[Route(AppConstants.Routes.LearningTasks)]
[Authorize]
public class LearningTasksController : ControllerBase
{
    private readonly ILearningTaskService _learningTaskService;
    private readonly ILogger<LearningTasksController> _logger;

    public LearningTasksController(ILearningTaskService learningTaskService, ILogger<LearningTasksController> logger)
    {
        _learningTaskService = learningTaskService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> GetLearningTasks()
    {
        _logger.LogDebug("Invoking learning-task service to fetch all tasks");

        IEnumerable<LearningTaskResposeDto> learningTasks = await _learningTaskService.GetLearningTaskAsync();

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            learningTasks
        );
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetLearningTaskById(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking learning-task service to retrieve for TaskId: {TaskId}", id);

        LearningTaskResposeDto learningTask = await _learningTaskService.GetLearningTaskByIdAsync(id);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            learningTask
        );
    }

    [HttpPost]
    public async Task<ActionResult> CreateLearningTask([FromBody] LearningTaskCreateDto createTaskDto)
    {
        if (!ModelState.IsValid)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking learning-task service to add a new task");

        LearningTaskResposeDto createdTask = await _learningTaskService.CreateLearningTaskAsync(createTaskDto);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK, 
            AppConstants.ApiResponse.CodeCreated, 
            AppConstants.ApiResponse.MsgCreated, 
            createdTask
        );
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteLearningTask(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking learning-task service to delete task for TaskId: {TaskId}", id);

        await _learningTaskService.DeleteLearningTaskByIdAsync(id);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status204NoContent,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgDeleted
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateLearningTask(int id, [FromBody] LearningTaskUpdateDto updateTaskDto)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogDebug("Invoking learning-task service to modify task for TaskId: {TaskId}", id);

        LearningTaskResposeDto updatedTask = await _learningTaskService.UpdateLearningTaskByIdAsync(id, updateTaskDto);

        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK, 
            AppConstants.ApiResponse.CodeSuccess, 
            AppConstants.ApiResponse.MsgUpdated, 
            updatedTask
        );
    }
}