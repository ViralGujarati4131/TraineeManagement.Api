namespace TraineeManagementApi.FileStorage.ServiceInterface;

public interface IFileStorageService
{
    Task<string> uploadFileAsync(int submissionId,Stream fileStream, string originalFileName, string contentType);
    Task<Stream> downloadFileAsync(int id);

    Task deleteFileAsync(int id);
}

