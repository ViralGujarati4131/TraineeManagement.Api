using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.LearningTasks.DTOs;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.LearningTasks.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.LearningTasks.Service;

public class LearningTaskService : ILearningTaskService
{
    private readonly AppDbContext _context;

    private readonly ILogger<LearningTaskService> _logger;
    
    public LearningTaskService(AppDbContext context, ILogger<LearningTaskService> logger)
    {
        _logger = logger;
        _context = context;
    }

    public LearningTaskResposeDto MapToResponseDto(LearningTask learningTask)
    {
        return new LearningTaskResposeDto
        (
            learningTask.Id,
            learningTask.Title,
            learningTask.Description,
            learningTask.ExpectedTechStack,
            learningTask.DueDate,
            learningTask.Status
        );
    }

    public async Task<LearningTask> FetchLearningTaskByIdInternalAsync(int id)
    {
        LearningTask? learningTask = await _context.LearningTasks.FindAsync(id);
        if (learningTask == null)
        {
            _logger.LogWarning("LearningTask with ID {TaskId} was not found", id);

            throw new NotFoundException("LearningTask");
        }
        return learningTask;
    }

    public async Task<IEnumerable<LearningTaskResposeDto>> GetLearningTaskAsync()
    {
        _logger.LogDebug("Fetching all learning-tasks from the database");

        IEnumerable<LearningTaskResposeDto> learningTasks = await _context.LearningTasks
                                                                        .Select(Lt =>  new 
                                                                            LearningTaskResposeDto
                                                                            (
                                                                                Lt.Id,
                                                                                Lt.Title,
                                                                                Lt.Description,
                                                                                Lt.ExpectedTechStack,
                                                                                Lt.DueDate,
                                                                                Lt.Status
                                                                            )
                                                                        ).ToListAsync();
        return learningTasks;
    }

    public async Task<LearningTaskResposeDto> GetLearningTaskByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving learning-task profile with ID: {TaskId}", id);
        
        LearningTaskResposeDto? learningTask = await _context.LearningTasks
                                                           .Where(lt => lt.Id == id)
                                                           .Select(lt => new 
                                                                LearningTaskResposeDto
                                                                (
                                                                   lt.Id,
                                                                   lt.Title,
                                                                   lt.Description,
                                                                   lt.ExpectedTechStack,
                                                                   lt.DueDate,
                                                                   lt.Status
                                                                )
                                                            ).FirstOrDefaultAsync();
        if (learningTask == null)
        {
            _logger.LogWarning("LearningTask with ID {TaskId} was not found", id);

            throw new NotFoundException("LearningTask");
        }
        return learningTask;
    }

    public async Task<LearningTaskResposeDto> CreateLearningTaskAsync(LearningTaskCreateDto createTask)
    {
        LearningTask learningTask = new LearningTask
        {
            Title = createTask.Title,
            Description = createTask.Description,
            ExpectedTechStack = createTask.ExpectedTechStack,
            DueDate = createTask.DueDate,
            Status = createTask.Status
        };

        _context.LearningTasks.Add(learningTask);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created new learning-task with ID {TaskId} and Title {Title}", learningTask.Id, learningTask.Title);
        
        return MapToResponseDto(learningTask);
    }

    public async Task DeleteLearningTaskByIdAsync(int id)
    {
        _logger.LogDebug("Find learning-task with ID {TaskId} for delete", id);

        LearningTask learningTask = await FetchLearningTaskByIdInternalAsync(id);
        _context.LearningTasks.Remove(learningTask);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted learning-task record with ID {TaskId}", id);

        return;
    }

    public async Task<LearningTaskResposeDto> UpdateLearningTaskByIdAsync(int id, LearningTaskUpdateDto updateTask)
    {
        _logger.LogDebug("Locating learning-task with ID {TaskId} for modifications", id);

        LearningTask learningTask = await FetchLearningTaskByIdInternalAsync(id);

        learningTask.Title = updateTask.Title;
        learningTask.Description = updateTask.Description;
        learningTask.ExpectedTechStack = updateTask.ExpectedTechStack;
        learningTask.DueDate = updateTask.DueDate;
        learningTask.Status = updateTask.Status;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully updated learning-task ID {TaskId}", id);

        return MapToResponseDto(learningTask);
    }
}