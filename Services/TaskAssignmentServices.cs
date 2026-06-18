using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.TaskAssignments.ServiceInterface;
using TraineeManagementApi.TaskAssignments.DTOs;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.TaskAssignments.Service;

public class TaskAssignmentService : ITaskAssignmentService
{
    private readonly AppDbContext _context;

    private readonly ILogger<TaskAssignmentService> _logger;

    public TaskAssignmentService(AppDbContext context, ILogger<TaskAssignmentService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private TaskAssignmentResponseDto MapToResponseDto(TaskAssignment taskAssignment)
    {
        return new TaskAssignmentResponseDto
        (
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

        _logger.LogInformation("Successfully created new taskAssignment with ID {AssingmentId}", taskAssignment.Id);

        return MapToResponseDto(taskAssignment);
    }

    public async Task<IEnumerable<TaskAssignmentResponseDto>> GetTaskAssignmentsAsync()
    {
        _logger.LogDebug("Fetching all taskAssignments from the database");
        
        IEnumerable<TaskAssignmentResponseDto> taskAssignments = await _context.TaskAssignments
                                                                             .Select(ta => new 
                                                                                TaskAssignmentResponseDto
                                                                                (
                                                                                   ta.Id,
                                                                                   ta.TraineeId,
                                                                                   ta.MentorId,
                                                                                   ta.LearningTaskId,
                                                                                   ta.AssignedDate,
                                                                                   ta.DueDate,
                                                                                   ta.Status,
                                                                                   ta.Remarks
                                                                                )
                                                                             ).ToListAsync();
        return taskAssignments;
    }

    public async Task<TaskAssignmentResponseDto> GetTaskAssignmentByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving taskAssignment with ID: {Assignmentid}", id);

        TaskAssignmentResponseDto? taskAssignment = await _context.TaskAssignments
                                                                .Where(ta => ta.Id == id)
                                                                .Select(ta => new 
                                                                    TaskAssignmentResponseDto
                                                                    (
                                                                        ta.Id,
                                                                        ta.TraineeId,
                                                                        ta.MentorId,
                                                                        ta.LearningTaskId,
                                                                        ta.AssignedDate,
                                                                        ta.DueDate,
                                                                        ta.Status,
                                                                        ta.Remarks
                                                                    )
                                                                ).FirstOrDefaultAsync();
        if(taskAssignment == null)
        {
            _logger.LogWarning("TaskAssignment with ID {AssignmentId} was not found", id);

            throw new NotFoundException("TaskAssignment");
        }
        return taskAssignment;
    }

    public async Task<TaskAssignmentResponseDto> UpdateTaskAssignmentAsync(int id, TaskAssignmentUpdateDto updateTaskAssignmentDto)
    {
        _logger.LogDebug("Locating taskAssignment with ID {Assignmentid} for modification status", id);

        TaskAssignment taskAssignment = await FetchTaskAssignmentByIdInternalAsync(id);
        taskAssignment.Status = updateTaskAssignmentDto.Status;
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully updated taskAssignment for ID {Assignmentid}", id);

        return MapToResponseDto(taskAssignment);
    }
}