using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.LearningTaskDTO;
using TraineeManagement.Api.Data.LearningTaskModel;
using TraineeManagement.Api.LearningTaskServiceInterface;
using TraineeManagement.Api.Data.CacheServiceInterface;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.CacheKey;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.Response;

namespace TraineeManagement.Api.LearningTaskService;

public class LearningTaskService : ILearningTaskService
{
    private readonly AppDbContext _context;

    private readonly ILogger<LearningTaskService> _logger;

    private readonly ICacheService _cacheService;

    
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
            _logger.LogWarning("Dependency failure: Record missing. Id: {TaskId}", id);
            throw new NotFoundException(CustomResponse.NotFound,"LearningTask");
        }
        return learningTask;
    }

    public async Task<IEnumerable<LearningTaskResposeDto>> GetLearningTaskAsync()
    {
        _logger.LogDebug("Fetching all learning-tasks from the database");

        string cacheKey = CacheKey.AllLearningTask();
        IEnumerable<LearningTaskResposeDto>? cached = await _cacheService.GetAsync<IEnumerable<LearningTaskResposeDto>>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit. CacheKey: {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss. CacheKey: {CacheKey}", cacheKey);

        IEnumerable<LearningTaskResposeDto> learningTasks = await _context.LearningTasks
            .AsNoTracking()
            .Select(Lt => new LearningTaskResposeDto(
                Lt.Id,
                Lt.Title,
                Lt.Description,
                Lt.ExpectedTechStack,
                Lt.DueDate,
                Lt.Status
            )).ToListAsync();
        
        await _cacheService.SetAsync(cacheKey, learningTasks, TimeSpan.FromMinutes(CacheTime.TTL));

        return learningTasks;
    }

    public async Task<LearningTaskResposeDto> GetLearningTaskByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving learning-task profile with ID: {TaskId}", id);
        
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
            _logger.LogWarning("Dependency failure: DTO projection missing. Id: {TaskId}", id);
            throw new NotFoundException(CustomResponse.NotFound,"LearningTask");
        }

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

        _logger.LogInformation("State transition: Created task record. Id: {TaskId}", learningTask.Id);

        string cacheKey = CacheKey.AllLearningTask();
        await _cacheService.RemoveManyAsync(cacheKey);
        _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);

        return MapToResponseDto(learningTask);
    }

    public async Task DeleteLearningTaskByIdAsync(int id)
    {
        _logger.LogDebug("Find learning-task with ID {TaskId} for delete", id);

        LearningTask learningTask = await FetchLearningTaskByIdInternalAsync(id);
        
        _context.LearningTasks.Remove(learningTask);
        await _context.SaveChangesAsync();

        _logger.LogInformation("State transition: Deleted task record. Id: {TaskId}", id);

        string cacheKey = CacheKey.AllLearningTask();
        await _cacheService.RemoveManyAsync(cacheKey);
        _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);
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
        _logger.LogInformation("State transition: Modified task record. Id: {TaskId}", id);

        string cacheKey = CacheKey.AllLearningTask();
        await _cacheService.RemoveManyAsync(cacheKey);
        _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);

        return MapToResponseDto(learningTask);
    }
}