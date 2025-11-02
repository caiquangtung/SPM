using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Models;

namespace user_service.Repositories;

public class EmailVerificationRepository : IEmailVerificationRepository
{
    private readonly UserDbContext _context;

    public EmailVerificationRepository(UserDbContext context)
    {
        _context = context;
    }

    // Read operations - Asynchronous (I/O), async/await required
    public async Task<EmailVerification?> GetByTokenAsync(string token)
    {
        return await _context.EmailVerifications
            .Include(e => e.User)
            .FirstOrDefaultAsync(e => e.Token == token);
    }

    // Write operations - Synchronous (in-memory), no async needed
    public Task<EmailVerification> CreateAsync(EmailVerification verification)
    {
        _context.EmailVerifications.Add(verification);
        return Task.FromResult(verification);
    }

    public Task<EmailVerification> UpdateAsync(EmailVerification verification)
    {
        _context.EmailVerifications.Update(verification);
        return Task.FromResult(verification);
    }

    // Delete operation - Needs async for reading entity first, then synchronous remove
    public async Task<bool> DeleteAsync(Guid id)
    {
        var verification = await _context.EmailVerifications.FindAsync(id);
        if (verification == null)
            return false;

        _context.EmailVerifications.Remove(verification); // Synchronous operation
        return true;
    }
}

