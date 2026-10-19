using Microsoft.EntityFrameworkCore;
using TaskManager.API.Data;
using TaskManager.API.DTOs;
using TaskManager.API.Models;

namespace TaskManager.API.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(int userId, bool isAdmin);
}

public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardDto> GetDashboardAsync(int userId, bool isAdmin)
    {
        var projects = _db.Projects.AsNoTracking();
        var tasks = _db.Tasks.AsNoTracking();
        if (!isAdmin)
        {
            projects = projects.Where(p => p.Members.Any(m => m.UserId == userId));
            tasks = tasks.Where(t => t.Project.Members.Any(m => m.UserId == userId));
        }

        // One grouped query instead of three separate COUNTs
        var statusCounts = await tasks
            .GroupBy(t => t.Status)
            .Select(g => new { Status = g.Key, Count = g.Count() })
            .ToDictionaryAsync(x => x.Status, x => x.Count);

        int Count(TaskItemStatus s) => statusCounts.GetValueOrDefault(s);

        var now = DateTime.UtcNow;
        var overdue = await tasks.CountAsync(t => t.Status != TaskItemStatus.Done && t.DueDate != null && t.DueDate < now);
        var myOpen = await tasks.CountAsync(t => t.AssignedToId == userId && t.Status != TaskItemStatus.Done);

        var upcoming = await tasks
            .Include(t => t.Project)
            .Include(t => t.AssignedTo)
            .Include(t => t.Comments)
            .Where(t => t.AssignedToId == userId && t.Status != TaskItemStatus.Done)
            .OrderBy(t => t.DueDate == null)
            .ThenBy(t => t.DueDate)
            .Take(5)
            .ToListAsync();

        return new DashboardDto(
            await projects.CountAsync(),
            statusCounts.Values.Sum(),
            Count(TaskItemStatus.ToDo),
            Count(TaskItemStatus.InProgress),
            Count(TaskItemStatus.Done),
            overdue,
            myOpen,
            upcoming.Select(t => t.ToDto()).ToList());
    }
}
