using StackExchange.Redis;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.Response;

namespace TraineeManagement.Api.Configuration;

public static class RedisConnection
{
    public static IServiceCollection AddRedisConnection(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        logger.LogInformation("Registering distributed cache engine client dependencies.");
        
        services.AddSingleton<IConnectionMultiplexer>(sp =>
        {
            string? connectionString = configuration[AppConstants.ConfigSections.GetRedisConnection];

            if(connectionString == null)
                throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
             
            return ConnectionMultiplexer.Connect(connectionString);
        });

        return services;
    }
}