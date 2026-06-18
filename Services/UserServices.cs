using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Users.DTOs;
using TraineeManagementApi.Users.Models;
using TraineeManagementApi.Users.ServiceInterface;
using TraineeManagementApi.Utils.JwtService;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.Users.Service;

public class UserService : IUserService
{
    private readonly AppDbContext _context;

    private readonly IJwtService _jwtService;

    private readonly ILogger<UserService> _logger;

    public UserService(AppDbContext context, IJwtService jwtService, ILogger<UserService> logger)
    {
        _context = context;
        _jwtService = jwtService;
        _logger = logger;
    }

    private PasswordVerificationResult VerifyPassword(User user, string password, string hashedPassword)
    {
        PasswordHasher<User> passwordHasher = new PasswordHasher<User>();
        return passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
    }

    private LoginTokenResponseDto MapToLoginResponseDto(string token, int expiresInSeconds, User user)
    {
        _logger.LogInformation("Creating response object for successful login session of Username: {Username}", user.Username);
        
        return new 
        LoginTokenResponseDto
        (
            token,
            expiresInSeconds,
            new UserResponseDto
            (
                user.Id,
                user.Username,
                user.Role
            )
        );
    }

    private async Task<User> FetchUserByUsernameInternalAsync(string username)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            _logger.LogWarning("User with Username {Username} was not found in the database", username);

            throw new NotFoundException("User");
        }
        return user;
    }

    public async Task<LoginTokenResponseDto> LoginUserAsync(UserLoginDto userLoginDto)
    {
        _logger.LogDebug("Attempting to find user record for Username: {Username}", userLoginDto.Username);

        User? user = await FetchUserByUsernameInternalAsync(userLoginDto.Username);

        _logger.LogDebug("Verifying password credentials for Username: {Username}", userLoginDto.Username);

        PasswordVerificationResult verificationResult = VerifyPassword(user, userLoginDto.Password, user.PasswordHash);

        if (verificationResult == PasswordVerificationResult.Success)
        {
            _logger.LogInformation("Password successfully verified. Initiating JWT token generation for Username: {Username}", user.Username);
            
            string token = _jwtService.GenerateJwtToken(user, out int expiryMinutes);
            int expiresInSeconds = expiryMinutes * 60;
            
            _logger.LogInformation("JWT Token successfully generated. Expiry: {ExpiryMinutes} minutes", expiryMinutes);
            
            return MapToLoginResponseDto(token, expiresInSeconds, user);
        }
        _logger.LogWarning("Invalid password credential provided for Username: {Username}", userLoginDto.Username);
        
        throw new UnauthorizedException(AppConstants.Errors.Users.InvalidCredentials);
    }
}