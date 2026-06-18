using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.ResponsesBuilder;

public static class ResponseBuilder
{
    public static ActionResult CreateValidationErrorResponse(ModelStateDictionary modelstate)
    {
        // var validationErrors = modelstate
        //     .Where(ms => ms.Value?.Errors.Count > 0)
        //     .Select(ms => new
        //     {
        //         Field = ms.Key,
        //         Errors = ms.Value?.Errors.Select(e => e.ErrorMessage).ToArray()
        //     });

        return new ObjectResult(new
        {
            Code = AppConstants.ApiResponse.CodeValidationError,
            Message = AppConstants.ApiResponse.MsgValidationError,
            // Errors = validationErrors
        })
        {
            StatusCode = StatusCodes.Status400BadRequest
        };
    }

    // Standard Success Envelope (2000, 2010, etc.)
    public static ActionResult CreateSuccessResponse(int httpStatus, string appCode, string sharedMessage, object? data = null)
    {
        if (httpStatus == StatusCodes.Status204NoContent)
        {
            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        return new ObjectResult(new
        {
            Code = appCode,
            Message = sharedMessage,
            Data = data
        })
        {
            StatusCode = httpStatus
        };
    }
}