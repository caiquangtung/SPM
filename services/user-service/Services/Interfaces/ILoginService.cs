using user_service.DTOs;

namespace user_service.Services;

public interface ILoginService
{
    Task<AuthResponse> LoginAsync(LoginRequest request);
}


