using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Models.TimestampInterface;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Trainees.Models;

public class Trainee : ITimestamp
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; set; 
    }

    [RequiredField]
    [FeildMaxLength(50)]
    public string FirstName 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    [FeildMaxLength(50)]
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
    public string TechStack 
    { 
        get; 
        set; 
    } = string.Empty;

    [EnumDataType(typeof(TraineeStatus))]
    [RequiredField]
    public TraineeStatus? Status 
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

public enum TraineeStatus
{
    Active,

    Inactive,
    
    Completed
}