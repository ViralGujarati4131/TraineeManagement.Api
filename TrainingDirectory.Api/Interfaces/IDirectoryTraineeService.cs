using TraineeManagement.Api.Data.TraineeDTO;

namespace TrainingDirectory.Api.DirectoryTraineeServiceInterface;

public interface IDirectoryTraineeService
{
    public Task<TraineeResponseDto> GetTraineeByIdAsync(int id, CancellationToken cancellationToken);
}