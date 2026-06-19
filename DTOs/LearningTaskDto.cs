using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.LearningTasks.DTOs;

public record LearningTaskCreateDto
(
    [RequiredField]
    string Title,

    [RequiredField]
    string Description,

    [RequiredField]
    string ExpectedTechStack,

    [RequiredField]
    DateOnly DueDate,

    [ValidEnum(typeof(LearningTaskStatus))]
    [RequiredField]
    LearningTaskStatus? Status
);

public record LearningTaskUpdateDto
(
    [RequiredField]
    string Title,

    [RequiredField]
    string Description,

    [RequiredField]
    string ExpectedTechStack,

    [RequiredField]
    DateOnly DueDate,

    [ValidEnum(typeof(LearningTaskStatus))]
    [RequiredField]
    LearningTaskStatus Status
);

public record LearningTaskResposeDto
(
    int Id,
    
    string Title,
    
    string Description,
    
    string ExpectedTechStack,
    
    DateOnly DueDate,
    
    LearningTaskStatus? Status
);