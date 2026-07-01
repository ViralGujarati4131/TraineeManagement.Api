using TraineeManagement.Api.Data.ResponseDescriptor;

namespace TraineeManagement.Api.Data.CustomException;

public class BaseApplicationException : Exception
{
    public CustomResponseDescriptor Descriptor 
    {
        get; 
    }
    public BaseApplicationException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(customMessage ?? descriptor.Message)
    {
        Descriptor = descriptor with { Message = customMessage ?? descriptor.Message };
    }
}
public class NotFoundException : BaseApplicationException
{
    public NotFoundException(CustomResponseDescriptor descriptor, string? entityName = null) 
        : base(descriptor, entityName != null ? $"{entityName} record was not found." : null) {}
}

public class BadRequestException : BaseApplicationException
{
    public BadRequestException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class UnauthorizedException : BaseApplicationException
{
    public UnauthorizedException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class FileNotFoundError : BaseApplicationException
{
    public FileNotFoundError(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class JwtOperationException : BaseApplicationException
{
    public JwtOperationException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class ConfigurationMissingException : BaseApplicationException
{
    public ConfigurationMissingException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class DataSeedingException : BaseApplicationException
{
    public DataSeedingException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class JsonConversionException : BaseApplicationException
{
    public JsonConversionException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class ConflictException : BaseApplicationException
{
    public ConflictException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class InternalServerException : BaseApplicationException
{
    public InternalServerException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class IOError : BaseApplicationException
{
    public IOError(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}

public class OperationException : BaseApplicationException
{
    public OperationException(CustomResponseDescriptor descriptor, string? customMessage = null) 
        : base(descriptor, customMessage) {}
}
