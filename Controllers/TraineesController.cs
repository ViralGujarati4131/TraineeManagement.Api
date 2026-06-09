using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.DTOs;
using TraineeManagementApi.Models;
using TraineeManagementApi.Service.Interface;

namespace TraineeManagement.Api.Controllers;

[ApiController]
[Route("api/trainee")]
public class TraineesController : ControllerBase
{
    private readonly ITraineeService traineeService;
    public TraineesController(ITraineeService service)
    {
        traineeService = service;
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TraineeResponseDto>> GetTraineeById(int id)
    {
        TraineeResponseDto? traineeById = await traineeService.GetTraineeByIdAsync(id);
        if (traineeById == null)
        {
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        return Ok(traineeById);
    }

    [HttpPost]
    public async Task<ActionResult<TraineeResponseDto>> CreateTrainee([FromBody] CreateTraineeDto createTrainee)
    {
        TraineeResponseDto t = await traineeService.CreateTraineeAsync(createTrainee);
        return CreatedAtAction(nameof(GetTraineeById), new { id = t.Id }, t);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<TraineeResponseDto>> UpdateTraineeById(int id, [FromBody] UpdateTraineeDto updatedTrainee)
    {
        TraineeResponseDto? trainee = await traineeService.UpdateTraineeAsync(id, updatedTrainee);
        if (trainee == null)
        {
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        return Ok(trainee);
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteTraineeById(int id)
    {
        if (!await traineeService.DeleteTraineeByIdAsync(id))
        {
            return NotFound(new { Message = $"Trainee with Id {id} not found" });
        }
        return NoContent();
    }

    [HttpGet]
    public async Task<ActionResult<TraineeResponseDto>> GetTrainee([FromQuery] string? searchTrainee)
    {
        if (searchTrainee == null)
        {
            IEnumerable<TraineeResponseDto> traineeAll = await traineeService.GetTraineeAsync();
            if (!traineeAll.Any())
            {
                return NotFound(new { Message = "No trainee found" });
            }
            return Ok(traineeAll);
        }
        IEnumerable<TraineeResponseDto> searchResult = await traineeService.SearchTraineesAsync(searchTrainee);
        if (!searchResult.Any())
        {
            return NotFound(new { Message = $"No trainee found matching '{searchTrainee}'" });
        }
        return Ok(searchResult);
    }

}


