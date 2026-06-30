using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.SubmissionDTO;
using TraineeManagement.Api.Data.SubmissionModel;
using TraineeManagement.Api.SubmissionServiceInterface;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.CacheKey;
using TraineeManagement.Api.Data.CacheServiceInterface;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.Response;


namespace TraineeManagement.Api.SubmissionService;

public class SubmissionService : ISubmissionService
{
    private readonly ILogger<SubmissionService> _logger;

    private readonly AppDbContext _context; 

    private readonly ICacheService _cacheService;


    public SubmissionService(ILogger<SubmissionService> logger, AppDbContext context, ICacheService cacheService)
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

        _logger.LogInformation("State transition: Created submission record. Id: {SubmissionId}", submission.Id);
        
        return MapToResponseDto(submission);
    }

    public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync()
    {
        _logger.LogDebug("Fetching all submissions from the database");

        return await _context.Submissions
            .AsNoTracking()
            .Select(s => new SubmissionResponseDto(
                s.Id,
                s.TaskAssignmentId,
                s.SubmissionUrl,
                s.Notes,
                s.SubmittedDate,
                s.Status
            )).ToListAsync();
    }
    
    public async Task<SubmissionResponseDto> GetSubmissionByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving submission with ID: {SubmissionId}", id);

        string cacheKey = CacheKey.Submission(id);
        SubmissionResponseDto? cached = await _cacheService.GetAsync<SubmissionResponseDto>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit. CacheKey: {CacheKey}", cacheKey);
            return cached;
        }

        _logger.LogDebug("Cache miss. CacheKey: {CacheKey}", cacheKey);

        SubmissionResponseDto? dto = await _context.Submissions
            .AsNoTracking()
            .Where(s => s.Id == id)
            .Select(s => s == null ? null : new SubmissionResponseDto(
                s.Id,
                s.TaskAssignmentId,
                s.SubmissionUrl,
                s.Notes,
                s.SubmittedDate,
                s.Status
            )).FirstOrDefaultAsync();

        if (dto == null)
        {
            _logger.LogWarning("Dependency failure: DTO projection missing. Id: {SubmissionId}", id);
            throw new NotFoundException(CustomResponse.NotFound,"Submission");
        }
        
        await _cacheService.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(CacheTime.TTL));

        _logger.LogInformation("State check: Bulk fetch submissions success. Id: {SubmissionId}", id);
        return dto;
    }
}