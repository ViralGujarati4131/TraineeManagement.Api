using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Users.Models;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Users.DTOs;

public record UserResponseDto
(
    int Id,

    string Username,

    UserRole? Role
);

public record LoginTokenResponseDto
(
    string Token,

    int ExpiresIn,
    
    UserResponseDto? User
);

public record UserLoginDto
(
    [RequiredField]
    string Username,

    [RequiredField]
    string Password
);