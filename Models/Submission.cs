using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Reviews.Models;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Submissions.Models;

public class Submission
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    }

    public int TaskAssignmentId 
    { 
        get; 
        set; 
    }
    public TaskAssignment? TaskAssignment 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public string SubmissionUrl 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public string Notes 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public DateOnly SubmittedDate 
    { 
        get; 
        set; 
    }

    [EnumDataType(typeof(SubmissionStatus))]
    [RequiredField]
    public SubmissionStatus? Status 
    { 
        get; 
        set; 
    }

    public ICollection<Review> Reviews 
    { 
        get; 
        set; 
    } = new List<Review>();
}

public enum SubmissionStatus
{
    Submitted,
    Resubmitted
}