using TraineeManagement.Api.Data.TraineeDTO;

namespace TraineeManagement.Api.TraineeServiceInterface;

public interface ITraineeService
{
    Task<IEnumerable<TraineeResponseDto>> GetTraineesAsync();

    Task<TraineeResponseDto?> GetTraineeByIdAsync(int id,CancellationToken cancellationToken);

    Task<TraineeResponseDto> CreateTraineeAsync(TraineeCreateDto createTraineeDto);

    Task<TraineeResponseDto> UpdateTraineeAsync(int id, TraineeUpdateDto updateTraineeDto);

    Task DeleteTraineeByIdAsync(int id);

    Task<IEnumerable<TraineeResponseDto>> SearchTraineesAsync(string searchTerm);

    Task<TraineePaginationSearchDto> GetPagedAndSearchedTraineesAsync(int pageNumber, int pageSize, string name, string status);
}