using Confluent.Kafka;
using System.Text.Json;

namespace user_service.Services;

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

    public async Task PublishUserCreatedAsync(Guid userId, string email, string role)
    {
        try
        {
            var message = new
            {
                UserId = userId,
                Email = email,
                Role = role,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("user.created", kafkaMessage);
            _logger.LogInformation("Published user.created event for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish user.created event for user {UserId}", userId);
        }
    }

    public async Task PublishUserUpdatedAsync(Guid userId, string email, string role)
    {
        try
        {
            var message = new
            {
                UserId = userId,
                Email = email,
                Role = role,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("user.updated", kafkaMessage);
            _logger.LogInformation("Published user.updated event for user {UserId}", userId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish user.updated event for user {UserId}", userId);
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

