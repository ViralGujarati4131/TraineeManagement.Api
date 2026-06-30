using TraineeManagement.Api.Data.Constants;

namespace TraineeManagement.Api.CorrelationIdMiddleware;

public class CorrelationIdMiddleware
{
    private const string HeaderName = AppConstants.ConfigSections.CorrelationIdHeaderName;
    private readonly RequestDelegate _next;
    public CorrelationIdMiddleware(RequestDelegate next) => _next = next;

    public async Task InvokeAsync(HttpContext context)
    {
        string correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existing)
            && !string.IsNullOrWhiteSpace(existing)
                ? existing.ToString()
                : Guid.NewGuid().ToString();

        context.Items[HeaderName] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        using (Serilog.Context.LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}