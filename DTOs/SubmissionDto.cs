using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Submissions.Models;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.Submissions.DTOs;

public record SubmissionCreateDto
(
    int TaskAssignmentId,

    [RequiredField]
    string SubmissionUrl,

    [RequiredField]
    string Notes,

    [RequiredField]
    DateOnly SubmittedDate,

    [ValidEnum(typeof(SubmissionStatus))]
    [RequiredField]
    SubmissionStatus? Status
);

public record SubmissionResponseDto
(
    int Id,

    int TaskAssignmentId,

    string SubmissionUrl,
    
    string Notes,
    
    DateOnly SubmittedDate,
    
    SubmissionStatus? Status
);