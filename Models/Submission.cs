using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Reviews.Models;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Utils.CustomValidation;
using TraineeManagementApi.SubmissionFiles.Models;

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

    [ValidEnum(typeof(SubmissionStatus))]
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

    public ICollection<SubmissionFile> SubmissionFiles 
    { 
        get; 
        set; 
    } = new List<SubmissionFile>();
}

public enum SubmissionStatus
{
    Submitted,
    Resubmitted
}