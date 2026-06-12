using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.DTOs;
using TraineeManagementApi.Models;
using TraineeManagementApi.Service.Interface;

namespace TraineeManagement.Api.Controllers;

[ApiController]
[Route("api/trainee")]
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
    public async Task<ActionResult<TraineeResponseDto>> GetTraineeById(int id)
    {
        _logger.LogDebug("Invoking trainee service to retrieve profile for TraineeId: {TraineeId}", id);
        
        TraineeResponseDto? trainee = await _traineeService.GetTraineeByIdAsync(id);
        if (trainee == null)
        {
            _logger.LogWarning("Retrieval failed. Trainee with ID {TraineeId} was not found", id);
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        
        return Ok(trainee);
    }

    [HttpPost]
    public async Task<ActionResult<TraineeResponseDto>> CreateTrainee([FromBody] CreateTraineeDto createTraineeDto)
    {
        _logger.LogDebug("Invoking trainee service to establish a new trainee registration");
        
        TraineeResponseDto createdTrainee = await _traineeService.CreateTraineeAsync(createTraineeDto);
        
        return CreatedAtAction(nameof(GetTraineeById), new { id = createdTrainee.Id }, createdTrainee);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TraineeResponseDto>> UpdateTraineeById(int id, [FromBody] UpdateTraineeDto updateTraineeDto)
    {
        _logger.LogDebug("Invoking trainee service to modify records for TraineeId: {TraineeId}", id);
        
        TraineeResponseDto? updatedTrainee = await _traineeService.UpdateTraineeAsync(id, updateTraineeDto);
        if (updatedTrainee == null)
        {
            _logger.LogWarning("Update failed. Trainee with ID {TraineeId} was not found", id);
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        
        return Ok(updatedTrainee);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTraineeById(int id)
    {
        _logger.LogDebug("Invoking trainee service to delete records for TraineeId: {TraineeId}", id);
        
        bool isDeleted = await _traineeService.DeleteTraineeByIdAsync(id);
        if (!isDeleted)
        {
            _logger.LogWarning("Deletion failed. Trainee with ID {TraineeId} was not found", id);
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TraineeResponseDto>>> GetTrainees([FromQuery] string? searchTrainee)
    {
        if (searchTrainee == null)
        {
            _logger.LogDebug("Invoking trainee service to fetch all trainees");
            
            IEnumerable<TraineeResponseDto?> trainees = await _traineeService.GetTraineesAsync();
            if (!trainees.Any())
            {
                _logger.LogWarning("Trainee collection catalog is empty. No records found");
                return NotFound(new { Message = "No trainee found" });
            }
            
            return Ok(trainees);
        }

        _logger.LogDebug("Invoking trainee service to query profiles matching search criteria: {SearchTerm}", searchTrainee);
        
        IEnumerable<TraineeResponseDto?> searchResults = await _traineeService.SearchTraineesAsync(searchTrainee);
        if (!searchResults.Any())
        {
            _logger.LogWarning("Query execution returned zero matching results for criteria: {SearchTerm}", searchTrainee);
            return NotFound(new { Message = $"No trainee found matching '{searchTrainee}'" });
        }
        
        return Ok(searchResults);
    }

    [HttpGet("paginationSearch")]
    public async Task<ActionResult<PaginationSearchDto>> PaginationSearchTrainee([FromQuery] int pageNumber, [FromQuery] int pageSize, [FromQuery] string? name, [FromQuery] string? status)
    {
        if (pageNumber <= 0 || pageSize <= 0 || name == null || status == null)
        {
            _logger.LogWarning("Pagination request processing aborted due to missing or invalid filter arguments");
            return BadRequest(new { Message = "All fields are require" });
        }

        _logger.LogDebug("Invoking trainee service to generate paginated lookup - Name: {FilterName}, Status: {FilterStatus}, Page: {PageNumber}, Size: {PageSize}", name, status, pageNumber, pageSize);
        
        PaginationSearchDto? pagedData = await _traineeService.GetPagedAndSearchedTraineesAsync(pageNumber, pageSize, name, status);
        if (pagedData == null)
        {
            _logger.LogWarning("Paginated request no results on Page: {PageNumber} for target Name: {FilterName} and Status: {FilterStatus}", pageNumber, name, status);
            return NotFound(new { Message = $"No trainee found in page number {pageNumber} matching name = '{name}' status = '{status}'" });
        }
        
        return Ok(pagedData);
    }
}