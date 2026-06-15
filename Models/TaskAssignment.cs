using System.ComponentModel.DataAnnotations;
using TraineeManagementApi.Trainees.Models;
using TraineeManagementApi.Mentors.Models;
using TraineeManagementApi.LearningTasks.Models;
using TraineeManagementApi.Submissions.Models;
using System.Collections;

namespace TraineeManagementApi.TaskAssignments.Models;

public class TaskAssignment
{
    [Key]
    public int Id { get; set; }

    public int TraineeId { get; set; }
    public Trainee? Trainee { get; set; }

    public int MentorId { get; set; }
    public Mentor? Mentor { get; set; }

    public int LearningTaskId { get; set; }
    public LearningTask? LearningTask { get; set; }

    [Required(ErrorMessage = "AssignedDate required")]
    public DateOnly AssignedDate { get; set; }

    [Required(ErrorMessage = "DueDate required")]
    public DateOnly DueDate { get; set; }

    [EnumDataType(typeof(TaskAssignmentStatus), ErrorMessage = "Invalid Status")]
    public TaskAssignmentStatus Status { get; set; }

    public string Remarks { get; set; } = string.Empty;

    public ICollection<Submission> Submissions { get; set; } = new List<Submission>();
}

public enum TaskAssignmentStatus
{
    Assigned,
    Inprogress,
    Submitted,
    Reviewed,
    Completed
}