using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.DTOs;
using TraineeManagementApi.Models;
using TraineeManagementApi.Service.Interface;

namespace TraineeManagementApi.Service;

public class TraineeService : ITraineeService
{

    private readonly AppDbContext context;
    private readonly ILogger<TraineeService> _logger;
    public TraineeService(AppDbContext c,ILogger<TraineeService> logger)
    {
        context = c;
        _logger = logger;
    }

    private TraineeResponseDto MakeResponse(Trainee responseTrainee)
    {
        return new TraineeResponseDto
        {
            Id = responseTrainee.Id,
            FirstName = responseTrainee.FirstName,
            LastName = responseTrainee.LastName,
        };
    }

    private async Task<Trainee?> FetchTrainee(int id)
    {
      Trainee? findTrainee = await context.Trainees.FindAsync(id);
        if (findTrainee == null)
        {
            _logger.LogInformation("User not found");
            return null;
        }
        return findTrainee;
    }

    public async Task<IEnumerable<TraineeResponseDto>> GetTraineeAsync()
    {
         _logger.LogDebug("Fetching all trainees");
        IEnumerable<Trainee> getTrainees = await context.Trainees.ToListAsync();
        IEnumerable<TraineeResponseDto> res = getTrainees.Select(u => MakeResponse(u));
        return res;
    }
    public async Task<TraineeResponseDto?> GetTraineeByIdAsync(int id)
    {
        _logger.LogDebug($"Finding the trainee whose id is {id}");
        Trainee? traineeGet = await FetchTrainee(id);
        if (traineeGet == null) return null;
        TraineeResponseDto res = MakeResponse(traineeGet);
        return res;
    }

    public async Task<TraineeResponseDto> CreateTraineeAsync(CreateTraineeDto newTrainee)
    {
        Trainee createTrainee = new Trainee
        {
            FirstName = newTrainee.FirstName,
            LastName = newTrainee.LastName,
            Email = newTrainee.Email,
            TechStack = newTrainee.TechStack,
            Status = newTrainee.Status
        };
        context.Trainees.Add(createTrainee);
        await context.SaveChangesAsync();
        _logger.LogInformation($"Trainee {newTrainee.FirstName} created");
        TraineeResponseDto res = MakeResponse(createTrainee);
        return res;
    }
    public async Task<TraineeResponseDto?> UpdateTraineeAsync(int id, UpdateTraineeDto updateTrainee)
    {
        _logger.LogDebug("Finding the trainee by id for update");
        Trainee? trainee = await FetchTrainee(id);
        if (trainee == null) return null;
        trainee.FirstName = updateTrainee.FirstName;
        trainee.LastName = updateTrainee.LastName;
        trainee.Email = updateTrainee.Email;
        trainee.TechStack = updateTrainee.TechStack;
        trainee.Status = updateTrainee.Status;
        await context.SaveChangesAsync();
        _logger.LogInformation($"Trainee {updateTrainee.FirstName} updated");
        TraineeResponseDto res = MakeResponse(trainee);
        return res;
    }
    public async Task<bool> DeleteTraineeByIdAsync(int id)
    {
        _logger.LogDebug("Finding the trainee by id for delete");
        Trainee? traineeDelete = await FetchTrainee(id);
        if (traineeDelete == null)
        {
            return false;
        }
         _logger.LogInformation($"Trainee {traineeDelete.FirstName} deleted");
        context.Trainees.Remove(traineeDelete);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTrainee)
    {
        _logger.LogDebug($"Finding the trainee for matching {searchTrainee}");   
        IEnumerable<Trainee> res = await context.Trainees.Where
                    (t => (t.FirstName).Contains(searchTrainee) ||
                    (t.LastName).Contains(searchTrainee) ||
                    (t.Email).Contains(searchTrainee) ||
                    (t.TechStack).Contains(searchTrainee))
        .ToListAsync();
        IEnumerable<TraineeResponseDto> t = res.Select(u => MakeResponse(u)).ToList();
        return t;
    }

    public async Task<PaginationSearchDto?> PaginationSearchTraineeAsync(int pageNumber, int pageSize, string name, string status)
    {
        _logger.LogDebug($"Finding the trainee for matching name is {name} status is {status}");   
        IEnumerable<Trainee> Data = await context.Trainees.Where
            (t => (t.FirstName).Equals(name) &&
            (t.Status).ToString().Equals(status))
        .ToListAsync();


        int totalRecords = Data.Count();
        _logger.LogDebug($"Adding pagination wiht page size {pageSize}");
        IEnumerable<Trainee> getData = Data
            .OrderBy(p => p.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize);

        if (!getData.Any())
        {
            return null;
        }

        IEnumerable<TraineeResponseDto> responseData = getData.Select(t => MakeResponse(t)).ToList();

        PaginationSearchDto response = new PaginationSearchDto
        {
            pageNumber = pageNumber,
            pageSize = getData.Count(),
            totalRecords = totalRecords,
            data = responseData
        };

        return response;
    }
}

