using TaskManager.API.Models;

namespace TaskManager.API.DTOs;

public record TaskCreateDto(
    int ProjectId,
    string Title,
    string? Description,
    TaskPriority Priority,
    DateTime? DueDate,
    int? AssignedToId);

public record TaskUpdateDto(
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    int? AssignedToId);

public record TaskStatusUpdateDto(TaskItemStatus Status);

public record TaskDto(
    int Id,
    string Title,
    string? Description,
    TaskItemStatus Status,
    TaskPriority Priority,
    DateTime? DueDate,
    int ProjectId,
    string ProjectName,
    UserDto? AssignedTo,
    int CommentCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt);

public class TaskQueryParams
{
    public TaskItemStatus? Status { get; set; }
    public TaskPriority? Priority { get; set; }
    public int? AssignedToId { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

public record CommentCreateDto(string Content);

public record CommentDto(int Id, string Content, UserDto User, DateTime CreatedAt);
