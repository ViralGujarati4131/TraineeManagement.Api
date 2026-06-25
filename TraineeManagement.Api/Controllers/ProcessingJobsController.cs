using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.ConstRoute;
using TraineeManagement.Api.Data.DatabaseContext;

namespace TraineeManagement.Api.ProcessingJobControllers;

[ApiController]
[Route(CustomConstRoute.ProcessingJob)]
public class ProcessingJobController : ControllerBase
{
    private readonly AppDbContext _context;

    public ProcessingJobController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobStatus(Guid id)
    {
        var job = await _context.ProcessingJobs
            .Select(j => new 
            {
                JobId = j.Id,
                j.CorrelationId,
                j.SubmissionId,
                StatusDescription = j.Status == 1 ? "Queued" :
                                    j.Status == 2 ? "Processing" :
                                    j.Status == 3 ? "Completed" : "Failed",
                j.Attempts,
                j.ErrorSummary,
                j.StartedAt,
                j.CompletedAt
            })
            .FirstOrDefaultAsync(j => j.JobId == id);

        if (job == null)
        {
            return NotFound(new { Message = $"Processing tracking profile reference '{id}' not discovered." });
        }

        return Ok(job);
    }
}