using TaskManager.API.DTOs;
using TaskManager.API.Models;

namespace TaskManager.API.Services;

// Entity -> DTO mapping, so entities (e.g. PasswordHash) never leak out of the API
public static class Mappings
{
    public static UserDto ToDto(this User u) => new(u.Id, u.Name, u.Email, u.Role);

    public static TaskDto ToDto(this TaskItem t) => new(
        t.Id,
        t.Title,
        t.Description,
        t.Status,
        t.Priority,
        t.DueDate,
        t.ProjectId,
        t.Project?.Name ?? string.Empty,
        t.AssignedTo?.ToDto(),
        t.Comments.Count,
        t.CreatedAt,
        t.UpdatedAt);

    public static CommentDto ToDto(this Comment c) => new(c.Id, c.Content, c.User.ToDto(), c.CreatedAt);


    public static SubTaskDto ToDto(this SubTask st) => new(st.Id, st.Title, st.IsDone);}
