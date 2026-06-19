using TraineeManagementApi.Models.TimestampInterface;
using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.TaskAssignments.Models;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.LearningTasks.Models;

public class LearningTask : ICreateTimestamp,IUpdateTimestamp
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public string Title 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public string Description 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public string ExpectedTechStack 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public DateOnly DueDate 
    { 
        get; 
        set; 
    }

    [ValidEnum(typeof(LearningTaskStatus))]
    [RequiredField]
    public LearningTaskStatus? Status 
    { 
        get; 
        set; 
    }

    public DateTime CreatedDate 
    { 
        get; 
        set; 
    }

    public DateTime UpdatedDate 
    { 
        get; 
        set; 
    }

    public ICollection<TaskAssignment> TaskAssignments 
    { 
        get; 
        set; 
    } = new List<TaskAssignment>();
}
public enum LearningTaskStatus
{
    Draft,

    Published,

    Closed
}