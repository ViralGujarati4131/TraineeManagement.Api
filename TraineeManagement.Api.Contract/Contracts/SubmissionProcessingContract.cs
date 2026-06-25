namespace TraineeManagement.Api.Contract.SubmissionProcessingContarct;

public record SubmissionProcessingContract
(
    int ProcessingJobId,
    
    Guid MessageId,

    Guid CorrelationId,

    int TaskAssignmentId,

    int SubmissionFileId,
    
    DateTimeOffset RequestedAt, 
  
    string ContractVersion 
);
    
public record SubmissionPublishResult
(
    int ProcessingJobId,
    
    Guid MessageId,

    Guid CorrelationId,

    bool Success
);

