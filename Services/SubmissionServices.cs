using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.RedisCaching.ServiceInterface;
using TraineeManagementApi.Submissions.DTOs;
using TraineeManagementApi.Submissions.Models;
using TraineeManagementApi.Submissions.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;

namespace TraineeManagementApi.Submissions.Service;

public class SubmissionService : ISubmissionService
{
    private readonly ILogger<SubmissionService> _logger;

    private readonly AppDbContext _context; 

    private readonly ICacheService _cacheService;

    private static readonly TimeSpan CacheTtl = TimeSpan.FromMinutes(10);


    public SubmissionService(ILogger<SubmissionService> logger, AppDbContext context,ICacheService cacheService)
    {
        _logger = logger;
        _context = context;
        _cacheService = cacheService;
    }

    private SubmissionResponseDto MapToResponseDto(Submission submission)
    {
        return new SubmissionResponseDto(
            submission.Id,
            submission.TaskAssignmentId,
            submission.SubmissionUrl,
            submission.Notes,
            submission.SubmittedDate,
            submission.Status
        );
    }

    public async Task<SubmissionResponseDto> CreateSubmissionAsync(SubmissionCreateDto submissionCreateDto)
    {
        Submission submission = new Submission
        {
            TaskAssignmentId = submissionCreateDto.TaskAssignmentId,
            SubmissionUrl = submissionCreateDto.SubmissionUrl,
            Notes = submissionCreateDto.Notes,
            SubmittedDate = submissionCreateDto.SubmittedDate,
            Status = submissionCreateDto.Status
        };
        
        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        _logger.LogInformation("Successfully created new submission with ID {SubmissionId}", submission.Id);
        
        await _cacheService.RemoveAsync("Submissions:All");
        
        return MapToResponseDto(submission);
    }

    public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync()
    {
        _logger.LogDebug("Fetching all submissions from the database");

        string cacheKey = "Submissions:All";
        IEnumerable<SubmissionResponseDto>? cached = await _cacheService.GetAsync<IEnumerable<SubmissionResponseDto>>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning submissions from cache");
            return cached;
        }

        IEnumerable<SubmissionResponseDto> submissions = await _context.Submissions
            .AsNoTracking()
            .Select(s => new SubmissionResponseDto(
                s.Id,
                s.TaskAssignmentId,
                s.SubmissionUrl,
                s.Notes,
                s.SubmittedDate,
                s.Status
            )).ToListAsync();

        await _cacheService.SetAsync(cacheKey, submissions, CacheTtl);

        return submissions;
    }
    
    public async Task<SubmissionResponseDto> GetSubmissionByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving submission with ID: {SubmissionId}", id);
        
        string cacheKey = $"Submissions:{id}";
        SubmissionResponseDto? cached = await _cacheService.GetAsync<SubmissionResponseDto>(cacheKey);
        if (cached != null)
        {
            _logger.LogDebug("Returning submission from cache");
            return cached;
        }

        SubmissionResponseDto? dto = await _context.Submissions
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => new SubmissionResponseDto(
                s.Id,
                s.TaskAssignmentId,
                s.SubmissionUrl,
                s.Notes,
                s.SubmittedDate,
                s.Status
            )).FirstOrDefaultAsync();

        if (dto == null)
        {
            _logger.LogWarning("Submission with ID {SubmissionId} was not found during target DTO projection.", id);
            throw new NotFoundException("Submission");
        }
        await _cacheService.SetAsync(cacheKey, dto, CacheTtl);

        
        return dto;
    }
}