using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.DTOs;
using TraineeManagementApi.Service.Interface;

namespace TraineeManagement.Api.Controllers;

[ApiController]
[Route("api/trainee")]
[Authorize]
public class TraineesController : ControllerBase
{
    private readonly ITraineeService traineeService;
    private readonly ILogger<TraineesController> _logger;
    public TraineesController(ITraineeService service,ILogger<TraineesController> logger)
    {
        traineeService = service;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TraineeResponseDto>> GetTraineeById(int id)
    {
        _logger.LogDebug("Calling the tainee service for get user by id");
        TraineeResponseDto? traineeById = await traineeService.GetTraineeByIdAsync(id);
        if (traineeById == null)
        {
            _logger.LogWarning($"For get trainee with Id {id} not found");
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        return Ok(traineeById);
    }

    [HttpPost]
    public async Task<ActionResult<TraineeResponseDto>> CreateTrainee([FromBody] CreateTraineeDto createTrainee)
    {
        _logger.LogDebug("Calling the tainee service for creating trainee");
        TraineeResponseDto t = await traineeService.CreateTraineeAsync(createTrainee);
        return CreatedAtAction(nameof(GetTraineeById), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TraineeResponseDto>> UpdateTraineeById(int id, [FromBody] UpdateTraineeDto updatedTrainee)
    {
         _logger.LogDebug("Calling the trainee service for updating trainee by id");
        TraineeResponseDto? trainee = await traineeService.UpdateTraineeAsync(id, updatedTrainee);
        if (trainee == null)
        {
            _logger.LogWarning($"For update trainee with Id {id} not found");
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        return Ok(trainee);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTraineeById(int id)
    {
         _logger.LogDebug("Calling the trainee service for delete trainee by id");
        if (!await traineeService.DeleteTraineeByIdAsync(id))
        {
             _logger.LogWarning($"For delete trainee with Id {id} not found");
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<TraineeResponseDto>> GetTrainee([FromQuery] string? searchTrainee)
    {
        if (searchTrainee == null)
        {
            _logger.LogDebug("Calling the trainee service for get all trainee");
            IEnumerable<TraineeResponseDto> traineeAll = await traineeService.GetTraineeAsync();
            if (!traineeAll.Any())
            {
                _logger.LogWarning("No trainee found");
                return NotFound(new { Message = "No trainee found" });
            }
            return Ok(traineeAll);
        }
         _logger.LogDebug($"Calling the trainee service for get trainee whose name|email|techstack is {searchTrainee}");
        IEnumerable<TraineeResponseDto> searchResult = await traineeService.SearchTraineesAsync(searchTrainee);
        if (!searchResult.Any())
        {
             _logger.LogWarning($"No trainee found matching {searchTrainee}");
            return NotFound(new { Message = $"No trainee found matching '{searchTrainee}'" });
        }
        return Ok(searchResult);
    }

    [HttpGet("paginationSearch")]
    public async Task<ActionResult<PaginationSearchDto>> PaginationSearchTrainee([FromQuery] int pageNumber, int pageSize, string? name, string? status)
    {
        if (pageNumber <= 0 || pageSize <= 0 || name == null || status == null)
        {
            _logger.LogWarning("Search trainee with name|status with pagination require all parameter");
            return BadRequest(new { Message = "All fields are require" });
        }
         _logger.LogDebug($"Calling the trainee service for search trainee matching name is {name} status is {status} with pagination");
        PaginationSearchDto? getData = await traineeService.PaginationSearchTraineeAsync(pageNumber, pageSize, name, status);
        if (getData == null)
        {
            _logger.LogWarning($"No trainee found in page number {pageNumber} matching name = '{name}' status = '{status}'");
            return NotFound(new { Message = $"No trainee found in page number {pageNumber} matching name = '{name}' status = '{status}'"});
        }
        return Ok(getData);
    }

}


