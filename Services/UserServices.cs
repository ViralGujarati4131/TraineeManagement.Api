using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Users.DTOs;
using Users.Models;
using Users.Service.Interface;
using Users.Utils;

namespace Users.Service;

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
        var passwordHasher = new PasswordHasher<User>();   
        return passwordHasher.VerifyHashedPassword(user, hashedPassword, password);
    }

    private LoginTokenResponseDto MapToLoginResponseDto(string token, int expiresInSeconds, User user)
    {
        _logger.LogInformation("Creating response object for successful login session of Username: {Username}", user.Username);
        
        return new LoginTokenResponseDto
        {
            Token = token,
            ExpiresIn = expiresInSeconds,
            User = new UserResponseDto
            {
                Id = user.Id,
                Username = user.Username,
                Role = user.Role
            }
        };
    }

    private async Task<User?> FetchUserByUsernameInternalAsync(string username)
    {
        User? user = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null)
        {
            _logger.LogWarning("User with Username {Username} was not found in the database", username);
            return null;
        }
        return user;
    }

    public async Task<LoginTokenResponseDto?> LoginUserAsync(UserLoginDto userLoginDto)
    {
        _logger.LogDebug("Attempting to find user record for Username: {Username}", userLoginDto.Username);
        
        User? user = await FetchUserByUsernameInternalAsync(userLoginDto.Username);
        if (user == null)
        {
            return null;
        }

        _logger.LogDebug("Verifying password credentials for Username: {Username}", userLoginDto.Username);
        
        var verificationResult = VerifyPassword(user, userLoginDto.Password, user.PasswordHash);
        if (verificationResult == PasswordVerificationResult.Success)
        {
            _logger.LogInformation("Password successfully verified. Initiating JWT token generation for Username: {Username}", user.Username);
            
            var token = _jwtService.GenerateJwtToken(user, out int expiryMinutes);
            var expiresInSeconds = expiryMinutes * 60;
            
            _logger.LogInformation("JWT Token successfully generated. Expiry: {ExpiryMinutes} minutes", expiryMinutes);
            
            return MapToLoginResponseDto(token, expiresInSeconds, user);
        }

        _logger.LogWarning("Invalid password credential provided for Username: {Username}", userLoginDto.Username);
        return null;
    }
}