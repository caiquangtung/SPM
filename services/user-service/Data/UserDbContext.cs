using Microsoft.EntityFrameworkCore;
using user_service.Models;

namespace user_service.Data;

public class UserDbContext : DbContext
{
    public UserDbContext(DbContextOptions<UserDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<EmailVerification> EmailVerifications { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Set schema
        modelBuilder.HasDefaultSchema("spm_user");

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(u => u.Id);
            entity.HasIndex(u => u.Email).IsUnique();
            entity.HasIndex(u => u.Role);
            entity.HasIndex(u => u.IsActive);

            entity.Property(u => u.Email)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.PasswordHash)
                .HasMaxLength(255)
                .IsRequired();

            entity.Property(u => u.FullName)
                .HasMaxLength(255);

            entity.Property(u => u.Role)
                .HasColumnName("role")
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.ToTable(t => t.HasCheckConstraint("CK_User_Role", "role IN ('Admin', 'PM', 'Member')"));
        });

        // EmailVerification configuration
        modelBuilder.Entity<EmailVerification>(entity =>
        {
            entity.ToTable("email_verifications");
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.UserId);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasIndex(e => e.ExpiresAt);

            entity.Property(e => e.Token)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(u => u.EmailVerifications)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // RefreshToken configuration
        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("refresh_tokens");
            entity.HasKey(r => r.Id);
            entity.HasIndex(r => r.UserId);
            entity.HasIndex(r => r.Token).IsUnique();
            entity.HasIndex(r => r.ExpiresAt);

            entity.Property(r => r.Token)
                .HasMaxLength(255)
                .IsRequired();

            entity.HasOne(r => r.User)
                .WithMany(u => u.RefreshTokens)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}

