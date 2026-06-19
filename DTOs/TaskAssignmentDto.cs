using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.TaskAssignments.Models;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.TaskAssignments.DTOs;

public record TaskAssignmentCreateDto
(
    int TraineeId,

    int MentorId,

    int LearningTaskId,

    [RequiredField]
    DateOnly AssignedDate,

    [RequiredField]
    [ValidDateRange("AssignedDate")]
    DateOnly DueDate,

    [EnumDataType(typeof(TaskAssignmentStatus))]
    [RequiredField]
    TaskAssignmentStatus? Status,

    string Remarks
);

public record TaskAssignmentResponseDto
(
    int Id,

    int TraineeId,

    int MentorId,

    int LearningTaskId,

    DateOnly AssignedDate,

    DateOnly DueDate,

    TaskAssignmentStatus? Status,

    string Remarks
);

public record TaskAssignmentUpdateDto
(
    [EnumDataType(typeof(TaskAssignmentStatus))]
    [RequiredField]
    TaskAssignmentStatus? Status
);
