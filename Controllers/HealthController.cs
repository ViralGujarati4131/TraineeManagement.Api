using Microsoft.AspNetCore.Mvc;

namespace TraineeManagement.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    private readonly ILogger<HealthController> _logger;

    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public IActionResult GetMessage()
    {
        _logger.LogInformation("System health status requested");

        return Ok(new
        {
            Status = "running",
            Application = "Trainee Management API",
            Timestamp = DateTime.UtcNow
        });
    }
}