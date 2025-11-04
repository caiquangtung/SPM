using Microsoft.EntityFrameworkCore;
using user_service.DTOs;
using user_service.Models;
using user_service.Repositories;
using user_service.UnitOfWork;

namespace user_service.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IKafkaProducerService kafkaProducer,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<VerifyEmailResult> VerifyEmailAsync(string token)
    {
        var verification = await _emailVerificationRepository.GetByTokenAsync(token);

        if (verification == null)
        {
            return VerifyEmailResult.FailResult("Invalid verification token", "INVALID_TOKEN");
        }

        if (verification.ExpiresAt < DateTime.UtcNow)
        {
            return VerifyEmailResult.FailResult("Verification token has expired", "TOKEN_EXPIRED");
        }

        if (verification.VerifiedAt.HasValue)
        {
            return VerifyEmailResult.FailResult("Email already verified", "EMAIL_ALREADY_VERIFIED");
        }

        if (verification.User == null)
        {
            _logger.LogError("Email verification {Token} has null User reference", token);
            return VerifyEmailResult.FailResult("Invalid verification token", "INVALID_TOKEN");
        }

        // Use transaction to ensure atomicity
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            verification.VerifiedAt = DateTime.UtcNow;
            verification.User.EmailConfirmed = true;
            verification.User.UpdatedAt = DateTime.UtcNow;

            await _emailVerificationRepository.UpdateAsync(verification);
            await _userRepository.UpdateAsync(verification.User);

            // Save all changes in single transaction
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Publish Kafka event (after transaction)
            await _kafkaProducer.PublishUserUpdatedAsync(verification.User.Id, verification.User.Email, verification.User.Role.ToString());

            return VerifyEmailResult.SuccessResult();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error verifying email with token: {Token}", token);
            throw; // Will be handled by GlobalExceptionHandlerMiddleware
        }
    }

    // Registration and Login responsibilities have been moved to specialized services
}

