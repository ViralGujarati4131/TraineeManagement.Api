using TraineeManagement.Api.Data.ProcessingJobDto;

namespace TraineeManagement.Api.ProcessingJobServiceInterface;

public interface IProcessingJobService
{
    Task<ProcessingJobResponseDto> GetProcessingJobByIdAsync(int id);
}