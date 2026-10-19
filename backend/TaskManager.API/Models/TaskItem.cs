namespace TaskManager.API.Models;

// Named TaskItem to avoid clashing with System.Threading.Tasks.Task
public class TaskItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public TaskItemStatus Status { get; set; } = TaskItemStatus.ToDo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;
    public DateTime? DueDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public int ProjectId { get; set; }
    public Project Project { get; set; } = null!;

    public int? AssignedToId { get; set; }
    public User? AssignedTo { get; set; }

    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    public ICollection<SubTask> SubTasks { get; set; } = new List<SubTask>();
}

public enum TaskItemStatus
{
    ToDo,
    InProgress,
    Done
}

public enum TaskPriority
{
    Low,
    Medium,
    High
}
