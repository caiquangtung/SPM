using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using user_service.Models;
using user_service.Services;

namespace user_service.Data;

public static class SeedData
{
    // Fixed GUID for default admin user (used across all services for FK references)
    private static readonly Guid AdminUserId = new Guid("11111111-1111-1111-1111-111111111111");

    public static void EnsureSeedData(UserDbContext context, IPasswordService passwordService, ILogger logger)
    {
        // If there are already users, do nothing
        if (context.Users.Any())
        {
            return;
        }

        logger.LogInformation("No users found in database. Seeding default admin user...");

        var adminEmail = "admin@spm.local";
        var adminPassword = "Admin123!";

        var adminUser = new User
        {
            Id = AdminUserId, // Fixed GUID for cross-service references
            Email = adminEmail.ToLowerInvariant(),
            EmailConfirmed = true,
            FullName = "Default Admin",
            Role = UserRole.Admin,
            IsActive = true,
            PasswordHash = passwordService.HashPassword(adminPassword),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Users.Add(adminUser);
        context.SaveChanges();

        logger.LogInformation(
            "Seeded default admin user (ID: {UserId}) with email {Email}. Use password '{Password}' for initial login (Development only).",
            AdminUserId,
            adminEmail,
            adminPassword);
    }
}

