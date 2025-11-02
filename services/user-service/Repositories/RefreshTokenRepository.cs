using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Models;

namespace user_service.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly UserDbContext _context;

    public RefreshTokenRepository(UserDbContext context)
    {
        _context = context;
    }

    // Read operations - Asynchronous (I/O), async/await required
    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }

    // Write operations - Synchronous (in-memory), no async needed
    public Task<RefreshToken> CreateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Add(token);
        return Task.FromResult(token);
    }

    public Task<RefreshToken> UpdateAsync(RefreshToken token)
    {
        _context.RefreshTokens.Update(token);
        return Task.FromResult(token);
    }

    // Delete operation - Needs async for reading entity first, then synchronous remove
    public async Task<bool> DeleteAsync(Guid id)
    {
        var token = await _context.RefreshTokens.FindAsync(id);
        if (token == null)
            return false;

        _context.RefreshTokens.Remove(token); // Synchronous operation
        return true;
    }

    // Revoke operation - Needs async for reading entity first, then synchronous update
    public async Task<bool> RevokeAsync(string token)
    {
        var refreshToken = await GetByTokenAsync(token);
        if (refreshToken == null)
            return false;

        refreshToken.RevokedAt = DateTime.UtcNow;
        await UpdateAsync(refreshToken); // Synchronous operation - only updates context, doesn't save
        return true;
    }
}

