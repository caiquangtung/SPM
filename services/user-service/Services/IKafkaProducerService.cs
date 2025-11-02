namespace user_service.Services;

public interface IKafkaProducerService
{
    Task PublishUserCreatedAsync(Guid userId, string email, string role);
    Task PublishUserUpdatedAsync(Guid userId, string email, string role);
}

