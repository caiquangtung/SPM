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
    private readonly IKafkaProducerService _kafkaProducer;

    public TaskService(ITaskRepository tasks, ProjectDbContext db, IKafkaProducerService kafkaProducer)
    {
        _tasks = tasks;
        _db = db;
        _kafkaProducer = kafkaProducer;
    }

    public async Task<IEnumerable<TaskResponse>> GetByProjectAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        var items = await _tasks.GetByProjectAsync(projectId, cancellationToken);
        return items.Select(TaskResponse.FromEntity);
    }

    public async Task<TaskResponse> CreateAsync(Guid userId, CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
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
            await transaction.CommitAsync(cancellationToken);

            // Publish event after transaction commit (fire-and-forget for performance)
            _ = _kafkaProducer.PublishTaskCreatedAsync(entity.Id, entity.ProjectId, userId, entity.Title);

            return TaskResponse.FromEntity(entity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<TaskResponse?> UpdateStatusAsync(Guid taskId, UpdateTaskStatusRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            var entity = await _tasks.GetByIdAsync(taskId, cancellationToken);
            if (entity == null) return null;

            var oldStatus = entity.Status.ToString();
            entity.Status = request.Status;
            _tasks.UpdateAsync(entity);
            await _db.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            // Publish events after transaction commit (fire-and-forget for performance)
            _ = _kafkaProducer.PublishTaskStatusChangedAsync(entity.Id, entity.ProjectId, oldStatus, request.Status.ToString());
            _ = _kafkaProducer.PublishTaskUpdatedAsync(entity.Id, entity.ProjectId, entity.CreatedBy, entity.Title);

            return TaskResponse.FromEntity(entity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }
}


