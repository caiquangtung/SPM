using Microsoft.EntityFrameworkCore;
using user_service.Data;
using user_service.Models;

namespace user_service.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _context;

    public UserRepository(UserDbContext context)
    {
        _context = context;
    }

    // Read operations - Asynchronous (I/O), async/await required
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email.ToLowerInvariant());
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Users.ToListAsync();
    }

    // Write operations - Synchronous (in-memory), no async needed
    public Task<User> CreateAsync(User user)
    {
        _context.Users.Add(user);
        return Task.FromResult(user);
    }

    public Task<User> UpdateAsync(User user)
    {
        user.UpdatedAt = DateTime.UtcNow;
        _context.Users.Update(user);
        return Task.FromResult(user);
    }

    // Delete operation - Needs async for reading entity first, then synchronous remove
    public async Task<bool> DeleteAsync(Guid id)
    {
        var user = await GetByIdAsync(id);
        if (user == null)
            return false;

        _context.Users.Remove(user); // Synchronous operation
        return true;
    }
}

