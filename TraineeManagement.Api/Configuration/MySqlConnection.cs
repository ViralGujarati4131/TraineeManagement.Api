using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.DatabaseContext;

namespace TraineeManagement.Api.Configuration;

public static class MySqlConnection
{
    public static IServiceCollection AddMySqlConnection(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        string? connectionString = configuration[AppConstants.ConfigSections.GetDbConnection];
        MySqlServerVersion serverVersion = new MySqlServerVersion(AppConstants.ConfigSections.MySqlVersion);

        logger.LogInformation("Configuring database context.");
        services.AddDbContext<AppDbContext>(options =>
            options.UseMySql(connectionString, serverVersion)
        );

        return services;
    }
}