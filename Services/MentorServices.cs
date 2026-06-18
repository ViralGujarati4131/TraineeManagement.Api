using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Mentors.DTOs;
using TraineeManagementApi.Mentors.Models;
using TraineeManagementApi.Mentors.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Mentors.Service;

public class MentorService : IMentorServices
{
    private readonly AppDbContext _context;

    private readonly ILogger<MentorService> _logger;
    
    public MentorService(AppDbContext context, ILogger<MentorService> logger)
    {
        _context = context;
        _logger = logger;
    }
    public MentorResponseDto MapToResponseDto(Mentor mentor)
    {
        return new MentorResponseDto
        (
            mentor.Id,
            mentor.FirstName,
            mentor.LastName
        );
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
        
        IEnumerable<MentorResponseDto> mentors = await _context.Mentors
                                                             .Select(m => new 
                                                                MentorResponseDto 
                                                                (
                                                                    m.Id,
                                                                    m.FirstName,
                                                                    m.LastName
                                                                )
                                                            ).ToListAsync();
        return mentors;
    }

    public async Task<MentorResponseDto> GetMentorByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving mentor profile with ID: {MentorId}", id);

        MentorResponseDto? mentor = await _context.Mentors
                                                .Where(m => m.Id == id)
                                                .Select(m => new 
                                                    MentorResponseDto
                                                    (
                                                        m.Id,
                                                        m.FirstName,
                                                        m.LastName
                                                    )
                                                ).FirstOrDefaultAsync();
        if (mentor == null)
        {
            _logger.LogWarning("Mentor with ID {MentorId} was not found", id);

            throw new NotFoundException("Mentor");
        }
        return mentor;
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
        
        return MapToResponseDto(mentor);
    }

    public async Task DeleteMentorByIdAsync(int id)
    {
        _logger.LogDebug("Find mentor with ID {MentorId} for delete", id);

        Mentor mentor = await FetchMentorByIdInternalAsync(id);
        _context.Mentors.Remove(mentor);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully deleted mentor record with ID {MentorId}", id);

        return;
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
        
        return MapToResponseDto(mentor);
    }
}