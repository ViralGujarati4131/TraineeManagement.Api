    using System.Security.Cryptography;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.Extensions.Options;
    using TraineeManagement.Api.FileStoreValidation;
    using TraineeManagement.Api.FileStorageServiceInterface;
    using TraineeManagement.Api.Data.SubmissionFileModel;
    using TraineeManagement.Api.Data.SubmissionModel;
    using TraineeManagement.Api.Data.CustomException;
    using TraineeManagement.Api.Data.DatabaseContext;

    namespace TraineeManagement.Api.FileStorageService;

    public class FileStorageService : IFileStorageService
    {
        private readonly string _rootPath;
        
        private readonly ILogger<FileStorageService> _logger;

        private readonly CustomFileStoreValidation _fileConfiguration;

        private readonly AppDbContext _context;

        public FileStorageService(ILogger<FileStorageService> logger, IOptions<CustomFileStoreValidation> fileConfiguration,AppDbContext context)
        {
            _logger = logger;
            _fileConfiguration = fileConfiguration.Value;
            _context = context;

            if (string.IsNullOrWhiteSpace(_fileConfiguration.RootPath))
            {
                throw new FileStorageConfigurationException();
            }
            
            string configuredPath = _fileConfiguration.RootPath; 
            string basePath = _fileConfiguration.BasePath;

            _rootPath = Path.Combine(basePath, configuredPath);

            if (!Directory.Exists(_rootPath))
            {
                Directory.CreateDirectory(_rootPath);
            }
        }

        public async Task<string> SaveAsync(int submissionId,IFormFile file)
        {
            Submission? submission = await _context.Submissions.FindAsync(submissionId);
            if (submission == null)
            {
                throw new BadRequestException("The referenced submission profile does not exist.");
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

            string uniqueId = Guid.NewGuid().ToString("N");
            string datePrefix = DateTime.UtcNow.ToString("yyyy/MM/dd");
            string storedFileName = $"{datePrefix}/{uniqueId}{ext}"; 
            string diskSafeFileName = storedFileName.Replace("/", "_");
            string filePath = Path.Combine(_rootPath, diskSafeFileName);
            try
            {
                using FileStream output = new FileStream(filePath, FileMode.Create, FileAccess.Write);
                using Stream fileStream = file.OpenReadStream();
                await fileStream.CopyToAsync(output);
                return diskSafeFileName;  
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Physical disk IO failure saving file {OriginalFileName}", file.FileName);
                throw;
            }
        }

        public async Task<(Stream FileStream, string ContentType, string OriginalFileName)> OpenAsync(int id)
        {
            SubmissionFile? metadata = await _context.SubmissionFiles.FindAsync(id);
            if (metadata == null)
            {
                throw new ExceptionFileNotFound();
            }

            string filePath = Path.Combine(_rootPath, metadata.StorageFileName);
            if (!File.Exists(filePath))
            {
                throw new ExceptionFileNotFound();
            }

            FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, FileOptions.Asynchronous);
        
            return (fileStream, metadata.ContentType, metadata.OriginalFileName);
        }

        public async Task<bool> Exists(int submissionId, IFormFile file)
        {
            using SHA256 sha256 = SHA256.Create();
            using Stream read = file.OpenReadStream();
            byte[] hashBytes = await sha256.ComputeHashAsync(read);
            string checksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();  
            bool isDuplicate = await _context.SubmissionFiles
                .AsNoTracking()
                .AnyAsync(s => 
                    s.SubmissionId == submissionId &&
                    s.Checksum == checksum);               
            return isDuplicate;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            SubmissionFile? metadata = await _context.SubmissionFiles.FindAsync(id);
            if (metadata == null)
            {
                throw new ExceptionFileNotFound();
            }
            string filePath = Path.Combine(_rootPath, metadata.StorageFileName);

            if (File.Exists(filePath))
            {
                try
                {
                    File.Delete(filePath);
                    _logger.LogInformation("Physical storage file successfully deleted: {FileName}", metadata.StorageFileName);
                }
                catch (IOException ex)
                {
                    _logger.LogError(ex, "Failed to delete physical file {FileName} from disk storage.", metadata.StorageFileName);
                    return false;
                }
            }
             _context.SubmissionFiles.Remove(metadata);
            await _context.SaveChangesAsync();
            return true;
        }
    }