using Microsoft.EntityFrameworkCore;
using TaskManager.API.Models;

namespace TaskManager.API.Data;

// Seeds demo data on first run so the app has something to show
public static class DbSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (await db.Users.AnyAsync()) return;

        var admin = new User
        {
            Name = "Admin User",
            Email = "admin@taskmanager.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
            Role = Roles.Admin
        };
        var john = new User
        {
            Name = "John Silva",
            Email = "john@taskmanager.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("John@123"),
            Role = Roles.Member
        };
        var nimali = new User
        {
            Name = "Nimali Perera",
            Email = "nimali@taskmanager.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Nimali@123"),
            Role = Roles.Member
        };
        db.Users.AddRange(admin, john, nimali);

        var project = new Project
        {
            Name = "E-Commerce Website",
            Description = "Build an online store with cart and payment integration",
            Owner = admin
        };
        project.Members.Add(new ProjectMember { User = admin });
        project.Members.Add(new ProjectMember { User = john });
        project.Members.Add(new ProjectMember { User = nimali });

        var now = DateTime.UtcNow;
        project.Tasks.Add(new TaskItem { Title = "Design database schema", Status = TaskItemStatus.Done, Priority = TaskPriority.High, AssignedTo = admin, DueDate = now.AddDays(-5) });
        project.Tasks.Add(new TaskItem { Title = "Create login page", Status = TaskItemStatus.Done, Priority = TaskPriority.Medium, AssignedTo = john, DueDate = now.AddDays(-2) });
        project.Tasks.Add(new TaskItem { Title = "Product listing API", Description = "GET /api/products with pagination", Status = TaskItemStatus.InProgress, Priority = TaskPriority.High, AssignedTo = john, DueDate = now.AddDays(3) });
        project.Tasks.Add(new TaskItem { Title = "Shopping cart UI", Status = TaskItemStatus.InProgress, Priority = TaskPriority.Medium, AssignedTo = nimali, DueDate = now.AddDays(5) });
        project.Tasks.Add(new TaskItem { Title = "Payment gateway integration", Status = TaskItemStatus.ToDo, Priority = TaskPriority.High, AssignedTo = admin, DueDate = now.AddDays(10) });
        project.Tasks.Add(new TaskItem { Title = "Write unit tests", Status = TaskItemStatus.ToDo, Priority = TaskPriority.Low, AssignedTo = nimali, DueDate = now.AddDays(14) });
        db.Projects.Add(project);

        var mobile = new Project
        {
            Name = "Mobile App",
            Description = "React Native companion app",
            Owner = john
        };
        mobile.Members.Add(new ProjectMember { User = john });
        mobile.Members.Add(new ProjectMember { User = nimali });
        mobile.Tasks.Add(new TaskItem { Title = "Setup React Native project", Status = TaskItemStatus.Done, Priority = TaskPriority.Medium, AssignedTo = john });
        mobile.Tasks.Add(new TaskItem { Title = "Push notifications", Status = TaskItemStatus.ToDo, Priority = TaskPriority.Medium, AssignedTo = nimali, DueDate = now.AddDays(7) });
        db.Projects.Add(mobile);

        await db.SaveChangesAsync();
    }
}
