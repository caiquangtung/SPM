using user_service.Models;

namespace user_service.Services;

public interface ITokenService
{
    string GenerateAccessToken(User user);
    string GenerateRefreshToken();
    Task<TokenValidationResult> ValidateRefreshTokenAsync(string token);
}

public class TokenValidationResult
{
    public bool IsValid { get; set; }
    public User? User { get; set; }
    public string? Error { get; set; }
}

