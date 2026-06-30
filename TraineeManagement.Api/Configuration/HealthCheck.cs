using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Messaging.RabbitMqConnection;

namespace TraineeManagement.Api.Configuration;

public static class HealthCheck
{
    public static IServiceCollection AddHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHealthChecks()
            .AddMySql(
                connectionString: configuration[AppConstants.ConfigSections.GetDbConnection]!,
                name: "mysql",
                tags: ["readiness", "db"])
            .AddRedis(
                redisConnectionString: configuration[AppConstants.ConfigSections.GetRedisConnection]!,
                name: "redis",
                tags: ["readiness", "cache"])
            .AddRabbitMQ(
                sp =>
                {
                    RabbitConnection rabbitConn = sp.GetRequiredService<RabbitConnection>();
                    return rabbitConn.Connection!;
                },
                name: "rabbitmq",
                tags: ["readiness", "messaging"])
            .AddUrlGroup(
                uri: new Uri(configuration[AppConstants.ConfigSections.GetMicroServiceUrl] + AppConstants.ConfigSections.GetMicroServiceHealthRoute),
                name: "directory-service",
                tags: ["readiness", "upstream"]);

        return services;
    }

    public static WebApplication MapAppHealthChecks(this WebApplication app)
    {
        app.MapHealthChecks(AppConstants.ConfigSections.HealthCheckLivenessRoute, new HealthCheckOptions
        {
            Predicate = _ => false,
            ResponseWriter = WriteHealthResponse
        });

        app.MapHealthChecks(AppConstants.ConfigSections.HealthCheckReadinessRoute, new HealthCheckOptions
        {
            Predicate = check => check.Tags.Contains("readiness"),
            ResponseWriter = WriteHealthResponse
        });

        return app;
    }

    private static Task WriteHealthResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = AppConstants.ConfigSections.ContentType;
        return context.Response.WriteAsync(System.Text.Json.JsonSerializer.Serialize(new
        {
            Status = report.Status == HealthStatus.Healthy ? AppConstants.ConfigSections.HealthReady : AppConstants.ConfigSections.HealthUnavailable,
            Timestamp = DateTime.UtcNow
        }));
    }
}