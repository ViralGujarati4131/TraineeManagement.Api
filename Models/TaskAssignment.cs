using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Trainees.Models;
using TraineeManagementApi.Mentors.Models;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.Submissions.Models;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Constants;

namespace TraineeManagementApi.TaskAssignments.Models;

public class TaskAssignment : IValidatableObject
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    }

    public int TraineeId 
    { 
        get; 
        set; 
    }
    public Trainee? Trainee 
    { 
        get; 
        set; 
    }

    public int MentorId 
    { 
        get; 
        set; 
    }
    public Mentor? Mentor 
    { 
        get; 
        set; 
    }

    public int LearningTaskId 
    { 
        get; 
        set; 
    }
    public LearningTask? LearningTask 
    { 
        get; 
        set; 
    }

    [Required]
    public DateOnly AssignedDate 
    { 
        get; 
        set; 
    }

    [Required]
    public DateOnly DueDate 
    { 
        get; 
        set; 
    }

    [EnumDataType(typeof(TaskAssignmentStatus))]
    [Required]
    public TaskAssignmentStatus? Status 
    { 
        get; 
        set; 
    }

    public string Remarks 
    { 
        get; 
        set; 
    } = string.Empty;

    public ICollection<Submission> Submissions 
    { 
        get; 
        set; 
    } = new List<Submission>();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        System.Console.WriteLine("HIiii");
        if (DueDate < AssignedDate)
        {
            yield return new ValidationResult
            (
                "DueDate can not be before the assigned date",
                new[] { nameof(DueDate) }   
            );
        }
    }
}

public enum TaskAssignmentStatus
{
    Assigned,

    Inprogress,

    Submitted,

    Reviewed,
    
    Completed
}