using user_service.Models;

namespace user_service.Repositories;

public interface IEmailVerificationRepository
{
    Task<EmailVerification?> GetByTokenAsync(string token);
    Task<EmailVerification> CreateAsync(EmailVerification verification);
    Task<EmailVerification> UpdateAsync(EmailVerification verification);
    Task<bool> DeleteAsync(Guid id);
}

