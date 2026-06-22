using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TraineeManagementApi.FileStorage.Configurations;
using TraineeManagementApi.FileStorage.ServiceInterface;
using TraineeManagementApi.SubmissionFiles.Models;
using TraineeManagementApi.Utils.CustomException;

namespace TraineeManagementApi.FileStorage.Service;

public class FileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    
    private readonly ILogger<FileStorageService> _logger;

    private readonly FileStorageConfiguration _fileConfiguration;

    private readonly AppDbContext _context;

    public FileStorageService(IWebHostEnvironment env, ILogger<FileStorageService> logger, IOptions<FileStorageConfiguration> fileConfiguration,AppDbContext context)
    {
        _logger = logger;
        _fileConfiguration = fileConfiguration.Value;
        _context = context;

        if (string.IsNullOrWhiteSpace(_fileConfiguration.RootPath))
        {
            throw new FileStorageConfigurationException();
        }
        
        string configuredPath = _fileConfiguration.RootPath; 
        string basePath = env.ContentRootPath;

        _rootPath = Path.Combine(basePath, configuredPath);

        if (!Directory.Exists(_rootPath))
        {
            Directory.CreateDirectory(_rootPath);
        }
    }

    public async Task<string?> SaveAsync(int submissionId,IFormFile file)
    {
        string diskSafeFileName = string.Empty;

        using SHA256 sha256 = SHA256.Create();
        using Stream read = file.OpenReadStream();
        byte[] hashBytes = await sha256.ComputeHashAsync(read);
        string checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();  
        SubmissionFile? alreadySubmittedFile = await _context.SubmissionFiles
            .AsNoTracking()
            .Where(s => 
                s.SubmissionId == submissionId &&
                s.Checksum.Contains(checksum))               
            .FirstOrDefaultAsync();
        if(alreadySubmittedFile != null)
        {
            return null;
        }

        if (file == null || file.Length == 0)
            throw new BadRequestException("Empty file uploads are not allowed.");

        if (file.Length > _fileConfiguration.MaxFileSizeInBytes)
            throw new BadRequestException($"File size exceeds the allowed {_fileConfiguration.MaxFileSizeInBytes / (1024 * 1024)} MB limit.");

        string ext = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!_fileConfiguration.AllowedExtensions.Contains(ext))
        {
            _logger.LogWarning("Unauthorized file extension block triggered for: {Extension}", ext);
            throw new BadRequestException($"File type extension '{ext}' is not authorized.");
        }

        if (_fileConfiguration.MagicNumbers.TryGetValue(ext, out string? hexSignature) && !string.IsNullOrWhiteSpace(hexSignature))
        {
            byte[] expectedSignature = Convert.FromHexString(hexSignature);
            byte[] actualHeader = new byte[expectedSignature.Length];

            using (Stream stream = file.OpenReadStream())
            {
                await stream.ReadExactlyAsync(actualHeader, 0, expectedSignature.Length);
            }

            if (!actualHeader.SequenceEqual(expectedSignature))
            {
                _logger.LogWarning("Security: File contents for {FileName} do not match hex signature {Hex}!", file.FileName, hexSignature);
                throw new BadRequestException("The file contents do not match its true file type extension signature safely.");
            }
        }

        using Stream fileStream = file.OpenReadStream();
        string originalFileName = file.FileName;

        string uniqueId = Guid.NewGuid().ToString("N");
        string datePrefix = DateTime.UtcNow.ToString("yyyy/MM/dd");
        
        string storedFileName = $"{datePrefix}/{uniqueId}{ext}"; 

        diskSafeFileName = storedFileName.Replace("/", "_");
        
        string filePath = Path.Combine(_rootPath, diskSafeFileName);
        
        try
        {
            using FileStream output = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            
            if (fileStream.CanSeek)
            {
                fileStream.Position = 0;
            }
            
            await fileStream.CopyToAsync(output);
            
            return diskSafeFileName; 
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Physical disk IO failure saving file {OriginalFileName}", originalFileName);
            throw;
        }
    }

    public Task<Stream> OpenReadAsync(string storageFileName)
    {
        string filePath = Path.Combine(_rootPath, storageFileName);

        if (!File.Exists(filePath))
        {
            throw new ExceptionFileNotFound();
        }

        return Task.FromResult<Stream>(new FileStream(filePath, FileMode.Open, FileAccess.Read));
    }

    public Task<bool> ExistsAsync(string storageFileName)
    {
        string filePath = Path.Combine(_rootPath, storageFileName);
        return Task.FromResult(File.Exists(filePath));
    }

    public Task DeleteAsync(string storageFileName)
    {
        string filePath = Path.Combine(_rootPath, storageFileName);

        if (File.Exists(filePath))
        {
            File.Delete(filePath);
            _logger.LogInformation("Physical storage asset successfully deleted: {FileName}", storageFileName);
        }

        return Task.CompletedTask;
    }
}