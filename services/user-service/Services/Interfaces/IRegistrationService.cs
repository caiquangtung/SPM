using user_service.DTOs;

namespace user_service.Services;

public interface IRegistrationService
{
    Task<RegisterResult> RegisterAsync(RegisterRequest request);
}


