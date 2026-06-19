using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Trainees.Models;
using TraineeManagementApi.Mentors.Models;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.Submissions.Models;
using System.ComponentModel.DataAnnotations.Schema;
using TraineeManagementApi.Constants;
using TraineeManagementApi.Utils.CustomValidation;

namespace TraineeManagementApi.TaskAssignments.Models;

public class TaskAssignment
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id 
    { 
        get; 
        set; 
    }

    public int TraineeId 
    { 
        get; 
        set; 
    }
    public Trainee? Trainee 
    { 
        get; 
        set; 
    }

    public int MentorId 
    { 
        get; 
        set; 
    }
    public Mentor? Mentor 
    { 
        get; 
        set; 
    }

    public int LearningTaskId 
    { 
        get; 
        set; 
    }
    public LearningTask? LearningTask 
    { 
        get; 
        set; 
    }

    [RequiredField]
    public DateOnly AssignedDate 
    { 
        get; 
        set; 
    }

    [RequiredField]
    [ValidDateRange("AssignedDate")]
    public DateOnly DueDate 
    { 
        get; 
        set; 
    }

    [EnumDataType(typeof(TaskAssignmentStatus))]
    [RequiredField]
    public TaskAssignmentStatus? Status 
    { 
        get; 
        set; 
    }

    public string Remarks 
    { 
        get; 
        set; 
    } = string.Empty;

    public ICollection<Submission> Submissions 
    { 
        get; 
        set; 
    } = new List<Submission>();
}

public enum TaskAssignmentStatus
{
    Assigned,

    Inprogress,

    Submitted,

    Reviewed,
    
    Completed
}