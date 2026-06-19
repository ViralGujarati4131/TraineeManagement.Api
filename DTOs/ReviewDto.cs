using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Reviews.Models;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Reviews.DTOs;

public record ReviewCreateDto
(
    int SubmissionId,
    
    int MentorId,
    
    [RequiredField]
    string Feedback,
    
    int score,
    
    [EnumDataType(typeof(ReviewStatus))]
    [RequiredField]
    ReviewStatus? ReviewStatus,
    
    [RequiredField]
    DateOnly ReviewedDate
);

public record ReviewResponseDto
(
    int Id,
    
    int SubmissionId,
    
    int MentorId,
    
    string Feedback,
    
    int score,
    
    ReviewStatus? ReviewStatus,
    
    DateOnly ReviewedDate
);