using System.Collections;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Mentors.DTOs;
using TraineeManagementApi.Mentors.Models;
using TraineeManagementApi.Mentors.ServiceInterface;
using TraineeManagementApi.RedisCaching.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;

namespace TraineeManagementApi.Mentors.Service;

public class MentorService : IMentorServices
{
    private readonly AppDbContext _context;

    private readonly ILogger<MentorService> _logger;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);

    private readonly ICacheService _cacheService;
    
    public MentorService(AppDbContext context, ILogger<MentorService> logger,ICacheService cacheService)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
    }

    public MentorResponseDto MapToResponseDto(Mentor mentor)
    {
        return new MentorResponseDto(mentor.Id, mentor.FirstName, mentor.LastName);
    }

    public async Task<Mentor> FetchMentorByIdInternalAsync(int id)
    {
        Mentor? mentor = await _context.Mentors.FindAsync(id);
        if (mentor == null)
        {
            _logger.LogWarning("Mentor with ID {MentorId} was not found", id);
            throw new NotFoundException("Mentor");
        }
        return mentor;
    }

    public async Task<IEnumerable<MentorResponseDto>> GetMentorsAsync()
    {
        _logger.LogDebug("Fetching all mentors from the database");

        string cacheKey = "Mentors:All";
        IEnumerable<MentorResponseDto>? cached = await _cacheService.GetAsync<IEnumerable<MentorResponseDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning mentors from cache");
            return cached;
        }
        
        IEnumerable<MentorResponseDto> mentors = await _context.Mentors
            .AsNoTracking()
            .Select(m => new MentorResponseDto(m.Id, m.FirstName, m.LastName))
            .ToListAsync();

        await _cacheService.SetAsync(cacheKey,mentors,CacheTtl);

        return mentors;
    }

    public async Task<MentorResponseDto> GetMentorByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving mentor profile with ID: {MentorId}", id);

        string cacheKey = $"Mentor:{id}";
        MentorResponseDto? cached = await _cacheService.GetAsync<MentorResponseDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning mentors from cache");
            return cached;
        }

        MentorResponseDto? dto = await _context.Mentors
            .AsNoTracking()
            .Where(m => m.Id == id)
            .Select(m => new MentorResponseDto(m.Id, m.FirstName, m.LastName))
            .FirstOrDefaultAsync();

        if (dto == null)
        {
            _logger.LogWarning("Mentor with ID {MentorId} was not found during target DTO projection.", id);
            throw new NotFoundException("Mentor");
        }
        await _cacheService.SetAsync(cacheKey,dto,CacheTtl);
        
        return dto;
    }

    public async Task<MentorResponseDto> CreateMentorAsync(MentorCreateDto createMentor)
    {
        Mentor mentor = new Mentor
        {
            FirstName = createMentor.FirstName,
            LastName = createMentor.LastName,
            Email = createMentor.Email,
            Expertise = createMentor.Expertise,
            Status = createMentor.Status
        };
        
        _context.Mentors.Add(mentor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created new mentor with ID {MentorId} and FirstName {FirstName}", mentor.Id, mentor.FirstName);
        
        await _cacheService.RemoveAsync("Mentors:All");
        
        return MapToResponseDto(mentor);
    }

    public async Task DeleteMentorByIdAsync(int id)
    {
        _logger.LogDebug("Find mentor with ID {MentorId} for delete", id);

        Mentor mentor = await FetchMentorByIdInternalAsync(id);
        
        _context.Mentors.Remove(mentor);
        await _context.SaveChangesAsync();

        await _cacheService.RemoveAsync($"Mentor:{id}");
        await _cacheService.RemoveAsync("Mentors:All");

        _logger.LogInformation("Successfully deleted mentor record with ID {MentorId}", id);
    }

    public async Task<MentorResponseDto> UpdateMentorByIdAsync(int id, MentorUpdateDto updateMentor)
    {
        _logger.LogDebug("Locating mentor with ID {MentorId} for modifications", id);

        Mentor mentor = await FetchMentorByIdInternalAsync(id);

        mentor.FirstName = updateMentor.FirstName;
        mentor.LastName = updateMentor.LastName;
        mentor.Email = updateMentor.Email;
        mentor.Expertise = updateMentor.Expertise;
        mentor.Status = updateMentor.Status;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Successfully updated mentor profile for ID {MentorId}", id);

        await _cacheService.RemoveAsync($"Mentor:{id}");
        await _cacheService.RemoveAsync("Mentors:All");
        
        return MapToResponseDto(mentor);
    }
}