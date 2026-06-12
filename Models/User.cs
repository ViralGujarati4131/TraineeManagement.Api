using System.ComponentModel.DataAnnotations;
using MyApi.Models;

namespace Users.Models;

public class User : ITimestamp
{
    [Key]
    public int Id { get; set; }

    [Required(ErrorMessage = "Username can not be empty")]
    public string Username { get; set; } = string.Empty;

    [Required(ErrorMessage = "Password can not be empty")]
    public string PasswordHash { get; set; } = string.Empty;

    [AllowedValues(UserRole.Admin, UserRole.Mentor, UserRole.Trainee)]
    public UserRole Role { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime UpdatedDate { get; set; }
}

public enum UserRole
{
    Admin,
    Mentor,
    Trainee
}