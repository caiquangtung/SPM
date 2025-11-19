using Microsoft.EntityFrameworkCore;
using Npgsql;
using Pgvector;
using project_service.Data;
using project_service.DTOs.Tasks;
using project_service.Models;
using project_service.Repositories.Interfaces;
using project_service.Services.Interfaces;
using TaskStatusEnum = project_service.Models.TaskStatus;

namespace project_service.Services;

public class TaskService : ITaskService
{
    private readonly ITaskRepository _tasks;
    private readonly ITaskEmbeddingRepository _taskEmbeddings;
    private readonly ProjectDbContext _db;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly IEmbeddingService _embeddingService;

    public TaskService(
        ITaskRepository tasks,
        ITaskEmbeddingRepository taskEmbeddings,
        ProjectDbContext db,
        IKafkaProducerService kafkaProducer,
        IEmbeddingService embeddingService)
    {
        _tasks = tasks;
        _taskEmbeddings = taskEmbeddings;
        _db = db;
        _kafkaProducer = kafkaProducer;
        _embeddingService = embeddingService;
    }

    public async Task<IEnumerable<TaskResponse>> GetByProjectAsync(
        Guid projectId,
        TaskStatusEnum? status = null,
        Guid? assignedTo = null,
        CancellationToken cancellationToken = default)
    {
        var items = await _tasks.GetByProjectAsync(projectId, status, assignedTo, cancellationToken);
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

            _ = _tasks.CreateAsync(entity);
            await _db.SaveChangesAsync(cancellationToken);

            // Generate and save embedding (async, non-blocking)
            _ = GenerateAndSaveEmbeddingAsync(entity, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Publish event after transaction commit (fire-and-forget for performance)
            _ = _kafkaProducer.PublishTaskCreatedAsync(entity.Id, entity.ProjectId, userId, entity.Title);
            if (entity.AssignedTo.HasValue)
            {
                _ = _kafkaProducer.PublishTaskAssignedAsync(
                    entity.Id,
                    entity.ProjectId,
                    entity.AssignedTo.Value,
                    userId);
            }

            return TaskResponse.FromEntity(entity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task GenerateAndSaveEmbeddingAsync(ProjectTask task, CancellationToken cancellationToken)
    {
        try
        {
            // Combine title and description for embedding
            var textToEmbed = $"{task.Title} {task.Description ?? string.Empty}".Trim();
            if (string.IsNullOrWhiteSpace(textToEmbed))
                return;

            var embedding = await _embeddingService.GenerateEmbeddingAsync(textToEmbed, cancellationToken);

            var taskEmbedding = new TaskEmbedding
            {
                TaskId = task.Id,
                Embedding = embedding
            };

            _ = _taskEmbeddings.CreateOrUpdateAsync(taskEmbedding);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Log error but don't fail the task creation
            // Embedding can be regenerated later if needed
            // In production, consider using a background job queue for reliability
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
            _ = _tasks.UpdateAsync(entity);
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

    public async Task<IEnumerable<SearchResult>> SearchSimilarAsync(SearchTasksRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Query))
        {
            return Enumerable.Empty<SearchResult>();
        }

        try
        {
            // Generate embedding for search query
            var queryEmbedding = await _embeddingService.GenerateEmbeddingAsync(request.Query, cancellationToken);

            // Perform vector similarity search using raw SQL
            // Convert Vector to float array for SQL parameter
            var embeddingArray = queryEmbedding.ToArray();

            var query = @"
                SELECT 
                    t.id AS ""Id"",
                    t.project_id AS ""ProjectId"",
                    t.title AS ""Title"",
                    t.description AS ""Description"",
                    1 - (te.embedding <=> @queryEmbedding::vector) AS ""Similarity""
                FROM spm_project.tasks t
                INNER JOIN spm_project.task_embeddings te ON t.id = te.task_id
                WHERE (@projectId::uuid IS NULL OR t.project_id = @projectId::uuid)
                ORDER BY te.embedding <=> @queryEmbedding::vector
                LIMIT @topK";

            var projectIdParam = request.ProjectId.HasValue
                ? new NpgsqlParameter("@projectId", request.ProjectId.Value)
                : new NpgsqlParameter("@projectId", DBNull.Value) { NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Uuid };

            var results = await _db.Database
                .SqlQueryRaw<SearchResult>(
                    query,
                    new NpgsqlParameter("@queryEmbedding", NpgsqlTypes.NpgsqlDbType.Array | NpgsqlTypes.NpgsqlDbType.Real) { Value = embeddingArray },
                    projectIdParam,
                    new NpgsqlParameter("@topK", request.TopK))
                .ToListAsync(cancellationToken);

            return results;
        }
        catch (Exception)
        {
            // Log error and return empty results
            return Enumerable.Empty<SearchResult>();
        }
    }
}
