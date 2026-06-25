using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.ProcessingJobDto;
using TraineeManagement.Api.ProcessingJobServiceInterface;

namespace TraineeManagement.Api.ProcessingJobService;

public class ProcessingJobService : IProcessingJobService
{
    private readonly AppDbContext _context;
    public ProcessingJobService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ProcessingJobResponseDto> GetProcessingJobByIdAsync(int id)
    {
         ProcessingJobResponseDto? jobTrack = await _context.ProcessingJobs
            .Where(pj => pj.Id == id)
            .Select(j => new ProcessingJobResponseDto(
                j.Id,
                j.MessageId,
                j.CorrelationId,
                j.SubmissionId,
                j.Status,
                j.Attempts,
                j.ErrorSummary,
                j.RequestedAt,
                j.StartedAt,
                j.CompletedAt
            ))
            .FirstOrDefaultAsync();

        if (jobTrack == null)
        {
            throw new NotFoundException("Processing Job");
        }

        return jobTrack;
    }
}