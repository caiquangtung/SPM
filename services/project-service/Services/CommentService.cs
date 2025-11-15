using Microsoft.EntityFrameworkCore;
using project_service.Data;
using project_service.DTOs.Comments;
using project_service.Models;
using project_service.Repositories.Interfaces;
using project_service.Services.Interfaces;

namespace project_service.Services;

public class CommentService : ICommentService
{
    private readonly ICommentRepository _comments;
    private readonly ICommentEmbeddingRepository _commentEmbeddings;
    private readonly ITaskRepository _tasks;
    private readonly ProjectDbContext _db;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly IEmbeddingService _embeddingService;

    public CommentService(
        ICommentRepository comments,
        ICommentEmbeddingRepository commentEmbeddings,
        ITaskRepository tasks,
        ProjectDbContext db,
        IKafkaProducerService kafkaProducer,
        IEmbeddingService embeddingService)
    {
        _comments = comments;
        _commentEmbeddings = commentEmbeddings;
        _tasks = tasks;
        _db = db;
        _kafkaProducer = kafkaProducer;
        _embeddingService = embeddingService;
    }

    public async Task<IEnumerable<CommentResponse>> GetByTaskAsync(Guid taskId, CancellationToken cancellationToken = default)
    {
        var list = await _comments.GetByTaskAsync(taskId, cancellationToken);
        return list.Select(CommentResponse.FromEntity);
    }

    public async Task<CommentResponse> CreateAsync(Guid taskId, Guid userId, CreateCommentRequest request, CancellationToken cancellationToken = default)
    {
        using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);
        try
        {
            // Optional existence check; lightweight for now
            var task = await _tasks.GetByIdAsync(taskId, cancellationToken);
            if (task == null) throw new KeyNotFoundException("Task not found");

            var entity = new ProjectComment
            {
                TaskId = taskId,
                UserId = userId,
                Content = request.Content
            };
            _ = _comments.CreateAsync(entity);
            await _db.SaveChangesAsync(cancellationToken);

            // Generate and save embedding (async, non-blocking)
            _ = GenerateAndSaveEmbeddingAsync(entity, taskId, cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            // Publish event after transaction commit (fire-and-forget for performance)
            _ = _kafkaProducer.PublishCommentCreatedAsync(entity.Id, taskId, userId, entity.Content);

            return CommentResponse.FromEntity(entity);
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    private async Task GenerateAndSaveEmbeddingAsync(ProjectComment comment, Guid taskId, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(comment.Content))
                return;

            var embedding = await _embeddingService.GenerateEmbeddingAsync(comment.Content, cancellationToken);

            var commentEmbedding = new CommentEmbedding
            {
                CommentId = comment.Id,
                TaskId = taskId,
                Embedding = embedding
            };

            _ = _commentEmbeddings.CreateOrUpdateAsync(commentEmbedding);
            await _db.SaveChangesAsync(cancellationToken);
        }
        catch (Exception)
        {
            // Log error but don't fail the comment creation
            // Embedding can be regenerated later if needed
        }
    }
}


