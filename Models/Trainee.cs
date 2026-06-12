using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace TraineeManagementApi.Models;

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

    [Required(ErrorMessage = "Invalid Status")]
    [AllowedValues(TraineeStatus.Active, TraineeStatus.Inactive, TraineeStatus.Completed)]
    public TraineeStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
}

public enum TraineeStatus
{
    Active,
    Inactive,
    Completed
}