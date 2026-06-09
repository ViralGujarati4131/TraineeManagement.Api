using System.Collections;
using Microsoft.EntityFrameworkCore;
using TraineeManagementApi.DTOs;
using TraineeManagementApi.Models;
using TraineeManagementApi.Service.Interface;

namespace TraineeManagementApi.Service;

public class TraineeService : ITraineeService
{

    private readonly AppDbContext context;
    public TraineeService(AppDbContext c)
    {
        context = c;
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

    public async Task<IEnumerable<TraineeResponseDto>> GetTraineeAsync()
    {
        IEnumerable<Trainee> getTrainees = await context.Trainees.ToListAsync();
        IEnumerable<TraineeResponseDto> res = getTrainees.Select(u => MakeResponse(u)).ToList();
        return res;
    }
    public async Task<TraineeResponseDto?> GetTraineeByIdAsync(int id)
    {
        Trainee? traineeGet = await context.Trainees.FindAsync(id);
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
            Status = newTrainee.Status,
            CreatedDate = DateTime.UtcNow,
            UpdatedDate = DateTime.UtcNow
        };
        context.Trainees.Add(createTrainee);
        await context.SaveChangesAsync();
        TraineeResponseDto res = MakeResponse(createTrainee);
        return res;
    }
    public async Task<TraineeResponseDto?> UpdateTraineeAsync(int id, UpdateTraineeDto updateTrainee)
    {
        Trainee? trainee = await context.Trainees.FindAsync(id);
        if (trainee == null) return null;
        trainee.FirstName = updateTrainee.FirstName;
        trainee.LastName = updateTrainee.LastName;
        trainee.Email = updateTrainee.Email;
        trainee.TechStack = updateTrainee.TechStack;
        trainee.Status = updateTrainee.Status;
        trainee.UpdatedDate = DateTime.UtcNow;
        await context.SaveChangesAsync();
        TraineeResponseDto res = MakeResponse(trainee);
        return res;
    }
    public async Task<bool> DeleteTraineeByIdAsync(int id)
    {
        Trainee? traineeDelete = await context.Trainees.FindAsync(id);
        if (traineeDelete == null)
        {
            return false;
        }
        context.Trainees.Remove(traineeDelete);
        await context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTrainee)
    {
        IEnumerable<Trainee> res = await context.Trainees.Where
                    (t => (t.FirstName).Contains(searchTrainee) ||
                    (t.LastName).Contains(searchTrainee) ||
                    (t.Email).Contains(searchTrainee) ||
                    (t.TechStack).Contains(searchTrainee))
        .ToListAsync();
        IEnumerable<TraineeResponseDto> t = res.Select(u => MakeResponse(u)).ToList();
        return t;
    }
}

