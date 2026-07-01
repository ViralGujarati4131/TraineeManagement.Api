using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.DatabaseContext;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.Response;

namespace TraineeManagement.Api.Configuration;

public static class MySqlConnection
{
    public static IServiceCollection AddMySqlConnection(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        string? connectionString = configuration[AppConstants.ConfigSections.GetDbConnection];

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            logger.LogCritical("Dependency failure: Database connection string is missing.");
            throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
        }
        
        MySqlServerVersion serverVersion;
        try
        {
            serverVersion = new MySqlServerVersion(AppConstants.ConfigSections.MySqlVersion);
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Dependency failure: Invalid MySQL server version configured.");
            throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
        }

        logger.LogInformation("Configuring database context.");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, serverVersion)
        );

        return services;
    }
}