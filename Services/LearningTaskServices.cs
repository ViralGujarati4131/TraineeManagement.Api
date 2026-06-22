using System.Collections;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.LearningTasks.DTOs;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.LearningTasks.ServiceInterface;
using TraineeManagementApi.RedisCaching.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;

namespace TraineeManagementApi.LearningTasks.Service;

public class LearningTaskService : ILearningTaskService
{
    private readonly AppDbContext _context;

    private readonly ILogger<LearningTaskService> _logger;

    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    
    public LearningTaskService(AppDbContext context, ILogger<LearningTaskService> logger,ICacheService cacheService)
    {
        _logger = logger;
        _context = context;
        _cacheService = cacheService;
    }

    public LearningTaskResposeDto MapToResponseDto(LearningTask learningTask)
    {
        return new LearningTaskResposeDto(
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

        string cacheKey = "LearningTasks:All";
        IEnumerable<LearningTaskResposeDto>? cached = await _cacheService.GetAsync<IEnumerable<LearningTaskResposeDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning learning-tasks from cache");
            return cached;
        }

        IEnumerable<LearningTaskResposeDto>? learningTaskResposes = await _context.LearningTasks
            .AsNoTracking()
            .Select(Lt => new LearningTaskResposeDto(
                Lt.Id,
                Lt.Title,
                Lt.Description,
                Lt.ExpectedTechStack,
                Lt.DueDate,
                Lt.Status
            )).ToListAsync();

        await _cacheService.SetAsync(cacheKey, learningTaskResposes, CacheTtl);

        return learningTaskResposes;
    }

    public async Task<LearningTaskResposeDto> GetLearningTaskByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving learning-task profile with ID: {TaskId}", id);

        string cacheKey = $"LearningTask:{id}";
        LearningTaskResposeDto? cached = await _cacheService.GetAsync<LearningTaskResposeDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning learning-tasks from cache");
            return cached;
        }
        
        LearningTaskResposeDto? dto = await _context.LearningTasks
            .AsNoTracking()
            .Where(Lt => Lt.Id == id)
            .Select(Lt => new LearningTaskResposeDto(
                Lt.Id,
                Lt.Title,
                Lt.Description,
                Lt.ExpectedTechStack,
                Lt.DueDate,
                Lt.Status
            )).FirstOrDefaultAsync();

        if (dto == null)
        {
            _logger.LogWarning("LearningTask with ID {TaskId} was not found during target DTO projection.", id);
            throw new NotFoundException("LearningTask");
        }
        await _cacheService.SetAsync(cacheKey, dto, CacheTtl);

        return dto;
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
        
        await _cacheService.RemoveAsync("LearningTasks:All");

        return MapToResponseDto(learningTask);
    }

    public async Task DeleteLearningTaskByIdAsync(int id)
    {
        _logger.LogDebug("Find learning-task with ID {TaskId} for delete", id);

        LearningTask learningTask = await FetchLearningTaskByIdInternalAsync(id);
        
        _context.LearningTasks.Remove(learningTask);
        await _context.SaveChangesAsync();

        await _cacheService.RemoveAsync("LearningTasks:All");
        await _cacheService.RemoveAsync($"LearningTask:{id}");

        _logger.LogInformation("Successfully deleted learning-task record with ID {TaskId}", id);
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

        await _cacheService.RemoveAsync("LearningTasks:All");
        await _cacheService.RemoveAsync($"LearningTask:{id}");

        return MapToResponseDto(learningTask);
    }
}