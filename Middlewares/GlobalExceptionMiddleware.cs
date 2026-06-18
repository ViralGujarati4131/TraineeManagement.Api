using TraineeManagementApi.Utils.CustomException;
using MySqlConnector;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.GlobalExceptionMiddleware;

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
        catch (NotFoundException ex)
        {
            _logger.LogWarning("Not Found Exception Intercepted: {Message}", ex.Message);
            await WriteResponse(context, StatusCodes.Status404NotFound, AppConstants.ApiResponse.CodeNotFound, AppConstants.ApiResponse.MsgNotFound);
        }
        catch (UnauthorizedException ex)
        {
            _logger.LogWarning("Unauthorized Exception Intercepted: {Message}", ex.Message);
            await WriteResponse(context, StatusCodes.Status401Unauthorized, AppConstants.ApiResponse.CodeUnauthorized, ex.Message);
        }
        catch (BadRequestException ex)
        {
            _logger.LogWarning("Bad Request Exception Intercepted: {Message}", ex.Message);
            await WriteResponse(context, StatusCodes.Status400BadRequest, AppConstants.ApiResponse.CodeBadRequest, AppConstants.ApiResponse.MsgBadRequest);
        }
        catch (JwtOperationException ex)
        {
            _logger.LogError(ex, "Invalid operation: {Message}", ex.Message);
            await WriteResponse(context, StatusCodes.Status500InternalServerError, AppConstants.ApiResponse.CodeServerError, AppConstants.Errors.JwtAuthError);
        }
        catch (Exception ex)
        {
            if (ex.InnerException is MySqlException mysqlEx)
            {
                _logger.LogError($"SQL Exception Occured Error Code : {mysqlEx.Number}");
                
                if (mysqlEx.Number == AppConstants.Database.MySqlErrorCodes.NotFoundReference)
                {
                    _logger.LogError("Reference operation Error: {ex}", ex);
                    await WriteResponse(context, StatusCodes.Status400BadRequest, AppConstants.ApiResponse.CodeBadRequest, AppConstants.Errors.SqlReferenceConflict);
                }
                else if (mysqlEx.Number == AppConstants.Database.MySqlErrorCodes.DeleteReference)
                {
                    _logger.LogError("Delete operation Referance Error: {ex}", ex);
                    await WriteResponse(context, StatusCodes.Status400BadRequest, AppConstants.ApiResponse.CodeBadRequest, AppConstants.Errors.SqlDeleteReferenceError);
                }
                else if (mysqlEx.Number == AppConstants.Database.MySqlErrorCodes.UsernameExists)
                {
                    _logger.LogError("Create operation Referance Error: {ex}", ex);
                    await WriteResponse(context, StatusCodes.Status400BadRequest, AppConstants.ApiResponse.CodeBadRequest, AppConstants.Errors.UsernameExists);
                }
            }
            else
            {
                _logger.LogError(ex, "Unhandled exception on {Method} {Path}", context.Request.Method, context.Request.Path);
                await WriteResponse(context, StatusCodes.Status500InternalServerError, AppConstants.ApiResponse.CodeServerError, AppConstants.Errors.GeneralInternalServerError);
            }
        }
    }

    private static async Task WriteResponse(HttpContext context, int statusCode, string customCode, string message)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(new
        {
            Code = customCode,
            Message = message
        });
    }
}