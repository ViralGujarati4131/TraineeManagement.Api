using Microsoft.AspNetCore.Mvc;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.Data.ResponseDescriptor;

namespace TraineeManagement.Api.ResponsesBuilder;

public static class CustomResponseBuilder
{
    public static ActionResult CreateValidationErrorResponse()
    {
        CustomResponseDescriptor descriptor = CustomResponse.ValidationError;

        return new ObjectResult(new
        {
            Code = descriptor.CustomCode,
            Message = descriptor.Message
        })
        {
            StatusCode = descriptor.HttpStatusCode
        };
    }
    public static ActionResult CreateSuccessResponse(CustomResponseDescriptor descriptor, object? data = null)
    {
        if (descriptor.HttpStatusCode == StatusCodes.Status204NoContent)
        {
            return new StatusCodeResult(StatusCodes.Status204NoContent);
        }

        return new ObjectResult(new
        {
            Code = descriptor.CustomCode,
            Message = descriptor.Message,
            Data = data
        })
        {
            StatusCode = descriptor.HttpStatusCode
        };
    }
}