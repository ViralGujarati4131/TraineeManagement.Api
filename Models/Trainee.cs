using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Models.TimestampInterface;
using TraineeManagementApi.TaskAssignments.Models;

namespace TraineeManagementApi.Trainees.Models;

public class Trainee : ITimestamp
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(50, ErrorMessage = "FirstName can not be exceed 50 characters")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(50, ErrorMessage = "LastName can not be exceed 50 characters")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "TechStack is required")]
    public string TechStack { get; set; } = string.Empty;

    [EnumDataType(typeof(TraineeStatus), ErrorMessage = "Invalid Status")]
    public TraineeStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
}

public enum TraineeStatus
{
    Active,
    Inactive,
    Completed
}