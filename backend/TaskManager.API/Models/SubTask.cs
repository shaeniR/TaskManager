namespace TaskManager.API.Models;

public class SubTask
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsDone { get; set; } 

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int TaskId { get; set; } // foreign key -> MySQL column
    public TaskItem Task { get; set; } = null!; // navigation property -> C# object
}

public enum SubTasks
{
    NotStarted,
    InProgress,
    Completed
}