using user_service.DTOs;

namespace user_service.Services;

public interface IAuthService
{
    Task<VerifyEmailResult> VerifyEmailAsync(string token);
}


