using System.Collections;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.DTOs;
using TraineeManagementApi.Models;
using TraineeManagementApi.Service.Interface;

namespace TraineeManagementApi.Service;

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
        {
            Id = trainee.Id,
            FirstName = trainee.FirstName,
            LastName = trainee.LastName,
        };
    }

    private async Task<Trainee?> FetchTraineeByIdInternalAsync(int id)
    {
        Trainee? trainee = await _context.Trainees.FindAsync(id);
        if (trainee == null)
        {
            _logger.LogWarning("Trainee with ID {TraineeId} was not found", id);
            return null;
        }
        return trainee;
    }

    public async Task<IEnumerable<TraineeResponseDto>> GetTraineesAsync()
    {
        _logger.LogDebug("Fetching all trainees from the database");
        IEnumerable<Trainee> trainees = await _context.Trainees.ToListAsync();
        return trainees.Select(MapToResponseDto);
    }

    public async Task<TraineeResponseDto?> GetTraineeByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving trainee profile with ID: {TraineeId}", id);
        Trainee? trainee = await FetchTraineeByIdInternalAsync(id);
        if (trainee == null) return null;
        return MapToResponseDto(trainee);
    }

    public async Task<TraineeResponseDto> CreateTraineeAsync(CreateTraineeDto createTraineeDto)
    {
        Trainee trainee = new Trainee
        {
            FirstName = createTraineeDto.FirstName,
            LastName = createTraineeDto.LastName,
            Email = createTraineeDto.Email,
            TechStack = createTraineeDto.TechStack,
            Status = createTraineeDto.Status
        };

        await _context.Trainees.AddAsync(trainee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created new trainee with ID {TraineeId} and FirstName {FirstName}", trainee.Id, trainee.FirstName);
        return MapToResponseDto(trainee);
    }

    public async Task<TraineeResponseDto?> UpdateTraineeAsync(int id, UpdateTraineeDto updateTraineeDto)
    {
        _logger.LogDebug("Locating trainee with ID {TraineeId} for modifications", id);
        Trainee? trainee = await FetchTraineeByIdInternalAsync(id);
        if (trainee == null) return null;

        trainee.FirstName = updateTraineeDto.FirstName;
        trainee.LastName = updateTraineeDto.LastName;
        trainee.Email = updateTraineeDto.Email;
        trainee.TechStack = updateTraineeDto.TechStack;
        trainee.Status = updateTraineeDto.Status;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully updated trainee profile for ID {TraineeId}", id);
        return MapToResponseDto(trainee);
    }

    public async Task<bool> DeleteTraineeByIdAsync(int id)
    {
        _logger.LogDebug("Find trainee with ID {TraineeId} for delete", id);
        Trainee? trainee = await FetchTraineeByIdInternalAsync(id);
        if (trainee == null)
        {
            return false;
        }

        _context.Trainees.Remove(trainee);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted trainee record with ID {TraineeId}", id);
        return true;
    }

    public async Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTerm)
    {
        _logger.LogDebug("Executing text search match for: {SearchTerm}", searchTerm);
        
        IEnumerable<Trainee> matchingTrainees = await _context.Trainees
            .Where(t => t.FirstName.Contains(searchTerm) ||
                        t.LastName.Contains(searchTerm) ||
                        t.Email.Contains(searchTerm) ||
                        t.TechStack.Contains(searchTerm))
            .ToListAsync();

        return matchingTrainees.Select(MapToResponseDto).ToList();
    }

    public async Task<PaginationSearchDto?> GetPagedAndSearchedTraineesAsync(int pageNumber, int pageSize, string name, string status)
    {
        _logger.LogDebug("Executing target filter parameters - Name: {FilterName}, Status: {FilterStatus}", name, status);

        IEnumerable<Trainee> query = await _context.Trainees
            .Where(t => t.FirstName.Equals(name) && t.Status.ToString().Equals(status)).ToListAsync();

        int totalRecords = query.Count();

        _logger.LogDebug("Applying pagination layout offset values - Page size: {PageSize}, Offset index: {Offset}", pageSize, (pageNumber - 1) * pageSize);
        
        IEnumerable<Trainee> pagedData = query
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        if (!pagedData.Any())
        {
            return null;
        }

        IEnumerable<TraineeResponseDto> responseData = pagedData.Select(MapToResponseDto).ToList();

        return new PaginationSearchDto
        {
            PageNumber = pageNumber,
            PageSize = pagedData.Count(),
            TotalRecords = totalRecords,
            Data = responseData
        };
    }
}