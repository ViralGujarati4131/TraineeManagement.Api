using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TraineeManagement.Api.Controllers;

[ApiController]
[Route("api/health")]
public class HealthController : ControllerBase
{
    ILogger<HealthController> _logger;
    public HealthController(ILogger<HealthController> logger)
    {
        _logger = logger;
    }
    [HttpGet]
    public IActionResult GetMessage()
    {
        _logger.LogInformation("Sending the health data");
        return Ok(new
        {
            status = "running",
            application = "Trainee Management API",
            timestamp = DateTime.UtcNow
        });
    }
}


