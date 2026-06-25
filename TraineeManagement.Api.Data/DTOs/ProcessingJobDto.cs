using System.Data;
using TraineeManagement.Api.Data.ProcessingJobModel;

namespace TraineeManagement.Api.Data.ProcessingJobDto;

public record ProcessingJobResponseDto
( 
    int Id,

    Guid MessageId,

    Guid CorrelationId, 
    
    int SubmissionId,

    ProcessingJobStatus Status,
    
    int Attempts,
    
    string? ErrorSummary,

    DateTime RequestedAt,
    
    DateTime? StartedAt,

    DateTime? CompletedAt
);