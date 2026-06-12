using TraineeManagementApi.DTOs;

namespace TraineeManagementApi.Service.Interface;

public interface ITraineeService
{
    Task<IEnumerable<TraineeResponseDto>> GetTraineesAsync();

    Task<TraineeResponseDto?> GetTraineeByIdAsync(int id);

    Task<TraineeResponseDto> CreateTraineeAsync(CreateTraineeDto createTraineeDto);

    Task<TraineeResponseDto?> UpdateTraineeAsync(int id, UpdateTraineeDto updateTraineeDto);

    Task<bool> DeleteTraineeByIdAsync(int id);

    Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTerm);

    Task<PaginationSearchDto?> GetPagedAndSearchedTraineesAsync(int pageNumber, int pageSize, string name, string status);
}