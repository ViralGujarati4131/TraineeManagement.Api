using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Users.Models;

namespace TraineeManagementApi.Utils.JwtService;

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

        IConfigurationSection jwtSettings = _configuration.GetSection("Jwt");
        string secretKey = jwtSettings["Key"] ?? throw new InvalidOperationException("JWT Secret Key not configured.");

        if (!int.TryParse(jwtSettings["ExpiryMinutes"], out expiryMinutes))
        {
            expiryMinutes = 60;
        }

        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[]
        {
            new Claim("Id", user.Id.ToString()),
            new Claim("Username", user.Username),
            new Claim("Role", user.Role.ToString())
        };

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = jwtSettings["Issuer"],
            Audience = jwtSettings["Audience"],
            SigningCredentials = credentials
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        _logger.LogInformation("JWT token successfully generated for UserId: {UserId}", user.Id);

        string generatedToken = tokenHandler.WriteToken(token);
        if (generatedToken == null)
        {
            throw new JwtOperationException();
        }
        return generatedToken;
    }
}