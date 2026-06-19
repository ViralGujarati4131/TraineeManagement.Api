using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Submissions.Models;
using TraineeManagementApi.Mentors.Models;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Reviews.Models;

public class Review
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    }

    public int SubmissionId
    { 
        get; 
        set; 
    }
    public Submission? Submission 
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

    [RequiredField]
    public string Feedback 
    { 
        get; 
        set; 
    } = string.Empty;

    public int Score 
    { 
        get; 
        set; 
    }

    [ValidEnum(typeof(ReviewStatus))]
    [RequiredField]
    public ReviewStatus? ReviewStatus 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public DateOnly ReviewedDate 
    { 
        get; 
        set; 
    }
}

public enum ReviewStatus
{
    Accepted,

    ChangesRequired,
    
    Rejected
}