using System.ComponentModel.DataAnnotations;

namespace TraineeManagementApi.Models;

public class Trainee
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(50, ErrorMessage = "FirstName can not be exceed 50 characters")]
    public string FirstName { get; set; }

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(50, ErrorMessage = "LastName can not be exceed 50 characters")]
    public string LastName { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }

    [Required(ErrorMessage = "TechStack is required")]
    public string TechStack { get; set; }

    [Required(ErrorMessage = "Invalid Status")]
    [AllowedValues(TraineeStatus.Active, TraineeStatus.Inactive, TraineeStatus.Completed)]
    public TraineeStatus Status { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
}

public enum TraineeStatus
{
    Active, Inactive, Completed
}