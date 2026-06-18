using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Submissions.DTOs;
using TraineeManagementApi.Submissions.Models;
using TraineeManagementApi.Submissions.ServiceInterface;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Submissions.Service;

public class SubmissionService : ISubmissionService
{
    private readonly ILogger<SubmissionService> _logger;
    
    private readonly AppDbContext _cotext;

    public SubmissionService(ILogger<SubmissionService> logger, AppDbContext context)
    {
        _logger = logger;
        _cotext = context;
    }

    private SubmissionResponseDto MapToResponseDto(Submission submission)
    {
        return new SubmissionResponseDto
        (
            submission.Id,
            submission.TaskAssignmentId,
            submission.SubmissionUrl,
            submission.Notes,
            submission.SubmittedDate,
            submission.Status
        );
    }

    private async Task<Submission> FetchSubmissionByIdInternalAsync(int id)
    {
        Submission? submission = await _cotext.Submissions.FindAsync(id);
        if (submission == null)
        {
            _logger.LogWarning("Submission with ID {SubmissionId} was not found", id);

            throw new NotFoundException("Submission");
        }
        return submission;
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
        _cotext.Submissions.Add(submission);
        await _cotext.SaveChangesAsync();

        _logger.LogInformation("Successfully created new submission with ID {SubmissionId}", submission.Id);

        return MapToResponseDto(submission);
    }

    public async Task<IEnumerable<SubmissionResponseDto>> GetSubmissionsAsync()
    {
        _logger.LogDebug("Fetching all submissions from the database");

        IEnumerable<SubmissionResponseDto> submissions = await _cotext.Submissions
                                                                    .Select(s => new 
                                                                        SubmissionResponseDto
                                                                        (
                                                                            s.Id,
                                                                            s.TaskAssignmentId,
                                                                            s.SubmissionUrl,
                                                                            s.Notes,
                                                                            s.SubmittedDate,
                                                                            s.Status
                                                                        )
                                                                    ).ToListAsync();
        return submissions;
    }
    
    public async Task<SubmissionResponseDto> GetSubmissionByIdAsync(int id)
    {
        _logger.LogDebug("Retrieving submission with ID: {SubmissionId}", id);

        SubmissionResponseDto? submission = await _cotext.Submissions
                                                       .Where(s => s.Id == id)
                                                       .Select(s => new 
                                                            SubmissionResponseDto
                                                            (
                                                               s.Id,
                                                               s.TaskAssignmentId,
                                                               s.SubmissionUrl,
                                                               s.Notes,
                                                               s.SubmittedDate,
                                                               s.Status
                                                           )
                                                        ).FirstOrDefaultAsync();
        if (submission == null)
        {
            _logger.LogWarning("Submission with ID {SubmissionId} was not found", id);

            throw new NotFoundException("Submission");
        }
        return submission;
    }
}