using TraineeManagement.Api.Data.ResponseDescriptor;
using TraineeManagement.Api.Data.Response;

namespace TraineeManagement.Api.Data.CustomException;

public class BaseApplicationException : Exception
{
    public CustomResponseDescriptor Descriptor { get; }

    public BaseApplicationException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(customMessage ?? descriptor.Message)
    {
        Descriptor = descriptor with { Message = customMessage ?? descriptor.Message };
    }
}

public class NotFoundException : BaseApplicationException
{
    public NotFoundException(string? entityName = null) 
        : base(CustomResponse.NotFound, entityName != null ? $"{entityName} record was not found." : null) {}
}

public class UnauthorizedException : BaseApplicationException
{
    public UnauthorizedException() 
        : base(CustomResponse.Unauthorized) {}
}

public class BadRequestException : BaseApplicationException
{
    public BadRequestException(string customMessage) 
        : base(CustomResponse.BadRequest, customMessage) {}
}

public class ExceptionFileNotFound : BaseApplicationException
{
    public ExceptionFileNotFound() 
        : base(CustomResponse.FileNotFound) {}
}

public class JwtOperationException : BaseApplicationException
{
    public JwtOperationException() 
        : base(CustomResponse.JwtAuthError) {}
}

public class JwtSecretMissingException : BaseApplicationException
{
    public JwtSecretMissingException() 
        : base(CustomResponse.JwtSecretMissing) {}
}

public class FileStorageConfigurationException : BaseApplicationException
{
    public FileStorageConfigurationException() 
        : base(CustomResponse.FileStorageConfigError) {}
}

public class DataSeedingException : BaseApplicationException
{
    public DataSeedingException() 
        : base(CustomResponse.DataSeedingError) {}
}

public class JsonConversionException : BaseApplicationException
{
    public JsonConversionException() 
        : base(CustomResponse.JsonConversionError) {}
}