using Microsoft.AspNetCore.Mvc;
using TraineeManagement.Api.Data.ConstRoute;
using TraineeManagement.Api.Data.TraineeDTO;
using TraineeManagement.Api.ResponsesBuilder;
using TrainingDirectory.Api.DirectoryTraineeServiceInterface;

namespace TrainingDirectory.Api.TraineeControllers;

[ApiController]
[Route(CustomConstRoute.DirectoryTrainee)]
public class TraineeController : ControllerBase
{
    private readonly IDirectoryTraineeService _directoryTraineeService;

    public TraineeController(IDirectoryTraineeService directoryTraineeService)
    {
        _directoryTraineeService = directoryTraineeService;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TraineeResponseDto>> GetTraineeProfileByIdAsync(int id, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }

       TraineeResponseDto trainee = await _directoryTraineeService.GetTraineeByIdAsync(id,cancellationToken);

        return trainee;        
    }
}