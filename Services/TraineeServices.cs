using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Trainees.DTOs;
using TraineeManagementApi.Trainees.Models;
using TraineeManagementApi.Trainees.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;

namespace TraineeManagementApi.Trainees.Service;

public class TraineeService : ITraineeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TraineeService> _logger;

    public TraineeService(AppDbContext context, ILogger<TraineeService> logger)
    {
        _context = context;
        _logger = logger;
    }

    private TraineeResponseDto MapToResponseDto(Trainee trainee)
    {
        return new TraineeResponseDto(trainee.Id, trainee.FirstName, trainee.LastName);
    }

    private async Task<Trainee> FetchTraineeByIdInternalAsync(int id)
    {
        Trainee? trainee = await _context.Trainees.FindAsync(id);
        if (trainee == null)
        {
            _logger.LogWarning("Trainee with ID {TraineeId} was not found in the database layer.", id);
            throw new NotFoundException("Trainee");
        }
        return trainee;
    }

    public async Task<IEnumerable<TraineeResponseDto>> GetTraineesAsync()
    {
        _logger.LogDebug("Fetching all trainees from the database");

        return await _context.Trainees
            .AsNoTracking() 
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .ToListAsync();
    }

    public async Task<TraineeResponseDto> GetTraineeByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving trainee profile with ID: {TraineeId}", id);

        TraineeResponseDto? dto = await _context.Trainees
            .AsNoTracking()
            .Where(t => t.Id == id)
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .FirstOrDefaultAsync();

        if (dto == null)
        {
            _logger.LogWarning("Trainee with ID {TraineeId} was not found during target DTO projection.", id);
            throw new NotFoundException("Trainee");
        }
        
        return dto;
    }

    public async Task<TraineeResponseDto> CreateTraineeAsync(TraineeCreateDto createTraineeDto)
    {
        Trainee trainee = new Trainee
        {
            FirstName = createTraineeDto.FirstName,
            LastName = createTraineeDto.LastName,
            Email = createTraineeDto.Email,
            TechStack = createTraineeDto.TechStack,
            Status = createTraineeDto.Status
        };

        _context.Trainees.Add(trainee);
        await _context.SaveChangesAsync();
        
        _logger.LogInformation("Successfully created new trainee with ID {TraineeId}", trainee.Id);
        return MapToResponseDto(trainee);
    }

    public async Task<TraineeResponseDto> UpdateTraineeAsync(int id, TraineeUpdateDto updateTraineeDto)
    {
        _logger.LogDebug("Locating trainee with ID {TraineeId} for modifications", id);
        Trainee trainee = await FetchTraineeByIdInternalAsync(id);

        trainee.FirstName = updateTraineeDto.FirstName;
        trainee.LastName = updateTraineeDto.LastName;
        trainee.Email = updateTraineeDto.Email;
        trainee.TechStack = updateTraineeDto.TechStack;
        trainee.Status = updateTraineeDto.Status;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully updated trainee profile for ID {TraineeId}", id);
        return MapToResponseDto(trainee);
    }

    public async Task DeleteTraineeByIdAsync(int id)
    {
        _logger.LogDebug("Finding trainee with ID {TraineeId} for physical deletion", id);
        Trainee trainee = await FetchTraineeByIdInternalAsync(id);
        
        _context.Trainees.Remove(trainee);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully deleted trainee record with ID {TraineeId}", id);
    }

    public async Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTerm)
    {
        _logger.LogDebug("Executing text search match for: {SearchTerm}", searchTerm);

        return await _context.Trainees
            .AsNoTracking()
            .Where(t => 
                t.FirstName.Contains(searchTerm) ||
                t.LastName.Contains(searchTerm) ||
                t.Email.Contains(searchTerm) ||
                t.TechStack.Contains(searchTerm)
            )
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .ToListAsync();
    }

    public async Task<TraineePaginationSearchDto> GetPagedAndSearchedTraineesAsync(int pageNumber, int pageSize, string name, string status)
    {
        _logger.LogDebug("Executing target filter parameters - Name: {FilterName}, Status: {FilterStatus}", name, status);
        
        IQueryable<Trainee> query = _context.Trainees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
        {
            query = query.Where(t => t.FirstName.Contains(name));
        }

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(status, true, out TraineeStatus parsedStatus))
        {
            query = query.Where(t => t.Status == parsedStatus);
        }

        int totalRecords = await query.CountAsync();

        _logger.LogDebug("Applying pagination - Page size: {PageSize}, Offset: {Offset}", pageSize, (pageNumber - 1) * pageSize);
        
        List<TraineeResponseDto> responseData = await query
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .ToListAsync();

        return new TraineePaginationSearchDto(pageNumber, responseData.Count, totalRecords, responseData);
    }
}