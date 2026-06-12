using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Users.Models;

namespace Users.Utils;

public interface IJwtService
{
    string GenerateJwtToken(User user, out int expiryMinutes);
}

public class JwtService : IJwtService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<JwtService> _logger;

    public JwtService(IConfiguration configuration, ILogger<JwtService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public string GenerateJwtToken(User user, out int expiryMinutes)
    {
        _logger.LogInformation("Retrieving configuration settings and claims to generate JWT token for UserId: {UserId}", user.Id);

        var jwtSettings = _configuration.GetSection("Jwt");
        var secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key not configured.");
        
        if (!int.TryParse(jwtSettings["ExpiryMinutes"], out expiryMinutes))
        {
            expiryMinutes = 60;
        }

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim("UserId", user.Id.ToString()),
            new Claim("Username", user.Username),
            new Claim("Role", user.Role.ToString())
        };

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = credentials
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);
        
        _logger.LogInformation("JWT token successfully generated for UserId: {UserId}", user.Id);

        return tokenHandler.WriteToken(token);
    }
}