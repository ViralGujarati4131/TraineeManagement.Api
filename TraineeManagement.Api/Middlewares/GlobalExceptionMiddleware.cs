using TraineeManagement.Api.Data.CustomException;
using MySqlConnector;
using TraineeManagement.Api.Data.ResponseDescriptor;
using TraineeManagement.Api.Data.Constants;
using TraineeManagement.Api.Data.Response;
using Polly.CircuitBreaker;

namespace TraineeManagement.Api.GlobalExceptionMiddleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;

    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (BaseApplicationException ex)
        {
            _logger.LogWarning(
                "Application exception caught. ExceptionType={ExceptionType}, Path={Path}, StatusCode={StatusCode}, ErrorCode={ErrorCode}",
                ex.GetType().Name,
                context.Request.Path,
                ex.Descriptor.HttpStatusCode,
                ex.Descriptor.CustomCode);

            await WriteResponseAsync(context, ex.Descriptor);
        }
        catch (TaskCanceledException)
        {
            _logger.LogWarning("Upstream timeout. Path={Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status504GatewayTimeout;
            await context.Response.WriteAsJsonAsync(CustomResponse.RequestTimeOut);
        }
        catch (BrokenCircuitException)
        {
            _logger.LogError("Circuit breaker open. Path={Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            await context.Response.WriteAsJsonAsync(CustomResponse.ServiceTemporaryUnavailable);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Upstream connection failure. Path={Path}", context.Request.Path);
            context.Response.StatusCode = StatusCodes.Status502BadGateway;
            await context.Response.WriteAsJsonAsync(CustomResponse.MicroServiceUnreachable);
        }
        catch (Exception ex)
        {
            if (ex is MySqlException mysqlEx || ex.InnerException is MySqlException { } nestedMysqlEx && (mysqlEx = nestedMysqlEx) != null)
            {
                _logger.LogError("Database dependency failure. Path={Path}, MySqlErrorCode={MySqlErrorCode}",
                    context.Request.Path,
                    mysqlEx.Number);

                CustomResponseDescriptor dbDescriptor = mysqlEx.Number switch
                {
                    AppConstants.Database.MySqlErrorCodes.NotFoundReference => CustomResponse.SqlReferenceConflict,
                    AppConstants.Database.MySqlErrorCodes.DeleteReference => CustomResponse.SqlDeleteReferenceError,
                    AppConstants.Database.MySqlErrorCodes.UsernameExists => CustomResponse.UsernameExists,
                    _ => CustomResponse.InternalServerError
                };

                await WriteResponseAsync(context, dbDescriptor);
            }
            else
            {
                _logger.LogError(
                    ex,
                    "Unhandled exception. Path={Path}, Method={Method}",
                    context.Request.Path,
                    context.Request.Method);

                await WriteResponseAsync(context, CustomResponse.InternalServerError);
            }
        }
    }

    private static async Task WriteResponseAsync(HttpContext context, CustomResponseDescriptor descriptor)
    {
        context.Response.StatusCode = descriptor.HttpStatusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            Code = descriptor.CustomCode,
            Message = descriptor.Message
        });
    }
}