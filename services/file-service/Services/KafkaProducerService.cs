using Confluent.Kafka;
using System.Text.Json;
using file_service.Services.Interfaces;

namespace file_service.Services;

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

    public async Task PublishFileUploadedAsync(Guid fileId, Guid userId, string fileName, long fileSize)
    {
        try
        {
            var message = new
            {
                FileId = fileId,
                UserId = userId,
                FileName = fileName,
                FileSize = fileSize,
                Timestamp = DateTime.UtcNow
            };

            var json = JsonSerializer.Serialize(message);
            var kafkaMessage = new Message<Null, string> { Value = json };

            await _producer.ProduceAsync("file.uploaded", kafkaMessage);
            _logger.LogInformation("Published file.uploaded event for file {FileId}", fileId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to publish file.uploaded event for file {FileId}", fileId);
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

