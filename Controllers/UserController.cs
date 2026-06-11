using Microsoft.AspNetCore.Mvc;
using Users.DTOs;
using Users.Service.Interface;
using Users.Service;

namespace Users.Controllers;

[ApiController]
[Route("api/auth")]

public class UserController : ControllerBase
{
    private readonly IUserService userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService service, ILogger<UserController> logger)
    {
        userService = service;
        _logger = logger;

    }

    [HttpPost("login")]
    public async Task<ActionResult<LoginTokenResponseDto>> LoginUser(UserLoginDto user)
    {
        _logger.LogInformation($"{user.Username} Try to login");
        LoginTokenResponseDto? userAuth = await userService.LoginUserAsync(user);
        if (userAuth == null)
        {
            _logger.LogWarning("Login falied unauthorized User");
            return Unauthorized();
        }
        _logger.LogInformation($"{user.Username} Login Successfully");
        return Ok(userAuth);
    }
}