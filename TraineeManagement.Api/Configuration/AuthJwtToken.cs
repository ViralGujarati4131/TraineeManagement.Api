using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.Response;

namespace TraineeManagement.Api.Configuration;

public static class AuthJwtToken
{
    public static IServiceCollection AddJwtAuth(this IServiceCollection services, IConfiguration configuration)
    {
        IConfigurationSection jwtSettings = configuration.GetSection(AppConstants.ConfigSections.GetJwt);

        string jwtKeyString = jwtSettings[AppConstants.ConfigSections.JwtKey] 
            ?? throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);

        byte[] tokenSigningKey = Encoding.UTF8.GetBytes(jwtKeyString);

        string jwtIssuer = jwtSettings[AppConstants.ConfigSections.JwtIssuer]
            ?? throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);

        string jwtAudience = jwtSettings[AppConstants.ConfigSections.JwtAudience]
            ?? throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(tokenSigningKey)
            };
        });

        return services;
    }
}