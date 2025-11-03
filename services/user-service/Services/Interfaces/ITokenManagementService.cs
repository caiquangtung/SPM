using user_service.DTOs;

namespace user_service.Services;

public interface ITokenManagementService
{
    Task<AuthResponse> RefreshTokenAsync(string refreshToken);
}


