using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.MentorDTO;
using TraineeManagement.Api.Data.MentorModel;
using TraineeManagement.Api.MentorServiceInterface;
using TraineeManagement.Api.Data.CacheServiceInterface;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.CacheKey;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.Response;

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
            _logger.LogWarning("Dependency failure: Record missing. Id: {MentorId}", id);
            throw new NotFoundException(CustomResponse.NotFound,"Mentor");
        }
        return mentor;
    }

    public async Task<IEnumerable<MentorResponseDto>> GetMentorsAsync()
    {
        _logger.LogDebug("Fetching all mentors from the database");

        string cacheKey = CacheKey.AllMentor();
        IEnumerable<MentorResponseDto>? cached = await _cacheService.GetAsync<IEnumerable<MentorResponseDto>>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit. CacheKey: {CacheKey}", cacheKey);
            return cached;
        }
        
        _logger.LogDebug("Cache miss. CacheKey: {CacheKey}", cacheKey);

        IEnumerable<MentorResponseDto> mentors = await _context.Mentors
            .AsNoTracking()
            .Select(m => new MentorResponseDto(m.Id, m.FirstName, m.LastName))
            .ToListAsync();

        await _cacheService.SetAsync(cacheKey, mentors, TimeSpan.FromMinutes(CacheTime.TTL));

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
            _logger.LogWarning("Dependency failure: DTO projection missing. Id: {MentorId}", id);
            throw new NotFoundException(CustomResponse.NotFound,"Mentor");
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

        _logger.LogInformation("State transition: Created mentor record. Id: {MentorId}", mentor.Id);
        
        string cacheKey = CacheKey.AllMentor();
        await _cacheService.RemoveManyAsync(cacheKey);
        _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);

        return MapToResponseDto(mentor);
    }

    public async Task DeleteMentorByIdAsync(int id)
    {
        _logger.LogDebug("Find mentor with ID {MentorId} for delete", id);

        Mentor mentor = await FetchMentorByIdInternalAsync(id);
        
        _context.Mentors.Remove(mentor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("State transition: Deleted mentor record. Id: {MentorId}", id);

        string cacheKey = CacheKey.AllMentor();
        await _cacheService.RemoveManyAsync(cacheKey);
        _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);
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
        _logger.LogInformation("State transition: Modified mentor record. Id: {MentorId}", id);

        string cacheKey = CacheKey.AllMentor();
        await _cacheService.RemoveManyAsync(cacheKey);
        _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);
        
        return MapToResponseDto(mentor);
    }
}