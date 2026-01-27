using Microsoft.EntityFrameworkCore;
using project_service.Models;
using TaskStatus = project_service.Models.TaskStatus;

namespace project_service.Data;

public static class SeedData
{
    // Fixed GUID for default admin user (matches user-service seed)
    private static readonly Guid AdminUserId = new Guid("11111111-1111-1111-1111-111111111111");

    public static void EnsureSeedData(ProjectDbContext context, ILogger logger)
    {
        // If there are already projects, do nothing
        if (context.Projects.Any())
        {
            return;
        }

        logger.LogInformation("No projects found in database. Seeding sample projects...");

        // Create sample projects
        var project1 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "SPM Development",
            Description = "Software Project Management system development project",
            CreatedBy = AdminUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var project2 = new Project
        {
            Id = Guid.NewGuid(),
            Name = "Mobile App",
            Description = "Cross-platform mobile application for project management",
            CreatedBy = AdminUserId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };

        context.Projects.AddRange(project1, project2);
        context.SaveChanges();

        // Add admin as owner of both projects
        var member1 = new ProjectMember
        {
            ProjectId = project1.Id,
            UserId = AdminUserId,
            Role = ProjectMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        var member2 = new ProjectMember
        {
            ProjectId = project2.Id,
            UserId = AdminUserId,
            Role = ProjectMemberRole.Owner,
            JoinedAt = DateTime.UtcNow
        };

        context.ProjectMembers.AddRange(member1, member2);
        context.SaveChanges();

        // Create sample tasks for project 1
        var tasks = new List<ProjectTask>
        {
            new ProjectTask
            {
                ProjectId = project1.Id,
                Title = "Setup authentication system",
                Description = "Implement JWT-based authentication with refresh tokens",
                Status = TaskStatus.Done,
                Priority = TaskPriority.High,
                CreatedBy = AdminUserId,
                AssignedTo = AdminUserId,
                CreatedAt = DateTime.UtcNow.AddDays(-7),
                UpdatedAt = DateTime.UtcNow.AddDays(-2),
                DueDate = DateTime.UtcNow.AddDays(-3)
            },
            new ProjectTask
            {
                ProjectId = project1.Id,
                Title = "Implement project management features",
                Description = "Create, read, update, delete operations for projects",
                Status = TaskStatus.InProgress,
                Priority = TaskPriority.High,
                CreatedBy = AdminUserId,
                AssignedTo = AdminUserId,
                CreatedAt = DateTime.UtcNow.AddDays(-5),
                UpdatedAt = DateTime.UtcNow.AddDays(-1),
                DueDate = DateTime.UtcNow.AddDays(3)
            },
            new ProjectTask
            {
                ProjectId = project1.Id,
                Title = "Setup task board with drag-and-drop",
                Description = "Implement Kanban board with react-beautiful-dnd",
                Status = TaskStatus.ToDo,
                Priority = TaskPriority.Medium,
                CreatedBy = AdminUserId,
                CreatedAt = DateTime.UtcNow.AddDays(-3),
                UpdatedAt = DateTime.UtcNow.AddDays(-3),
                DueDate = DateTime.UtcNow.AddDays(7)
            },
            new ProjectTask
            {
                ProjectId = project1.Id,
                Title = "Add AI-powered search with embeddings",
                Description = "Integrate pgvector for semantic search of tasks and comments",
                Status = TaskStatus.InReview,
                Priority = TaskPriority.Medium,
                CreatedBy = AdminUserId,
                AssignedTo = AdminUserId,
                CreatedAt = DateTime.UtcNow.AddDays(-2),
                UpdatedAt = DateTime.UtcNow,
                DueDate = DateTime.UtcNow.AddDays(5)
            }
        };

        context.Tasks.AddRange(tasks);
        context.SaveChanges();

        logger.LogInformation(
            "Seeded {ProjectCount} projects with {TaskCount} tasks (Development only).",
            2,
            tasks.Count);
    }
}
