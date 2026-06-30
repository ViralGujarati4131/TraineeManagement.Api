using System.Net;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.TraineeService;
using TraineeManagement.Api.TraineeServiceInterface;
using Polly;
using Polly.Retry;
using Polly.CircuitBreaker;
using Polly.Fallback;
using Polly.Extensions.Http;

namespace TraineeManagement.Api.Configuration;

public static class HttpClient
{
    public static IServiceCollection AddHttpClient(this IServiceCollection services, IConfiguration configuration, ILogger logger)
    {
        AsyncFallbackPolicy<HttpResponseMessage> fallbackPolicy = Policy<HttpResponseMessage>
            .Handle<HttpRequestException>()
            .Or<TaskCanceledException>()
            .Or<BrokenCircuitException>()
            .FallbackAsync(
                fallbackValue: new HttpResponseMessage(HttpStatusCode.ServiceUnavailable),
                onFallbackAsync: (outcome, context) =>
                {
                    logger.LogWarning("Fallback triggered. Upstream unavailable. Reason: {Reason}", outcome.Exception?.Message);
                    return Task.CompletedTask;
                });

        AsyncRetryPolicy<HttpResponseMessage> retryPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt =>
                    TimeSpan.FromMilliseconds(200 * retryAttempt),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    logger.LogWarning("Retry attempt. AttemptCount: {Attempt}, DelayMs: {Delay}, Reason: {Reason}",
                        retryAttempt, 
                        timespan.TotalMilliseconds, 
                        outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString());
                });

        AsyncCircuitBreakerPolicy<HttpResponseMessage> circuitBreakerPolicy = HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                handledEventsAllowedBeforeBreaking: 5,
                durationOfBreak: TimeSpan.FromSeconds(15),
                onBreak: (outcome, duration) =>
                {
                    logger.LogError("Dependency failure: Circuit breaker tripped open. DurationSec: {Duration}", duration.TotalSeconds);
                },
                onReset: () =>
                {
                    logger.LogInformation("State transition: Circuit breaker reset to closed.");
                });

        services.AddHttpContextAccessor();

        services.AddHttpClient<ITraineeService, TraineeServices>((sp, client) =>
        {
            string? baseUrl = configuration["DirectoryService:BaseUrl"];
            if (string.IsNullOrWhiteSpace(baseUrl))
            {
                logger.LogCritical("Dependency failure: Base address missing for Directory TraineeService configuration lookup.");
                throw new ConfigurationMissingException(CustomResponse.ConfigurationMissingError);
            }

            client.BaseAddress = new Uri(baseUrl);
            client.Timeout = TimeSpan.FromSeconds(5); 
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            const string HeaderName = "X-Correlation-ID";
            IHttpContextAccessor httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            string? correlationId = httpContextAccessor.HttpContext?.Items[HeaderName]?.ToString();
            if (!string.IsNullOrWhiteSpace(correlationId))
            {
                if (!client.DefaultRequestHeaders.Contains(HeaderName))
                {
                    client.DefaultRequestHeaders.Add(HeaderName, correlationId);
                }
            }
        })
        .AddPolicyHandler(fallbackPolicy)      
        .AddPolicyHandler(retryPolicy)       
        .AddPolicyHandler(circuitBreakerPolicy); 

        return services;
    }
}