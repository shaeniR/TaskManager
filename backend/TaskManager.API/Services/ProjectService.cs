using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Exceptions;
using TaskManager.API.Models;

namespace TaskManager.API.Services;

public interface IProjectService
{
    Task<List<ProjectSummaryDto>> GetProjectsAsync(int userId, bool isAdmin);
    Task<ProjectDetailDto> GetProjectAsync(int projectId, int userId, bool isAdmin);
    Task<ProjectDetailDto> CreateProjectAsync(ProjectCreateDto dto, int userId);
    Task<ProjectDetailDto> UpdateProjectAsync(int projectId, ProjectUpdateDto dto, int userId, bool isAdmin);
    Task DeleteProjectAsync(int projectId, int userId, bool isAdmin);
    Task<ProjectDetailDto> AddMemberAsync(int projectId, AddMemberDto dto, int userId, bool isAdmin);
    Task RemoveMemberAsync(int projectId, int memberId, int userId, bool isAdmin);
    Task EnsureMemberAsync(int projectId, int userId, bool isAdmin);
}

public class ProjectService : IProjectService
{
    private readonly AppDbContext _db;

    public ProjectService(AppDbContext db) => _db = db;

    public async Task<List<ProjectSummaryDto>> GetProjectsAsync(int userId, bool isAdmin)
    {
        //get the projects from the database using Entity Framework Core. 
        // I use AsNoTracking() because I only need to read the data, not update it.
        var query = _db.Projects.AsNoTracking();
        if (!isAdmin)
            query = query.Where(p => p.Members.Any(m => m.UserId == userId));

        return await query
            .OrderByDescending(p => p.CreatedAt)
            .Select(p => new ProjectSummaryDto(
                p.Id,
                p.Name,
                p.Description,
                new UserDto(p.Owner.Id, p.Owner.Name, p.Owner.Email, p.Owner.Role),
                p.Members.Count,
                p.Tasks.Count,
                p.Tasks.Count(t => t.Status == TaskItemStatus.Done),
                p.CreatedAt))
            .ToListAsync();
    }

    public async Task<ProjectDetailDto> GetProjectAsync(int projectId, int userId, bool isAdmin)
    {
        await EnsureMemberAsync(projectId, userId, isAdmin);
        return await LoadDetailAsync(projectId);
    }

    public async Task<ProjectDetailDto> CreateProjectAsync(ProjectCreateDto dto, int userId)
    {
        var project = new Project
        {
            Name = dto.Name.Trim(),
            Description = dto.Description?.Trim(),
            OwnerId = userId
        };
        // The creator is automatically a member of the project
        project.Members.Add(new ProjectMember { UserId = userId });

        _db.Projects.Add(project);
        await _db.SaveChangesAsync();
        return await LoadDetailAsync(project.Id);
    }

    public async Task<ProjectDetailDto> UpdateProjectAsync(int projectId, ProjectUpdateDto dto, int userId, bool isAdmin)
    {
        var project = await GetOwnedProjectAsync(projectId, userId, isAdmin);
        project.Name = dto.Name.Trim();
        project.Description = dto.Description?.Trim();
        await _db.SaveChangesAsync();
        return await LoadDetailAsync(projectId);
    }

    public async Task DeleteProjectAsync(int projectId, int userId, bool isAdmin)
    {
        var project = await GetOwnedProjectAsync(projectId, userId, isAdmin);
        _db.Projects.Remove(project);
        await _db.SaveChangesAsync();
    }

    public async Task<ProjectDetailDto> AddMemberAsync(int projectId, AddMemberDto dto, int userId, bool isAdmin)
    {
        await GetOwnedProjectAsync(projectId, userId, isAdmin);

        if (!await _db.Users.AnyAsync(u => u.Id == dto.UserId))
            throw new NotFoundException("User not found.");
        if (await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == dto.UserId))
            throw new BadRequestException("User is already a member of this project.");

        _db.ProjectMembers.Add(new ProjectMember { ProjectId = projectId, UserId = dto.UserId });
        await _db.SaveChangesAsync();
        return await LoadDetailAsync(projectId);
    }

    public async Task RemoveMemberAsync(int projectId, int memberId, int userId, bool isAdmin)
    {
        var project = await GetOwnedProjectAsync(projectId, userId, isAdmin);
        if (project.OwnerId == memberId)
            throw new BadRequestException("The project owner cannot be removed.");

        var membership = await _db.ProjectMembers.FindAsync(projectId, memberId)
            ?? throw new NotFoundException("Member not found in this project.");

        // Unassign this member's tasks in the project
        var tasks = await _db.Tasks.Where(t => t.ProjectId == projectId && t.AssignedToId == memberId).ToListAsync();
        tasks.ForEach(t => t.AssignedToId = null);

        _db.ProjectMembers.Remove(membership);
        await _db.SaveChangesAsync();
    }

    public async Task EnsureMemberAsync(int projectId, int userId, bool isAdmin)
    {
        if (!await _db.Projects.AnyAsync(p => p.Id == projectId))
            throw new NotFoundException("Project not found.");
        if (isAdmin) return;
        if (!await _db.ProjectMembers.AnyAsync(m => m.ProjectId == projectId && m.UserId == userId))
            throw new ForbiddenException("You are not a member of this project.");
    }

    private async Task<Project> GetOwnedProjectAsync(int projectId, int userId, bool isAdmin)
    {
        var project = await _db.Projects.FindAsync(projectId)
            ?? throw new NotFoundException("Project not found.");
        if (!isAdmin && project.OwnerId != userId)
            throw new ForbiddenException("Only the project owner can do this.");
        return project;
    }

    private async Task<ProjectDetailDto> LoadDetailAsync(int projectId)
    {
        var p = await _db.Projects.AsNoTracking()
            .Include(p => p.Owner)
            .Include(p => p.Members).ThenInclude(m => m.User)
            .FirstAsync(p => p.Id == projectId);

        return new ProjectDetailDto(
            p.Id,
            p.Name,
            p.Description,
            p.Owner.ToDto(),
            p.Members.OrderBy(m => m.User.Name).Select(m => m.User.ToDto()).ToList(),
            p.CreatedAt);
    }
}
