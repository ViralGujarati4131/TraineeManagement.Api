using TraineeManagementApi.FileStorage.ServiceInterface;
using TraineeManagementApi.SubmissionFiles.Models;
using System.Security.Cryptography;
using TraineeManagementApi.Utils.CustomException;
using TraineeManagementApi.Submissions.Models;
using MySqlConnector;


namespace TraineeManagementApi.FileStorage.Service;

public class FileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<FileStorageService> _logger;
        
    private readonly AppDbContext _context;

    public FileStorageService(IWebHostEnvironment env,IConfiguration config,ILogger<FileStorageService> logger,AppDbContext context)
    {
        string configuredPath = config["FileStorage:RootPath"]
                    ?? throw new InvalidOperationException("FileStorage:RootPath not configured.");

        string basePath = env.ContentRootPath;
        var parent1 = Directory.GetParent(basePath);
        var parent2 = parent1 != null ? Directory.GetParent(parent1.FullName) : null;

        if (parent2 != null)
        {
            basePath = parent2.FullName;
        }
        else
        {
            basePath = env.ContentRootPath;
        }
                    
        _rootPath = Path.IsPathRooted(configuredPath)
            ? configuredPath
            : Path.Combine(basePath, configuredPath);
            
        if(!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }

        _logger = logger;
        _context = context;
    }

    private async Task<SubmissionFile> FetchSubmissionFileByIdInternalAsync(int id)
    {
        SubmissionFile? submissionFile = await _context.SubmissionFiles.FindAsync(id);
        if(submissionFile == null)
        {
            _logger.LogWarning("SubmissionFile with ID {FileId} was not found", id);

            throw new NotFoundException("SubmissionFile");
        }
        return submissionFile;
    }

    public async Task<string> uploadFileAsync(int submissionId,Stream fileStream, string originalFileName, string contentType)
    {
        Submission? submission = await _context.Submissions.FindAsync(submissionId);
        if(submission == null)
        {
            _logger.LogWarning("Submission with ID {SubmissionId} was not found for as a referance to upload submissionFile", submissionId);

            throw new BadRequestException("Submission not exists");
        }
        string extension = Path.GetExtension(originalFileName);
        string storedFileName = $"{Guid.NewGuid():N}{extension}";
        string filePath = Path.Combine(_rootPath, storedFileName);
        try
        {
            using FileStream output = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write);
            await fileStream.CopyToAsync(output);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving file {OriginalFileName}", originalFileName);
            throw;
        }

        string checksum;
        using (var sha256 = SHA256.Create())
        {
            var hash = await sha256.ComputeHashAsync(fileStream);
            checksum = BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
        }

        SubmissionFile submissionFile = new SubmissionFile
        {
            SubmissionId = submissionId,
            OriginalFileName = originalFileName,
            StorageFileName = storedFileName,
            ContentType = contentType,
            Size = fileStream.Length,
            Checksum = checksum,
            UploadedByUserId = "1"
        };

        _context.SubmissionFiles.Add(submissionFile);
        await _context.SaveChangesAsync();
        return storedFileName; 
    }

    public async Task<Stream> downloadFileAsync(int id)
    {
        SubmissionFile submissionFile = await FetchSubmissionFileByIdInternalAsync(id);

        string filePath = Path.Combine(_rootPath, Path.GetFileName(submissionFile.StorageFileName));
        if (!File.Exists(filePath))
            throw new FileNotFound();

        return await Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    public async Task deleteFileAsync(int id)
    {
        SubmissionFile submissionFile = await FetchSubmissionFileByIdInternalAsync(id);

        string filePath = Path.Combine(_rootPath, Path.GetFileName(submissionFile.StorageFileName));
        if (!File.Exists(filePath))
            throw new FileNotFoundException("SubmissionFile Not Found for given id");

        _context.SubmissionFiles.Remove(submissionFile);
        await _context.SaveChangesAsync();
        File.Delete(filePath);

        return;
    }
}

