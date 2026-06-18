using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Trainees.DTOs;
using TraineeManagementApi.Trainees.Models;
using TraineeManagementApi.Trainees.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Constants;

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
        return new TraineeResponseDto
        (
            trainee.Id,
            trainee.FirstName,
            trainee.LastName
        );
    }

    private async Task<Trainee> FetchTraineeByIdInternalAsync(int id)
    {
        Trainee? trainee = await _context.Trainees.FindAsync(id);
        if (trainee == null)
        {
            _logger.LogWarning("Trainee with ID {TraineeId} was not found", id);

            throw new NotFoundException("Trainee");
        }
        return trainee;
    }

    public async Task<IEnumerable<TraineeResponseDto>> GetTraineesAsync()
    {
        _logger.LogDebug("Fetching all trainees from the database");

        IEnumerable<TraineeResponseDto> trainees = await _context.Trainees
                                                               .Select(t => new TraineeResponseDto(
                                                                   t.Id,
                                                                   t.FirstName,
                                                                   t.LastName
                                                               )).ToListAsync();
        return trainees;
    }

    public async Task<TraineeResponseDto> GetTraineeByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving trainee profile with ID: {TraineeId}", id);

        TraineeResponseDto? trainee = await _context.Trainees
                                                  .Where(t => t.Id == id)
                                                  .Select(t => new TraineeResponseDto(
                                                      t.Id,
                                                      t.FirstName,
                                                      t.LastName
                                                  )).FirstOrDefaultAsync();
        if (trainee == null)
        {
            _logger.LogWarning("Trainee with ID {TraineeId} was not found", id);

            throw new NotFoundException("Trainee");
        }
        return trainee;
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
        
        _logger.LogInformation("Successfully created new trainee with ID {TraineeId} and FirstName {FirstName}", trainee.Id, trainee.FirstName);
        
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
        _logger.LogDebug("Find trainee with ID {TraineeId} for delete", id);

        Trainee trainee = await FetchTraineeByIdInternalAsync(id);
        _context.Trainees.Remove(trainee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted trainee record with ID {TraineeId}", id);

        return;
    }

    public async Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTerm)
    {
        _logger.LogDebug("Executing text search match for: {SearchTerm}", searchTerm);

        IEnumerable<TraineeResponseDto> matchingTrainees = await _context.Trainees
                                                                        .Where
                                                                        (t => 
                                                                            t.FirstName.Contains(searchTerm) ||
                                                                            t.LastName.Contains(searchTerm) ||
                                                                            t.Email.Contains(searchTerm) ||
                                                                            t.TechStack.Contains(searchTerm)
                                                                        )
                                                                        .Select
                                                                        (t => new 
                                                                        TraineeResponseDto
                                                                        (
                                                                            t.Id,
                                                                            t.FirstName,
                                                                            t.LastName
                                                                        )).ToListAsync();
        return matchingTrainees;
    }

    public async Task<TraineePaginationSearchDto> GetPagedAndSearchedTraineesAsync(int pageNumber, int pageSize, string name, string status)
    {
        _logger.LogDebug("Executing target filter parameters - Name: {FilterName}, Status: {FilterStatus}", name, status);
        
        IQueryable<Trainee> query = _context.Trainees.AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(t => t.FirstName.Contains(name));

        if (!string.IsNullOrWhiteSpace(status))
            query = query.Where(t => t.Status.ToString() == status);

        int totalRecords = await query.CountAsync();

        _logger.LogDebug("Applying pagination - Page size: {PageSize}, Offset: {Offset}", pageSize, (pageNumber - 1) * pageSize);
        
        IEnumerable<TraineeResponseDto> responseData = await query
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new 
                TraineeResponseDto
                (
                    t.Id,
                    t.FirstName,
                    t.LastName
                )
            ).ToListAsync();

        return new 
        TraineePaginationSearchDto
        (
            pageNumber,
            responseData.Count(),
            totalRecords,
            responseData
        );
    }
}