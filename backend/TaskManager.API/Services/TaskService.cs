using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Exceptions;
using TaskManager.API.Models;

namespace TaskManager.API.Services;

public interface ITaskService
{
    Task<PagedResult<TaskDto>> GetProjectTasksAsync(int projectId, TaskQueryParams q, int userId, bool isAdmin);
    Task<TaskDto> GetTaskAsync(int taskId, int userId, bool isAdmin);
    Task<TaskDto> CreateTaskAsync(TaskCreateDto dto, int userId, bool isAdmin);
    Task<TaskDto> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId, bool isAdmin);
    Task<TaskDto> UpdateStatusAsync(int taskId, TaskItemStatus status, int userId, bool isAdmin);
    Task DeleteTaskAsync(int taskId, int userId, bool isAdmin);
    Task<List<CommentDto>> GetCommentsAsync(int taskId, int userId, bool isAdmin);
    Task<CommentDto> AddCommentAsync(int taskId, CommentCreateDto dto, int userId, bool isAdmin);
}

public class TaskService : ITaskService
{
    private readonly AppDbContext _db;
    private readonly IProjectService _projects;

    public TaskService(AppDbContext db, IProjectService projects)
    {
        _db = db;
        _projects = projects;
    }

    public async Task<PagedResult<TaskDto>> GetProjectTasksAsync(int projectId, TaskQueryParams q, int userId, bool isAdmin)
    {
        await _projects.EnsureMemberAsync(projectId, userId, isAdmin);

        var query = WithIncludes().Where(t => t.ProjectId == projectId);

        if (q.Status.HasValue) query = query.Where(t => t.Status == q.Status);
        if (q.Priority.HasValue) query = query.Where(t => t.Priority == q.Priority);
        if (q.AssignedToId.HasValue) query = query.Where(t => t.AssignedToId == q.AssignedToId);
        if (!string.IsNullOrWhiteSpace(q.Search))
        {
            var term = q.Search.Trim();
            query = query.Where(t => t.Title.Contains(term) || (t.Description != null && t.Description.Contains(term)));
        }

        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var total = await query.CountAsync();

        var items = await query
            // Priority is stored as a string, so order by an explicit rank (High first)
            .OrderBy(t => t.Priority == TaskPriority.High ? 0 : t.Priority == TaskPriority.Medium ? 1 : 2)
            .ThenBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<TaskDto>(items.Select(t => t.ToDto()).ToList(), page, pageSize, total);
    }

    public async Task<TaskDto> GetTaskAsync(int taskId, int userId, bool isAdmin)
    {
        var task = await LoadTaskAsync(taskId);
        await _projects.EnsureMemberAsync(task.ProjectId, userId, isAdmin);
        return task.ToDto();
    }

    public async Task<TaskDto> CreateTaskAsync(TaskCreateDto dto, int userId, bool isAdmin)
    {
        await _projects.EnsureMemberAsync(dto.ProjectId, userId, isAdmin);
        await EnsureAssigneeIsMemberAsync(dto.ProjectId, dto.AssignedToId);

        var task = new TaskItem
        {
            ProjectId = dto.ProjectId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Priority = dto.Priority,
            DueDate = dto.DueDate,
            AssignedToId = dto.AssignedToId
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        return (await LoadTaskAsync(task.Id)).ToDto();
    }

    public async Task<TaskDto> UpdateTaskAsync(int taskId, TaskUpdateDto dto, int userId, bool isAdmin)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new NotFoundException("Task not found.");
        await _projects.EnsureMemberAsync(task.ProjectId, userId, isAdmin);
        await EnsureAssigneeIsMemberAsync(task.ProjectId, dto.AssignedToId);

        task.Title = dto.Title.Trim();
        task.Description = dto.Description?.Trim();
        task.Status = dto.Status;
        task.Priority = dto.Priority;
        task.DueDate = dto.DueDate;
        task.AssignedToId = dto.AssignedToId;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (await LoadTaskAsync(taskId)).ToDto();
    }

    public async Task<TaskDto> UpdateStatusAsync(int taskId, TaskItemStatus status, int userId, bool isAdmin)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new NotFoundException("Task not found.");
        await _projects.EnsureMemberAsync(task.ProjectId, userId, isAdmin);

        task.Status = status;
        task.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        return (await LoadTaskAsync(taskId)).ToDto();
    }

    public async Task DeleteTaskAsync(int taskId, int userId, bool isAdmin)
    {
        var task = await _db.Tasks.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == taskId)
            ?? throw new NotFoundException("Task not found.");

        // Only an Admin or the project owner may delete tasks
        if (!isAdmin && task.Project.OwnerId != userId)
            throw new ForbiddenException("Only the project owner or an admin can delete tasks.");

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();
    }

    public async Task<List<CommentDto>> GetCommentsAsync(int taskId, int userId, bool isAdmin)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new NotFoundException("Task not found.");
        await _projects.EnsureMemberAsync(task.ProjectId, userId, isAdmin);

        var comments = await _db.Comments.AsNoTracking()
            .Include(c => c.User)
            .Where(c => c.TaskId == taskId)
            .OrderBy(c => c.CreatedAt)
            .ToListAsync();
        return comments.Select(c => c.ToDto()).ToList();
    }

    public async Task<CommentDto> AddCommentAsync(int taskId, CommentCreateDto dto, int userId, bool isAdmin)
    {
        var task = await _db.Tasks.FindAsync(taskId) ?? throw new NotFoundException("Task not found.");
        await _projects.EnsureMemberAsync(task.ProjectId, userId, isAdmin);

        var comment = new Comment { TaskId = taskId, UserId = userId, Content = dto.Content.Trim() };
        _db.Comments.Add(comment);
        await _db.SaveChangesAsync();

        await _db.Entry(comment).Reference(c => c.User).LoadAsync();
        return comment.ToDto();
    }

    private IQueryable<TaskItem> WithIncludes() => _db.Tasks.AsNoTracking()
        .Include(t => t.Project)
        .Include(t => t.AssignedTo)
        .Include(t => t.Comments);

    private async Task<TaskItem> LoadTaskAsync(int taskId) =>
        await WithIncludes().FirstOrDefaultAsync(t => t.Id == taskId)
        ?? throw new NotFoundException("Task not found.");

    private async Task EnsureAssigneeIsMemberAsync(int projectId, int? assigneeId)
    {
        if (assigneeId is null) return;
        if (!await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == assigneeId))
            throw new BadRequestException("A task can only be assigned to a project member.");
    }
}
