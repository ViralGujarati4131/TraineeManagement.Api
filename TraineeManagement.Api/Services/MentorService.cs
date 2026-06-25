using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.MentorDTO;
using TraineeManagement.Api.Data.MentorModel;
using TraineeManagement.Api.MentorServiceInterface;
using TraineeManagement.Api.CacheServiceInterface;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.CacheKey;
using TraineeManagement.Api.Data.DatabaseContext;

namespace TraineeManagement.Api.MentorService;

public class MentorService : IMentorServices
{
    private readonly AppDbContext _context;

    private readonly ILogger<MentorService> _logger;

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

        IEnumerable<MentorResponseDto>? cached = await _cacheService.GetAsync<IEnumerable<MentorResponseDto>>(CacheKey.AllMentor());
        if (cached is not null)
            return cached;
        
        IEnumerable<MentorResponseDto> mentors = await _context.Mentors
            .AsNoTracking()
            .Select(m => new MentorResponseDto(m.Id, m.FirstName, m.LastName))
            .ToListAsync();

        await _cacheService.SetAsync(CacheKey.AllMentor(), mentors, TimeSpan.FromMinutes(10));

        return mentors;
    }

    public async Task<MentorResponseDto> GetMentorByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving mentor profile with ID: {MentorId}", id);

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
        
        await _cacheService.RemoveManyAsync(CacheKey.AllMentor());

        return MapToResponseDto(mentor);
    }

    public async Task DeleteMentorByIdAsync(int id)
    {
        _logger.LogDebug("Find mentor with ID {MentorId} for delete", id);

        Mentor mentor = await FetchMentorByIdInternalAsync(id);
        
        _context.Mentors.Remove(mentor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted mentor record with ID {MentorId}", id);

        await _cacheService.RemoveManyAsync(CacheKey.AllMentor());
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

        await _cacheService.RemoveManyAsync(CacheKey.AllMentor());
        
        return MapToResponseDto(mentor);
    }
}