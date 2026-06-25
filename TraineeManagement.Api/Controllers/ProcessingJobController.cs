using Microsoft.AspNetCore.Mvc;
using TraineeManagement.Api.Data.ConstRoute;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.ProcessingJobDto;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.ProcessingJobServiceInterface;
using TraineeManagement.Api.ResponsesBuilder;

namespace TraineeManagement.Api.ProcessingJobControllers;

[ApiController]
[Route(CustomConstRoute.ProcessingJob)]
public class ProcessingJobsController : ControllerBase
{
    private readonly AppDbContext _context;

    private readonly IProcessingJobService _processingJobService;

    public ProcessingJobsController(AppDbContext context,IProcessingJobService processingJobService)
    {
        _context = context;
        _processingJobService = processingJobService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetJobStatus(int id)
    {
        if (!ModelState.IsValid)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }

        ProcessingJobResponseDto jobTrack = await _processingJobService.GetProcessingJobByIdAsync(id);

        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.Success,
            jobTrack
        ); 
    }
}