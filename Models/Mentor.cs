using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Models.TimestampInterface;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Reviews.Models;

namespace TraineeManagementApi.Mentors.Models;

public class Mentor : ITimestamp
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "LastName is required")]
    public string LastName { get; set; } = string.Empty;

    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Expertise is required")]
    public string Expertise { get; set; } = string.Empty;

    [EnumDataType(typeof(MentorStatus), ErrorMessage = "Invalid Status")]
    public MentorStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }

    public ICollection<TaskAssignment> TaskAssignments { get; set; } = new List<TaskAssignment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
public enum MentorStatus
{
    Active,
    Inactive
}