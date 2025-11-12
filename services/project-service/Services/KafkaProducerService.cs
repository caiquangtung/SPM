using Confluent.Kafka;
using System.Text.Json;
using project_service.Services.Interfaces;

namespace project_service.Services;

public class KafkaProducerService : IKafkaProducerService, IDisposable
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaProducerService> _logger;
    private readonly IProducer<Null, string> _producer;
    private bool _disposed = false;

    public KafkaProducerService(IConfiguration configuration, ILogger<KafkaProducerService> logger)
    {
        _configuration = configuration;
        _logger = logger;

        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "kafka:9092";
        var config = new ProducerConfig
        {
            BootstrapServers = bootstrapServers
        };

        _producer = new ProducerBuilder<Null, string>(config).Build();
    }

    public async Task PublishProjectCreatedAsync(Guid projectId, Guid createdBy, string name)
    {
        try
        {
            var message = new
            {
                ProjectId = projectId,
                CreatedBy = createdBy,
                Name = name,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("project.created", kafkaMessage);
            _logger.LogInformation("Published project.created event for project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish project.created event for project {ProjectId}", projectId);
        }
    }

    public async Task PublishProjectUpdatedAsync(Guid projectId, Guid updatedBy, string name)
    {
        try
        {
            var message = new
            {
                ProjectId = projectId,
                UpdatedBy = updatedBy,
                Name = name,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("project.updated", kafkaMessage);
            _logger.LogInformation("Published project.updated event for project {ProjectId}", projectId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish project.updated event for project {ProjectId}", projectId);
        }
    }

    public async Task PublishTaskCreatedAsync(Guid taskId, Guid projectId, Guid createdBy, string title)
    {
        try
        {
            var message = new
            {
                TaskId = taskId,
                ProjectId = projectId,
                CreatedBy = createdBy,
                Title = title,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("task.created", kafkaMessage);
            _logger.LogInformation("Published task.created event for task {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task.created event for task {TaskId}", taskId);
        }
    }

    public async Task PublishTaskUpdatedAsync(Guid taskId, Guid projectId, Guid updatedBy, string title)
    {
        try
        {
            var message = new
            {
                TaskId = taskId,
                ProjectId = projectId,
                UpdatedBy = updatedBy,
                Title = title,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("task.updated", kafkaMessage);
            _logger.LogInformation("Published task.updated event for task {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task.updated event for task {TaskId}", taskId);
        }
    }

    public async Task PublishTaskStatusChangedAsync(Guid taskId, Guid projectId, string oldStatus, string newStatus)
    {
        try
        {
            var message = new
            {
                TaskId = taskId,
                ProjectId = projectId,
                OldStatus = oldStatus,
                NewStatus = newStatus,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("task.status.changed", kafkaMessage);
            _logger.LogInformation("Published task.status.changed event for task {TaskId}", taskId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish task.status.changed event for task {TaskId}", taskId);
        }
    }

    public async Task PublishCommentCreatedAsync(Guid commentId, Guid taskId, Guid userId, string content)
    {
        try
        {
            var message = new
            {
                CommentId = commentId,
                TaskId = taskId,
                UserId = userId,
                Content = content,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("comment.created", kafkaMessage);
            _logger.LogInformation("Published comment.created event for comment {CommentId}", commentId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish comment.created event for comment {CommentId}", commentId);
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _producer?.Dispose();
            }
            _disposed = true;
        }
    }
}

