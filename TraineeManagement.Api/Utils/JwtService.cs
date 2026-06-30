using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.UserModel;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.ResponsesBuilder;

namespace TraineeManagement.Api.JwtService;

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
        _logger.LogInformation("Generating token parameters. UserId: {UserId}", user.Id);

        IConfigurationSection jwtSettings = _configuration.GetSection(AppConstants.ConfigSections.GetJwt);
        
        string secretKey = jwtSettings[AppConstants.ConfigSections.JwtKey] ?? throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);

        if (!int.TryParse(jwtSettings[AppConstants.ConfigSections.JwtExpiryMinute], out expiryMinutes))
        {
            expiryMinutes = AppConstants.Security.DefaultExpiryMinutes;
        }

        SymmetricSecurityKey securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        SigningCredentials credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        Claim[] claims = new[]
        {
            new Claim(AppConstants.Security.ClaimId, user.Id.ToString()),
            new Claim(AppConstants.Security.ClaimUsername, user.Username),
            new Claim(AppConstants.Security.ClaimRole, user.Role.ToString() ?? AppConstants.Security.DefaultRole)
        };

        SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(expiryMinutes),
            Issuer = jwtSettings[AppConstants.ConfigSections.JwtIssuer],
            Audience = jwtSettings[AppConstants.ConfigSections.JwtAudience],
            SigningCredentials = credentials
        };

        JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
        SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);

        string generatedToken = tokenHandler.WriteToken(token);
        
        if (string.IsNullOrWhiteSpace(generatedToken))
        {
            _logger.LogError("Dependency failure: Generation failed. UserId: {UserId}", user.Id);
            throw new JwtOperationException(CustomResponse.JwtOperationError);
        }

        _logger.LogInformation("Token generation complete. UserId: {UserId}", user.Id);
        return generatedToken;
    }
}