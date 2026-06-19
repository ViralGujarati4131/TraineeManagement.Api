using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Models.TimestampInterface;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Users.Models;

public class User : ITimestamp
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public string Username 
    { 
        get; 
        set; 
    } = string.Empty;

    [RequiredField]
    public string PasswordHash 
    { 
        get; 
        set; 
    } = string.Empty;

    [EnumDataType(typeof(UserRole))]
    [RequiredField]
    public UserRole? Role 
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
}

public enum UserRole
{
    Admin,

    Mentor,
    
    Trainee
}