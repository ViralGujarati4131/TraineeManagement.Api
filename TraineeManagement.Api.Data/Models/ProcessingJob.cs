using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagement.Api.Data.CustomDataAnnotation;

namespace TraineeManagement.Api.Data.ProcessingJobModel;

public class ProcessingJob
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    } 

    [RequiredField]
    public Guid MessageId 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public Guid CorrelationId 
    { 
        get; 
        set; 
    }
    
    [RequiredField]
    public int SubmissionId 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public int SubmissionFileId 
    { 
        get; 
        set; 
    }

    [ValidEnum(typeof(ProcessingJobStatus))]
    [RequiredField]
    public ProcessingJobStatus Status 
    { 
        get; 
        set; 
    } 

    [RequiredField]
    public int Attempts 
    { 
        get; 
        set; 
    }
    public string? ErrorSummary 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public DateTime RequestedAt 
    { 
        get; 
        set; 
    }

    public DateTime? StartedAt 
    { 
        get; 
        set; 
    }
    public DateTime? CompletedAt 
    { 
        get; 
        set; 
    }
}

public enum ProcessingJobStatus
{
    Queued,

    Processing,
    
    Completed,
    Failed
}