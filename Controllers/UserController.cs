using Microsoft.AspNetCore.Mvc;
using Users.DTOs;
using Users.Service.Interface;

namespace Users.Controllers;

[ApiController]
[Route("api/auth")]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginTokenResponseDto>> LoginUser([FromBody] UserLoginDto userLoginDto)
    {
        _logger.LogInformation("Login attempt initiated for Username: {Username}", userLoginDto.Username);

        var authenticationResult = await _userService.LoginUserAsync(userLoginDto);
        if (authenticationResult == null)
        {
            _logger.LogWarning("Authentication failed. Invalid credentials for Username: {Username}", userLoginDto.Username);
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        _logger.LogInformation("Authentication successful. Session token issued for Username: {Username}", userLoginDto.Username);
        return Ok(authenticationResult);
    }
}