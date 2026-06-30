using Microsoft.EntityFrameworkCore;
using TraineeManagement.Api.Data.CacheServiceInterface;
using TraineeManagement.Api.Data.TraineeDTO;
using TraineeManagement.Api.Data.TraineeModel;
using TraineeManagement.Api.TraineeServiceInterface;
using TraineeManagement.Api.Data.CustomException;
using TraineeManagement.Api.Data.CacheKey;
using TraineeManagement.Api.Data.DatabaseContext;
using System.Net;
using TraineeManagement.Api.Data.Response;
using TraineeManagement.Api.Data.ResponseDTO;
using System.Text.Json;
using TraineeManagement.Api.CorrelationId;

namespace TraineeManagement.Api.TraineeService;

public class TraineeServices : ITraineeService
{
    private readonly AppDbContext _context;
    private readonly ILogger<TraineeServices> _logger;
    private readonly ICacheService _cacheService;

    private readonly HttpClient _httpClient;

    private readonly ICorrelationIdAccessor _correlationIdAccessor;

    public TraineeServices(AppDbContext context, ILogger<TraineeServices> logger, ICacheService cacheService,HttpClient httpClient, ICorrelationIdAccessor correlationIdAccessor)
    {
        _context = context;
        _logger = logger;
        _cacheService = cacheService;
        _httpClient = httpClient;
        _correlationIdAccessor = correlationIdAccessor;
    }

    private TraineeResponseDto MapToResponseDto(Trainee trainee)
    {
        return new TraineeResponseDto(trainee.Id, trainee.FirstName, trainee.LastName);
    }

    private async Task<Trainee> FetchTraineeByIdInternalAsync(int id)
    {
        Trainee? trainee = await _context.Trainees.FindAsync(id);
        if (trainee == null)
        {
            _logger.LogWarning("Dependency failure: Record missing. Id: {TraineeId}", id);
            throw new NotFoundException(CustomResponse.NotFound,"Trainee");
        }
        return trainee;
    }

    public async Task<IEnumerable<TraineeResponseDto>> GetTraineesAsync()
    {
        _logger.LogDebug("Fetching all trainees from the database");

        List<TraineeResponseDto> trainees = await _context.Trainees
            .AsNoTracking()
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .ToListAsync();

        _logger.LogInformation("State check: Bulk fetch trainees success.");
        return trainees;
    }

    public async Task<TraineeResponseDto?> GetTraineeByIdAsync(int id, CancellationToken cancellationToken)
    {
        string cacheKey = CacheKey.Trainee(id);

        TraineeResponseDto? cached = await _cacheService.GetAsync<TraineeResponseDto>(cacheKey);
        if (cached is not null)
        {
            _logger.LogDebug("Cache hit");
            return cached;
        }

        _logger.LogDebug("Cache miss.");
        _logger.LogInformation("Executing HTTP GET. TraineeId: {TraineeId}", id);

        try
        {
            HttpResponseMessage response = await _httpClient.GetAsync($"/api/directory/trainee/{id}", cancellationToken);

            if (response.IsSuccessStatusCode)
            {
                InterServiceCommunicationResponse<TraineeResponseDto>? responseData = await response.Content.ReadFromJsonAsync<InterServiceCommunicationResponse<TraineeResponseDto>>(
                    new JsonSerializerOptions(JsonSerializerDefaults.Web),
                    cancellationToken
                );

                if (responseData is null || responseData.Data is null)
                {
                    _logger.LogWarning("Dependency failure: Empty body payload. TraineeId: {TraineeId}",id);
                    throw new NotFoundException(CustomResponse.NotFound);
                }

                TraineeResponseDto trainee = responseData.Data;
                await _cacheService.SetAsync(cacheKey, trainee, TimeSpan.FromMinutes(CacheTime.TTL));

                _logger.LogInformation("HTTP GET success. TraineeId: {TraineeId}", id);
                return trainee;
            }
            else if (response.StatusCode == HttpStatusCode.NotFound)
            {
                _logger.LogWarning("Dependency failure: Remote resource missing. TraineeId: {TraineeId}", id);
                throw new NotFoundException(CustomResponse.NotFound, "Trainee");
            }
        }
        catch
        {
            throw;
        }

        return null;
    }

    public async Task<TraineeResponseDto> CreateTraineeAsync(TraineeCreateDto createTraineeDto)
    {
        Trainee trainee = new Trainee
        {
            FirstName = createTraineeDto.FirstName,
            LastName = createTraineeDto.LastName,
            Email = createTraineeDto.Email,
            TechStack = createTraineeDto.TechStack,
            Status = createTraineeDto.Status
        };

        _context.Trainees.Add(trainee);
        await _context.SaveChangesAsync();
        _logger.LogInformation("State transition: Created trainee record. Id: {TraineeId}", trainee.Id);    

        return MapToResponseDto(trainee);
    }

    public async Task<TraineeResponseDto> UpdateTraineeAsync(int id, TraineeUpdateDto updateTraineeDto)
    {
        _logger.LogDebug("Locating trainee with ID {TraineeId} for modifications", id);
        Trainee trainee = await FetchTraineeByIdInternalAsync(id);

        trainee.FirstName = updateTraineeDto.FirstName;
        trainee.LastName = updateTraineeDto.LastName;
        trainee.Email = updateTraineeDto.Email;
        trainee.TechStack = updateTraineeDto.TechStack;
        trainee.Status = updateTraineeDto.Status;
        
        try
        {
            await _context.SaveChangesAsync();
            _logger.LogInformation("State transition: Modified trainee record. Id: {TraineeId}", id);
        }
        finally
        {
            string cacheKey = CacheKey.Trainee(id);
            await _cacheService.RemoveAsync(cacheKey);
            _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);
        }
        return MapToResponseDto(trainee);
    }

    public async Task DeleteTraineeByIdAsync(int id)
    {
        _logger.LogDebug("Finding trainee with ID {TraineeId} for physical deletion", id);
        Trainee trainee = await FetchTraineeByIdInternalAsync(id);

        try
        {    
            _context.Trainees.Remove(trainee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("State transition: Deleted trainee record. Id: {TraineeId}", id);
        }
        finally
        {
            string cacheKey = CacheKey.Trainee(id);
            await _cacheService.RemoveAsync(cacheKey);
            _logger.LogInformation("Cache evict. CacheKey: {CacheKey}", cacheKey);
        }

    }

    public async Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTerm)
    {
        _logger.LogDebug("Executing text search match for: {SearchTerm}", searchTerm);

        List<TraineeResponseDto> results = await _context.Trainees
            .AsNoTracking()
            .Where(t =>
                t.FirstName.Contains(searchTerm) ||
                t.LastName.Contains(searchTerm) ||
                t.Email.Contains(searchTerm) ||
                t.TechStack.Contains(searchTerm)
            )
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .ToListAsync();

        _logger.LogInformation("State check: Trainee list search completed.");
        return results;
    }

    public async Task<TraineePaginationSearchDto> GetPagedAndSearchedTraineesAsync(int pageNumber, int pageSize, string name, string status)
    {
        _logger.LogDebug("Executing target filter parameters - Name: {FilterName}, Status: {FilterStatus}", name, status);

        IQueryable<Trainee> query = _context.Trainees.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(t => t.FirstName.Contains(name));

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse(status, true, out TraineeStatus parsedStatus))
            query = query.Where(t => t.Status == parsedStatus);

        int totalRecords = await query.CountAsync();

        _logger.LogDebug("Applying pagination - Page size: {PageSize}, Offset: {Offset}", pageSize, (pageNumber - 1) * pageSize);

        List<TraineeResponseDto> responseData = await query
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TraineeResponseDto(t.Id, t.FirstName, t.LastName))
            .ToListAsync();

        _logger.LogInformation("State check: Paged search calculation complete.");
        return new TraineePaginationSearchDto(pageNumber, responseData.Count, totalRecords, responseData);
    }
}