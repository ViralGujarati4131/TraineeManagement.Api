using Microsoft.EntityFrameworkCore;
using Users.Models;
using Users.Service.Interface;
using Users.DTOs;
using Users.Utils;
using Microsoft.AspNetCore.Identity;

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

    private PasswordVerificationResult VerifyPassword(User u, string password, string hashedPassword)
    {
        PasswordHasher<User> ps = new PasswordHasher<User>();   
        return ps.VerifyHashedPassword(u, hashedPassword, password);

    }
    private LoginTokenResponseDto makeResponse(string t, int i, User u)
    {
        _logger.LogInformation("Creating response for successfull login");
        return new LoginTokenResponseDto
        {
            token = t,
            expiresIn = i,
            user = new UserResponseDto
            {
                Id = u.Id,
                Username = u.Username,
                Role = u.Role
            }
        };
    }

    private async Task<User?> FetchUser(string username)
    {
        User? findUser = await _context.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (findUser == null)
        {
            _logger.LogInformation("User not found");
            return null;
        }
        return findUser;
    }

    public async Task<LoginTokenResponseDto?> LoginUserAsync(UserLoginDto userLogin)
    {
        _logger.LogDebug($"Try to find the user {userLogin.Username}");
        User? findUser = await FetchUser(userLogin.Username);
        if (findUser == null)
        {
            _logger.LogInformation("User not found");
            return null;
        }
        _logger.LogDebug("Start verifing the password");
        if (PasswordVerificationResult.Success == VerifyPassword(findUser, userLogin.Password, findUser.PasswordHash))
        {
            _logger.LogInformation("Password varified now go for creating the jwt token");
            var token = _jwtService.GenerateJwtToken(findUser, out int expiryMinutes);
            _logger.LogInformation("JwtToken created");
            return makeResponse(token, expiryMinutes * 60, findUser);
        }
        return null;
    }
}