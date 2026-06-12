using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Models;

namespace TraineeManagementApi.DTOs;

public class UpdateTraineeDto
{
    [Required(ErrorMessage = "FirstName is required")]
    [MaxLength(50, ErrorMessage = "FirstName can not be exceed 50 character")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "LastName is required")]
    [MaxLength(50, ErrorMessage = "LastName can not be exceed 50 character")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "TechStack is required")]
    public string TechStack { get; set; } = string.Empty;

    [Required(ErrorMessage = "Invalid Status")]
    [AllowedValues(TraineeStatus.Active, TraineeStatus.Inactive, TraineeStatus.Completed)]
    public TraineeStatus Status { get; set; }
}

public class TraineeResponseDto
{
    public int Id { get; set; }

    public string FirstName { get; set; } = string.Empty;

    public string LastName { get; set; } = string.Empty;
}

public class CreateTraineeDto
{
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
}

public class PaginationSearchDto
{
    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalRecords { get; set; }

    public IEnumerable<TraineeResponseDto>? Data { get; set; }
}