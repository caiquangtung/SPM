using user_service.Models;

namespace user_service.Repositories;

public interface IRefreshTokenRepository
{
    Task<RefreshToken?> GetByTokenAsync(string token);
    Task<RefreshToken> CreateAsync(RefreshToken token);
    Task<RefreshToken> UpdateAsync(RefreshToken token);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> RevokeAsync(string token);
}

