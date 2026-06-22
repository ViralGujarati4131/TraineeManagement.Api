namespace TraineeManagementApi.FileStorage.ServiceInterface;
public interface IFileStorageService
{
    Task<string?> SaveAsync(int submissionId,IFormFile file);
    Task<Stream> OpenReadAsync(string storageFileName);
    Task<bool> ExistsAsync(string storageFileName);
    Task DeleteAsync(string storageFileName);
}