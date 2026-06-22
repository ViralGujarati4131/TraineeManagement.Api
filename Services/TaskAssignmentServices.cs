using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.TaskAssignments.ServiceInterface;
using TraineeManagementApi.TaskAssignments.DTOs;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.RedisCaching.ServiceInterface;

namespace TraineeManagementApi.TaskAssignments.Service;

public class TaskAssignmentService : ITaskAssignmentService
{
    private readonly AppDbContext _context;

    private readonly ILogger<TaskAssignmentService> _logger;

    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);


    public TaskAssignmentService(AppDbContext context, ILogger<TaskAssignmentService> logger,ICacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    private TaskAssignmentResponseDto MapToResponseDto(TaskAssignment taskAssignment)
    {
        return new TaskAssignmentResponseDto(
            taskAssignment.Id,
            taskAssignment.TraineeId,
            taskAssignment.MentorId,
            taskAssignment.LearningTaskId,
            taskAssignment.AssignedDate,
            taskAssignment.DueDate,
            taskAssignment.Status,
            taskAssignment.Remarks
        );
    }

    private async Task<TaskAssignment> FetchTaskAssignmentByIdInternalAsync(int id)
    {
        TaskAssignment? taskAssignment = await _context.TaskAssignments.FindAsync(id);
        if (taskAssignment == null)
        {
            _logger.LogWarning("TaskAssignment with ID {AssignmentId} was not found", id);
            throw new NotFoundException("TaskAssignment");
        }
        return taskAssignment;
    }

    public async Task<TaskAssignmentResponseDto> CreateTaskAssignmentAsync(TaskAssignmentCreateDto createTaskAssignmentDto)
    {
        TaskAssignment taskAssignment = new TaskAssignment
        {
            TraineeId = createTaskAssignmentDto.TraineeId,
            MentorId = createTaskAssignmentDto.MentorId,
            LearningTaskId = createTaskAssignmentDto.LearningTaskId,
            AssignedDate = createTaskAssignmentDto.AssignedDate,
            DueDate = createTaskAssignmentDto.DueDate,
            Status = createTaskAssignmentDto.Status,
            Remarks = createTaskAssignmentDto.Remarks
        };
        
        _context.TaskAssignments.Add(taskAssignment);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created new taskAssignment with ID {AssignmentId}", taskAssignment.Id);

        await _cacheService.RemoveAsync("TaskAssignments:All");

        return MapToResponseDto(taskAssignment);
    }

    public async Task<IEnumerable<TaskAssignmentResponseDto>> GetTaskAssignmentsAsync()
    {
        _logger.LogDebug("Fetching all taskAssignments from the database");

        string cacheKey = "TaskAssignments:All";
        IEnumerable<TaskAssignmentResponseDto>? cached = await _cacheService.GetAsync<IEnumerable<TaskAssignmentResponseDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning taskAssignments from cache");
            return cached;
        }
        
        IEnumerable<TaskAssignmentResponseDto> taskAssignmentResponses = await _context.TaskAssignments
            .AsNoTracking()
            .Select(ta => new TaskAssignmentResponseDto(
                ta.Id,
                ta.TraineeId,
                ta.MentorId,
                ta.LearningTaskId,
                ta.AssignedDate,
                ta.DueDate,
                ta.Status,
                ta.Remarks
            )).ToListAsync();

        await _cacheService.SetAsync(cacheKey,taskAssignmentResponses,CacheTtl);

        return taskAssignmentResponses;
    }

    public async Task<TaskAssignmentResponseDto> GetTaskAssignmentByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving taskAssignment with ID: {AssignmentId}", id);

        string cacheKey = $"TaskAssignment:{id}";
        TaskAssignmentResponseDto? cached = await _cacheService.GetAsync<TaskAssignmentResponseDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning taskAssignment from cache");
            return cached;
        }

        TaskAssignmentResponseDto? dto = await _context.TaskAssignments
            .AsNoTracking()
            .Where(ta => ta.Id == id)
            .Select(ta => new TaskAssignmentResponseDto(
                ta.Id,
                ta.TraineeId,
                ta.MentorId,
                ta.LearningTaskId,
                ta.AssignedDate,
                ta.DueDate,
                ta.Status,
                ta.Remarks
            )).FirstOrDefaultAsync();

        if (dto == null)
        {
            _logger.LogWarning("TaskAssignment with ID {AssignmentId} was not found during target DTO projection.", id);
            throw new NotFoundException("TaskAssignment");
        }
        await _cacheService.SetAsync(cacheKey,dto,CacheTtl);
        
        return dto;
    }

    public async Task<TaskAssignmentResponseDto> UpdateTaskAssignmentAsync(int id, TaskAssignmentUpdateDto updateTaskAssignmentDto)
    {
        _logger.LogDebug("Locating taskAssignment with ID {AssignmentId} for status modification", id);

        TaskAssignment taskAssignment = await FetchTaskAssignmentByIdInternalAsync(id);
        
        taskAssignment.Status = updateTaskAssignmentDto.Status;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully updated taskAssignment for ID {AssignmentId}", id);

        await _cacheService.RemoveAsync("TaskAssignments:All");
        await _cacheService.RemoveAsync($"TaskAssignment:{id}");
    
        return MapToResponseDto(taskAssignment);
    }
}