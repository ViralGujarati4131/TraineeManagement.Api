using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.Response;

namespace TraineeManagement.Api.Configuration;

public static class SetFrontendCors
{
    public const string AllowedOriginsPolicy = "_myAllowSpecificOrigins";

    public static IServiceCollection AddFrontendCors(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        
        string[]? allowedOrigins = configuration.GetSection(AppConstants.ConfigSections.GetFrontendCors).Get<string[]>();
        if (allowedOrigins == null || allowedOrigins.Length == 0)
        {
            logger.LogCritical("Dependency failure: Frontend CORS initialization parameters are missing.");
            throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
        }
        logger.LogInformation("Configuring CORS access profile layer.");
        services.AddCors(options =>
        {
            options.AddPolicy(name: AllowedOriginsPolicy, policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });

        return services;
    }
}