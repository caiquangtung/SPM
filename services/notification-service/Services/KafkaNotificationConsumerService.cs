using System.Text.Json;
using Confluent.Kafka;
using notification_service.Models;
using notification_service.Services.Interfaces;

namespace notification_service.Services;

public class KafkaNotificationConsumerService : BackgroundService
{
    private static readonly string[] Topics =
    [
        "project.created",
        "project.updated",
        "task.created",
        "task.updated",
        "task.assigned",
        "task.status.changed",
        "comment.created"
    ];

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaNotificationConsumerService> _logger;

    public KafkaNotificationConsumerService(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<KafkaNotificationConsumerService> logger)
    {
        _scopeFactory = scopeFactory;
        _configuration = configuration;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "kafka:9092";
        var groupId = _configuration["Kafka:GroupId"] ?? "notification-service";

        var config = new ConsumerConfig
        {
            BootstrapServers = bootstrapServers,
            GroupId = groupId,
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true,
            EnableAutoOffsetStore = true,
            AllowAutoCreateTopics = false
        };

        using var consumer = new ConsumerBuilder<Ignore, string>(config).Build();
        consumer.Subscribe(Topics);

        _logger.LogInformation("Notification Kafka consumer started for topics: {Topics}", string.Join(", ", Topics));

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    var consumeResult = consumer.Consume(stoppingToken);
                    await HandleMessageAsync(consumeResult.Topic, consumeResult.Message.Value, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (ConsumeException ex)
                {
                    _logger.LogError(ex, "Kafka consume error");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unhandled notification consumer error");
                }
            }
        }
        finally
        {
            consumer.Close();
        }
    }

    private async Task HandleMessageAsync(string topic, string payload, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();

        try
        {
            var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            switch (topic)
            {
                case "project.created":
                    await CreateProjectCreatedAsync(notificationService, root, cancellationToken);
                    break;
                case "project.updated":
                    await CreateProjectUpdatedAsync(notificationService, root, cancellationToken);
                    break;
                case "task.created":
                    await CreateTaskCreatedAsync(notificationService, root, cancellationToken);
                    break;
                case "task.updated":
                    await CreateTaskUpdatedAsync(notificationService, root, cancellationToken);
                    break;
                case "task.assigned":
                    await CreateTaskAssignedAsync(notificationService, root, cancellationToken);
                    break;
                case "task.status.changed":
                    await CreateTaskStatusChangedAsync(notificationService, root, cancellationToken);
                    break;
                case "comment.created":
                    await CreateCommentCreatedAsync(notificationService, root, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to handle notification event {Topic}", topic);
        }
    }

    private static async Task CreateProjectCreatedAsync(INotificationService service, JsonElement root, CancellationToken cancellationToken)
    {
        var userId = ReadGuid(root, "CreatedBy");
        if (userId == null) return;

        var name = ReadString(root, "Name") ?? "Project";
        var projectId = ReadGuid(root, "ProjectId")?.ToString();

        await service.CreateAsync(
            userId.Value,
            NotificationType.ProjectCreated,
            "Project created",
            $"Project '{name}' was created successfully.",
            projectId,
            "project.created",
            cancellationToken);
    }

    private static async Task CreateProjectUpdatedAsync(INotificationService service, JsonElement root, CancellationToken cancellationToken)
    {
        var userId = ReadGuid(root, "UpdatedBy");
        if (userId == null) return;

        var name = ReadString(root, "Name") ?? "Project";
        var projectId = ReadGuid(root, "ProjectId")?.ToString();

        await service.CreateAsync(
            userId.Value,
            NotificationType.ProjectUpdated,
            "Project updated",
            $"Project '{name}' was updated.",
            projectId,
            "project.updated",
            cancellationToken);
    }

    private static async Task CreateTaskCreatedAsync(INotificationService service, JsonElement root, CancellationToken cancellationToken)
    {
        var userId = ReadGuid(root, "CreatedBy");
        if (userId == null) return;

        var title = ReadString(root, "Title") ?? "Task";
        var taskId = ReadGuid(root, "TaskId")?.ToString();

        await service.CreateAsync(
            userId.Value,
            NotificationType.TaskCreated,
            "Task created",
            $"Task '{title}' was created.",
            taskId,
            "task.created",
            cancellationToken);
    }

    private static async Task CreateTaskUpdatedAsync(INotificationService service, JsonElement root, CancellationToken cancellationToken)
    {
        var userId = ReadGuid(root, "UpdatedBy");
        if (userId == null) return;

        var title = ReadString(root, "Title") ?? "Task";
        var taskId = ReadGuid(root, "TaskId")?.ToString();

        await service.CreateAsync(
            userId.Value,
            NotificationType.TaskUpdated,
            "Task updated",
            $"Task '{title}' was updated.",
            taskId,
            "task.updated",
            cancellationToken);
    }

    private static async Task CreateTaskAssignedAsync(INotificationService service, JsonElement root, CancellationToken cancellationToken)
    {
        var userId = ReadGuid(root, "AssignedTo");
        if (userId == null) return;

        var taskId = ReadGuid(root, "TaskId")?.ToString();

        await service.CreateAsync(
            userId.Value,
            NotificationType.TaskAssigned,
            "Task assigned",
            "A task has been assigned to you.",
            taskId,
            "task.assigned",
            cancellationToken);
    }

    private static async Task CreateTaskStatusChangedAsync(INotificationService service, JsonElement root, CancellationToken cancellationToken)
    {
        var userId = ReadGuid(root, "ChangedBy") ?? ReadGuid(root, "UpdatedBy");
        if (userId == null) return;

        var taskId = ReadGuid(root, "TaskId")?.ToString();
        var newStatus = ReadString(root, "NewStatus") ?? "Updated";

        await service.CreateAsync(
            userId.Value,
            NotificationType.TaskStatusChanged,
            "Task status changed",
            $"Task status changed to {newStatus}.",
            taskId,
            "task.status.changed",
            cancellationToken);
    }

    private static async Task CreateCommentCreatedAsync(INotificationService service, JsonElement root, CancellationToken cancellationToken)
    {
        var userId = ReadGuid(root, "UserId");
        if (userId == null) return;

        var content = ReadString(root, "Content") ?? "New comment";
        if (content.Length > 80)
        {
            content = content[..80] + "...";
        }

        var commentId = ReadGuid(root, "CommentId")?.ToString();

        await service.CreateAsync(
            userId.Value,
            NotificationType.CommentCreated,
            "Comment added",
            $"Your comment '{content}' was added.",
            commentId,
            "comment.created",
            cancellationToken);
    }

    private static Guid? ReadGuid(JsonElement root, string propertyName)
    {
        if (!root.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        return Guid.TryParse(property.GetString(), out var value) ? value : null;
    }

    private static string? ReadString(JsonElement root, string propertyName)
    {
        return root.TryGetProperty(propertyName, out var property) ? property.GetString() : null;
    }
}