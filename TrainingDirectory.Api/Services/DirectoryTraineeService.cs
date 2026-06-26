using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.CacheServiceInterface;
using TraineeManagement.Api.Data.CacheKey;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.TraineeDTO;
using TrainingDirectory.Api.DirectoryTraineeServiceInterface;

namespace TrainingDirectory.Api.DirectoryTraineeService;

public class DirectoryTraineeService : IDirectoryTraineeService
{
    private readonly AppDbContext _context;

    private readonly ICacheService _cacheService;

    public DirectoryTraineeService(AppDbContext context,ICacheService cacheService)
    {
        _context = context;
        _cacheService = cacheService;
    }
    
    public async Task<TraineeResponseDto> GetTraineeByIdAsync(int id, CancellationToken cancellationToken)
    {
        TraineeResponseDto? cached = await _cacheService.GetAsync<TraineeResponseDto>(CacheKey.Trainee(id));
        if (cached is not null)
            return cached;

        TraineeResponseDto? trainee = await _context.Trainees
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .FirstOrDefaultAsync(cancellationToken);

        if (trainee == null)
        {
            throw new NotFoundException("Trainee");
        }

        await _cacheService.SetAsync(CacheKey.Trainee(id), trainee, TimeSpan.FromMinutes(10));

        return trainee;
    }
}