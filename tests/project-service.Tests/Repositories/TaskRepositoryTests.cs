using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.Models;
using project_service.Repositories;
using System.Linq;
using Xunit;

namespace project_service.Tests.Repositories;

public class TaskRepositoryTests
{
    private static ProjectDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<ProjectDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ProjectDbContext(options);
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsTasksOrderedByCreatedDate()
    {
        await using var context = CreateDbContext();
        var repository = new TaskRepository(context);
        var projectId = Guid.NewGuid();

        context.Tasks.AddRange(
            new ProjectTask { ProjectId = projectId, Title = "Older task", CreatedAt = DateTime.UtcNow.AddMinutes(-10) },
            new ProjectTask { ProjectId = projectId, Title = "Newest task", CreatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        var result = (await repository.GetByProjectAsync(projectId)).ToList();

        result.Should().HaveCount(2);
        result.First().Title.Should().Be("Newest task");
    }

    [Fact]
    public async Task GetByProjectAsync_ReturnsOnlyTasksForRequestedProject()
    {
        await using var context = CreateDbContext();
        var repository = new TaskRepository(context);
        var projectId = Guid.NewGuid();

        context.Tasks.AddRange(
            new ProjectTask
            {
                ProjectId = projectId,
                Title = "Task 1"
            },
            new ProjectTask
            {
                ProjectId = projectId,
                Title = "Task 2"
            },
            new ProjectTask
            {
                ProjectId = projectId,
                Title = "Task 3"
            }
        );
        context.Tasks.Add(new ProjectTask
        {
            ProjectId = Guid.NewGuid(),
            Title = "Other project"
        });
        await context.SaveChangesAsync();

        var result = (await repository.GetByProjectAsync(projectId)).ToList();

        result.Should().HaveCount(3);
        result.Should().OnlyContain(t => t.ProjectId == projectId);
    }
}
