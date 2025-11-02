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
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public AuthService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IKafkaProducerService kafkaProducer,
        IConfiguration configuration,
        ILogger<AuthService> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _kafkaProducer = kafkaProducer;
        _configuration = configuration;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return RegisterResult.FailResult("Password must be at least 6 characters", "INVALID_PASSWORD");
        }

        // Check if user already exists
        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            return RegisterResult.FailResult("Email already exists", "EMAIL_EXISTS");
        }

        // Use transaction to ensure atomicity
        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            // Create user
            var user = new User
            {
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = _passwordService.HashPassword(request.Password),
                FullName = request.FullName,
                Role = "Member",
                EmailConfirmed = false,
                IsActive = true
            };

            await _userRepository.CreateAsync(user);

            // Generate email verification token
            var verificationToken = _tokenService.GenerateRefreshToken();
            var emailVerification = new EmailVerification
            {
                UserId = user.Id,
                Token = verificationToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _emailVerificationRepository.CreateAsync(emailVerification);

            // Save all changes in single transaction
            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            // Publish Kafka event (after transaction to avoid rollback issues)
            await _kafkaProducer.PublishUserCreatedAsync(user.Id, user.Email, user.Role);

            // TODO: Send verification email with token
            _logger.LogInformation("User registered: {Email}, Verification token: {Token}", user.Email, verificationToken);

            return RegisterResult.SuccessResult(user.Id, verificationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error registering user with email: {Email}", request.Email);
            throw; // Will be handled by GlobalExceptionHandlerMiddleware
        }
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
            await _kafkaProducer.PublishUserUpdatedAsync(verification.User.Id, verification.User.Email, verification.User.Role);

            return VerifyEmailResult.SuccessResult();
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error verifying email with token: {Token}", token);
            throw; // Will be handled by GlobalExceptionHandlerMiddleware
        }
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);

        if (user == null || !_passwordService.VerifyPassword(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password");
        }

        if (!user.IsActive)
        {
            throw new UnauthorizedAccessException("Account is deactivated");
        }

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        user.UpdatedAt = DateTime.UtcNow;
        await _userRepository.UpdateAsync(user);

        // Generate tokens
        var accessToken = _tokenService.GenerateAccessToken(user);
        var refreshTokenValue = _tokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshTokenExpirationDays = int.Parse(_configuration["JWT:RefreshTokenExpirationDays"] ?? "7");
        var refreshToken = new RefreshToken
        {
            UserId = user.Id,
            Token = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddDays(refreshTokenExpirationDays)
        };

        await _refreshTokenRepository.CreateAsync(refreshToken);

        // Save all changes in single transaction
        await _unitOfWork.SaveChangesAsync();

        var jwtSettings = _configuration.GetSection("JWT");
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshTokenValue,
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role
            }
        };
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var validationResult = await _tokenService.ValidateRefreshTokenAsync(refreshToken);

        if (!validationResult.IsValid || validationResult.User == null)
        {
            throw new UnauthorizedAccessException(validationResult.Error ?? "Invalid refresh token");
        }

        var user = validationResult.User;

        // Generate new access token
        var accessToken = _tokenService.GenerateAccessToken(user);

        var jwtSettings = _configuration.GetSection("JWT");
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken, // Reuse same refresh token
            ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
            User = new UserDto
            {
                Id = user.Id,
                Email = user.Email,
                EmailConfirmed = user.EmailConfirmed,
                FullName = user.FullName,
                AvatarUrl = user.AvatarUrl,
                Role = user.Role
            }
        };
    }
}

