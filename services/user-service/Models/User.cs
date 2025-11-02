namespace user_service.Models;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    public string PasswordHash { get; set; } = string.Empty;
    public string? FullName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Role { get; set; } = "Member"; // Admin, PM, Member
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public ICollection<EmailVerification> EmailVerifications { get; set; } = new List<EmailVerification>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
}

