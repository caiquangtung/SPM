namespace file_service.Services.Interfaces;

public interface IKafkaProducerService
{
    Task PublishFileUploadedAsync(Guid fileId, Guid userId, string fileName, long fileSize);
}

