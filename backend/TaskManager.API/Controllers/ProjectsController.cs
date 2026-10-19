using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskManager.API.DTOs;
using TaskManager.API.Extensions;
using TaskManager.API.Services;

namespace TaskManager.API.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projects;
    private readonly ITaskService _tasks;

    public ProjectsController(IProjectService projects, ITaskService tasks)
    {
        _projects = projects;
        _tasks = tasks;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProjectSummaryDto>>> GetAll() =>
        Ok(await _projects.GetProjectsAsync(User.GetUserId(), User.IsAdmin()));

    [HttpGet("{id:int}")]
    public async Task<ActionResult<ProjectDetailDto>> Get(int id) =>
        Ok(await _projects.GetProjectAsync(id, User.GetUserId(), User.IsAdmin()));

    [HttpPost]
    public async Task<ActionResult<ProjectDetailDto>> Create(ProjectCreateDto dto)
    {
        var project = await _projects.CreateProjectAsync(dto, User.GetUserId());
        return CreatedAtAction(nameof(Get), new { id = project.Id }, project);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<ProjectDetailDto>> Update(int id, ProjectUpdateDto dto) =>
        Ok(await _projects.UpdateProjectAsync(id, dto, User.GetUserId(), User.IsAdmin()));

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        await _projects.DeleteProjectAsync(id, User.GetUserId(), User.IsAdmin());
        return NoContent();
    }

    [HttpPost("{id:int}/members")]
    public async Task<ActionResult<ProjectDetailDto>> AddMember(int id, AddMemberDto dto) =>
        Ok(await _projects.AddMemberAsync(id, dto, User.GetUserId(), User.IsAdmin()));

    [HttpDelete("{id:int}/members/{userId:int}")]
    public async Task<IActionResult> RemoveMember(int id, int userId)
    {
        await _projects.RemoveMemberAsync(id, userId, User.GetUserId(), User.IsAdmin());
        return NoContent();
    }

    // GET /api/projects/5/tasks?status=InProgress&search=api&page=1&pageSize=20
    [HttpGet("{id:int}/tasks")]
    public async Task<ActionResult<PagedResult<TaskDto>>> GetTasks(int id, [FromQuery] TaskQueryParams query) =>
        Ok(await _tasks.GetProjectTasksAsync(id, query, User.GetUserId(), User.IsAdmin()));
}
