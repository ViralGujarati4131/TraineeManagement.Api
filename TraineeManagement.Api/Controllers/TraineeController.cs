using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagement.Api.Data.TraineeDTO;
using TraineeManagement.Api.TraineeServiceInterface;
using TraineeManagement.Api.ResponsesBuilder;
using TraineeManagement.Api.Data.ConstRoute;
using TraineeManagement.Api.Data.Response;

namespace TraineeManagement.Api.TraineeController;

[ApiController]
[Route(CustomConstRoute.Trainees)]
[Authorize]
public class TraineesController : ControllerBase
{
    private readonly ITraineeService _traineeService;

    private readonly ILogger<TraineesController> _logger;

    public TraineesController(ITraineeService traineeService, ILogger<TraineesController> logger)
    {
        _traineeService = traineeService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult> GetTraineeById(int id,CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }
        _logger.LogDebug("Invoking trainee service to retrieve profile for TraineeId: {TraineeId}", id);
        
        TraineeResponseDto? trainee = await _traineeService.GetTraineeByIdAsync(id,cancellationToken);
        
        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.Success,
            trainee
        );
    }

    [HttpPost]
    public async Task<ActionResult> CreateTrainee([FromBody] TraineeCreateDto createTraineeDto)
    {
        if (!ModelState.IsValid)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }
        _logger.LogDebug("Invoking trainee service to establish a new trainee registration");
        
        TraineeResponseDto createdTrainee = await _traineeService.CreateTraineeAsync(createTraineeDto);
        
        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.Created,
            createdTrainee
        );
    }

    [HttpPut("{id}")]
    public async Task<ActionResult> UpdateTraineeById(int id, [FromBody] TraineeUpdateDto updateTraineeDto)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }
        _logger.LogDebug("Invoking trainee service to modify records for TraineeId: {TraineeId}", id);
        
        TraineeResponseDto updatedTrainee = await _traineeService.UpdateTraineeAsync(id, updateTraineeDto);
        
        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.Updated,
            updatedTrainee
        );
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTraineeById(int id)
    {
        if (!ModelState.IsValid || id < 1)
        {
            return CustomResponseBuilder.CreateValidationErrorResponse();
        }
        _logger.LogDebug("Invoking trainee service to delete records for TraineeId: {TraineeId}", id);
        
        await _traineeService.DeleteTraineeByIdAsync(id);
        
        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.NoContent
        );
    }

    [HttpGet]
    public async Task<ActionResult> GetTrainees([FromQuery] string? searchTrainee)
    {
        if (searchTrainee == null)
        {
            _logger.LogDebug("Invoking trainee service to fetch all trainees");

            IEnumerable<TraineeResponseDto> trainees = await _traineeService.GetTraineesAsync();

            return CustomResponseBuilder.CreateSuccessResponse(
                CustomResponse.Success,
                trainees
            );
        }
        _logger.LogDebug("Invoking trainee service to query profiles matching search criteria: {SearchTerm}", searchTrainee);
        
        IEnumerable<TraineeResponseDto> searchResults = await _traineeService.SearchTraineesAsync(searchTrainee);
        
        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.Success,
            searchResults
        );
    }

    [HttpGet(CustomConstRoute.PaginationSearch)]
    public async Task<ActionResult> PaginationSearchTrainee([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? name, [FromQuery] string? status)
    {
        if (pageNumber <= 0 || pageSize <= 0 || name == null || status == null)
        {
            _logger.LogWarning("Pagination request processing aborted due to missing or invalid filter arguments");

            return CustomResponseBuilder.CreateValidationErrorResponse();
        }
        _logger.LogDebug("Invoking trainee service to generate paginated lookup - Name: {FilterName}, Status: {FilterStatus}, Page: {PageNumber}, Size: {PageSize}", name, status, pageNumber, pageSize);
        
        TraineePaginationSearchDto? pagedData = await _traineeService.GetPagedAndSearchedTraineesAsync(pageNumber, pageSize, name, status);
        
        return CustomResponseBuilder.CreateSuccessResponse(
            CustomResponse.Success,
            pagedData
        );
    }
}