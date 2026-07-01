
using TraineeManagement.Api.Messaging.RabbitMqConnection;
using TraineeManagement.Api.Messaging.RabbitMqConnectionSettings;
using TraineeManagement.Api.Data.Constants;

namespace TraineeManagement.Api.Configuration;

public static class RabbitMqConnection
{
    public static IServiceCollection AddRabbitMqConnection(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<RabbitConnection>();
        services.Configure<RabbitMqSettings>(configuration.GetSection(AppConstants.ConfigSections.GetRabbitMqSettings));

        return services;
    }

    public static async Task InitializeMessagingAsync(this WebApplication app, ILogger logger)
    {
        using IServiceScope scope = app.Services.CreateScope();
        RabbitConnection conn = scope.ServiceProvider.GetRequiredService<RabbitConnection>();
        try
        {
            await conn.InitializeAsync();
            logger.LogInformation("Publish/consume connection initialized successfully.");
        }
        catch (Exception ex)
        {
            logger.LogCritical(ex, "Dependency failure: RabbitMq connection initialization collapsed during application startup.");
            throw;
        }
    }
}