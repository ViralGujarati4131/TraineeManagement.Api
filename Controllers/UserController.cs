using Microsoft.AspNetCore.Mvc;
using TraineeManagementApi.Users.DTOs;
using TraineeManagementApi.Users.ServiceInterface;
using TraineeManagementApi.Utils.ResponsesBuilder;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Users.Controller;

[ApiController]
[Route(AppConstants.Routes.Auth)]
public class UserController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UserController> _logger;

    public UserController(IUserService userService, ILogger<UserController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    [HttpPost(AppConstants.Routes.Login)]
    public async Task<ActionResult> LoginUser([FromBody] UserLoginDto userLoginDto)
    {
        if (!ModelState.IsValid)
        {
            return ResponseBuilder.CreateValidationErrorResponse(ModelState);
        }
        _logger.LogInformation("Login attempt initiated for Username: {Username}", userLoginDto.Username);

        LoginTokenResponseDto authenticationResult = await _userService.LoginUserAsync(userLoginDto);
        
        _logger.LogInformation("Authentication successful. Session token issued for Username: {Username}", userLoginDto.Username);
        
        return ResponseBuilder.CreateSuccessResponse(
            StatusCodes.Status200OK,
            AppConstants.ApiResponse.CodeSuccess,
            AppConstants.ApiResponse.MsgSuccess,
            authenticationResult
        );
    }
}