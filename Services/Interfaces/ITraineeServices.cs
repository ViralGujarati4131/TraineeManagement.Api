using TraineeManagementApi.DTOs;

namespace TraineeManagementApi.Service.Interface;

public interface ITraineeService
{
    Task<IEnumerable<TraineeResponseDto>> GetTraineeAsync();

    Task<TraineeResponseDto?> GetTraineeByIdAsync(int id);

    Task<TraineeResponseDto> CreateTraineeAsync(CreateTraineeDto createTrainee);

    Task<TraineeResponseDto?> UpdateTraineeAsync(int id, UpdateTraineeDto updateTrainee);

    Task<bool> DeleteTraineeByIdAsync(int id);

    Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTrainee);

    Task<PaginationSearchDto?> PaginationSearchTraineeAsync(int pageNumber, int pageSize, string name, string status);
}