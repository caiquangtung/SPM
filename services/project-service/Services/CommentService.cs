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
    private readonly ITaskRepository _tasks;
    private readonly ProjectDbContext _db;
    private readonly IKafkaProducerService _kafkaProducer;

    public CommentService(ICommentRepository comments, ITaskRepository tasks, ProjectDbContext db, IKafkaProducerService kafkaProducer)
    {
        _comments = comments;
        _tasks = tasks;
        _db = db;
        _kafkaProducer = kafkaProducer;
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
            _comments.CreateAsync(entity);
            await _db.SaveChangesAsync(cancellationToken);
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
}


