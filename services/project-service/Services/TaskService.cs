using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.DTOs.Tasks;
using project_service.Models;
using project_service.Repositories.Interfaces;
using project_service.Services.Interfaces;

namespace project_service.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly ProjectDbContext _db;

    public TaskService(ITaskRepository tasks, ProjectDbContext db)
    {
        _tasks = tasks;
        _db = db;
    }

    public async Task<IEnumerable<TaskResponse>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var items = await _tasks.GetByProjectAsync(projectId, cancellationToken);
        return items.Select(TaskResponse.FromEntity);
    }

    public async Task<TaskResponse> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var entity = new ProjectTask
        {
            ProjectId = request.ProjectId,
            Title = request.Title,
            Description = request.Description,
            AssignedTo = request.AssignedTo,
            CreatedBy = userId,
            Priority = request.Priority,
            DueDate = request.DueDate
        };

        _tasks.CreateAsync(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return TaskResponse.FromEntity(entity);
    }

    public async Task<TaskResponse?> UpdateStatusAsync(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
    {
        var entity = await _tasks.GetByIdAsync(taskId, cancellationToken);
        if (entity == null) return null;
        entity.Status = request.Status;
        _tasks.UpdateAsync(entity);
        await _db.SaveChangesAsync(cancellationToken);
        return TaskResponse.FromEntity(entity);
    }
}


