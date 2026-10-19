namespace TaskManager.API.DTOs;

public record PagedResult<T>(List<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public record DashboardDto(
    int TotalProjects,
    int TotalTasks,
    int ToDo,
    int InProgress,
    int Done,
    int Overdue,
    int MyOpenTasks,
    List<TaskDto> UpcomingTasks);
