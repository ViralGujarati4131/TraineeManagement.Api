using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Models.TimestampInterface;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Reviews.Models;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Mentors.Models;

public class Mentor : ICreateTimestamp,IUpdateTimestamp
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public string FirstName 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public string LastName 
    { 
        get; 
        set; 
    } = string.Empty;

    [EmailAddress]
    public string Email
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public string Expertise 
    { 
        get; 
        set; 
    } = string.Empty;

    [ValidEnum(typeof(MentorStatus))]
    [RequiredField]
    public MentorStatus? Status 
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

    public ICollection<Review> Reviews 
    { 
        get; 
        set; 
    } = new List<Review>();
}
public enum MentorStatus
{
    Active,
    
    Inactive
}