using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.API.Extensions;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly ITaskService _tasks;

    public TasksController(ITaskService tasks) => _tasks = tasks;

    [HttpGet("{id:int}")]
    public async Task<ActionResult<TaskDto>> Get(int id) =>
        Ok(await _tasks.GetTaskAsync(id, User.GetUserId(), User.IsAdmin()));

    [HttpPost]
    public async Task<ActionResult<TaskDto>> Create(TaskCreateDto dto)
    {
        var task = await _tasks.CreateTaskAsync(dto, User.GetUserId(), User.IsAdmin());
        return CreatedAtAction(nameof(Get), new { id = task.Id }, task);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<TaskDto>> Update(int id, TaskUpdateDto dto) =>
        Ok(await _tasks.UpdateTaskAsync(id, dto, User.GetUserId(), User.IsAdmin()));

    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<TaskDto>> UpdateStatus(int id, TaskStatusUpdateDto dto) =>
        Ok(await _tasks.UpdateStatusAsync(id, dto.Status, User.GetUserId(), User.IsAdmin()));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _tasks.DeleteTaskAsync(id, User.GetUserId(), User.IsAdmin());
        return NoContent();
    }

    [HttpGet("{id:int}/comments")]
    public async Task<ActionResult<List<CommentDto>>> GetComments(int id) =>
        Ok(await _tasks.GetCommentsAsync(id, User.GetUserId(), User.IsAdmin()));

    [HttpPost("{id:int}/comments")]
    public async Task<ActionResult<CommentDto>> AddComment(int id, CommentCreateDto dto) =>
        Ok(await _tasks.AddCommentAsync(id, dto, User.GetUserId(), User.IsAdmin()));
}
