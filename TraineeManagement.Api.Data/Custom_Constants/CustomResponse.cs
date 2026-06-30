using TraineeManagement.Api.Data.ResponseDescriptor;

namespace TraineeManagement.Api.Data.Response;

 public static class CustomResponse
 {
    public static readonly CustomResponseDescriptor LoginSuccess = 
        new(200, "2000", "User authenticated successfully. Security token issued.");

    public static readonly CustomResponseDescriptor DataRetrivedSuccess = 
        new(200, "2002", "Data retrieved successfully");
        
    public static readonly CustomResponseDescriptor DataInsertedSuccess = 
        new(201, "2001", "Data inserted successfully");
            
    public static readonly CustomResponseDescriptor DataUpdatedSuccess = 
        new(200, "2003", "Data updated successfully");

    public static readonly CustomResponseDescriptor DataDeletedNoContent = 
        new(204, "2004", "No content Data deleted succesfully");

    public static readonly CustomResponseDescriptor FileUploadAccepted = 
        new(202, "2005", "File Upload Accepted Successfully");
    
    public static readonly CustomResponseDescriptor StatusTrack = 
        new(200, "2006", "Current Status");


    //  CLIENT SIDE ERRORS  

    public static readonly CustomResponseDescriptor BadRequest = 
        new(400, "3001", "Invalid request argument.");
            
    public static readonly CustomResponseDescriptor UnprocessableEntity = 
        new(422, "3002", "Request data fails the validation rule.");
            
    public static readonly CustomResponseDescriptor Unauthorized = 
        new(401, "3000", "Invalid credential or authorization missing.");
            
    public static readonly CustomResponseDescriptor NotFound = 
        new(404, "3003", "Requested data was not found.");
            
    public static readonly CustomResponseDescriptor FileNotFound = 
        new(404, "3004", "The requested file could not be located.");

    public static readonly CustomResponseDescriptor DataEntryNotFound = 
        new(404, "3005", "Data not found for requested id.");

    public static readonly CustomResponseDescriptor FileNotAttached = 
        new(400, "3006", "No files were attached to the upload request.");

    public static readonly CustomResponseDescriptor FileEmpty = 
        new(400, "3007", "File can not be empty.");

    public static readonly CustomResponseDescriptor FileSizeExcced = 
        new(400, "3008", "File size exceeds the allow limit.");

    public static readonly CustomResponseDescriptor FileExtentionNotAllowed = 
        new(400, "3009", "File extention is not allowed.");

    public static readonly CustomResponseDescriptor FileContentMismatch = 
        new(400, "3010", "File conent not match with provided extention.");


    //  DATABASE SPECIFIC CLIENT ERRORS 

    public static readonly CustomResponseDescriptor SqlReferenceConflict = 
        new(409, "6001", "Some of the provided reference identifiers conflict with data rules");
        
    public static readonly CustomResponseDescriptor SqlDeleteReferenceError = 
        new(409, "6002", "Delete operation could not be completed because of existing data references");
        
    public static readonly CustomResponseDescriptor UsernameExists = 
        new(409, "6000", "Username already exists.");

    public static readonly CustomResponseDescriptor FileAlreadyUploaded = 
        new(409, "6003", "This file is already uploaded.");

    
    // SERVER ERRORS 
    
    public static readonly CustomResponseDescriptor InternalServerError = 
        new(500, "5000", "Something went wrong internally, please try again");
            
    public static readonly CustomResponseDescriptor JwtOperationError = 
        new(500, "5001", "An unexpected error occurred while processing jwt Token");

    public static readonly CustomResponseDescriptor ConfigurationMissingError = 
        new(500, "5002", "Configuration mismatch: Configuration was not found");
            
    public static readonly CustomResponseDescriptor DataSeedingError = 
        new(500, "5003", "Data seeding failed.");

    public static readonly CustomResponseDescriptor JsonConversionError = 
        new(500, "5004", "Json Conversion Failed");

    public static readonly CustomResponseDescriptor IOFail = 
        new(500, "5005", "Exception occur while doing the IO Operation");
    
    public static readonly CustomResponseDescriptor ChannelNotInitialized = 
        new(500, "5006", "Channel is not initialized.");

    public static readonly CustomResponseDescriptor UnavailableRabbitMQService = 
        new(503, "5007", "Submission saved but could not be queued for processing.");

    public static readonly CustomResponseDescriptor RequestTimeOut = 
        new(504, "5008", "Upstream service timed out.");
    
    public static readonly CustomResponseDescriptor ServiceTemporaryUnavailable = 
        new(503, "5009", "Upstream service temporarily unavailable.");

    public static readonly CustomResponseDescriptor MicroServiceUnreachable = 
        new(502, "5010", "Upstream service unreachable.");
}