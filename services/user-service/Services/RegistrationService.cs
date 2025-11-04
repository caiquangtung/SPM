using user_service.DTOs;
using user_service.Models;
using user_service.Repositories;
using user_service.UnitOfWork;

namespace user_service.Services;

public class RegistrationService : IRegistrationService
{
    private readonly IUserRepository _userRepository;
    private readonly IEmailVerificationRepository _emailVerificationRepository;
    private readonly IPasswordService _passwordService;
    private readonly ITokenService _tokenService;
    private readonly IKafkaProducerService _kafkaProducer;
    private readonly ILogger<RegistrationService> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public RegistrationService(
        IUserRepository userRepository,
        IEmailVerificationRepository emailVerificationRepository,
        IPasswordService passwordService,
        ITokenService tokenService,
        IKafkaProducerService kafkaProducer,
        ILogger<RegistrationService> logger,
        IUnitOfWork unitOfWork)
    {
        _userRepository = userRepository;
        _emailVerificationRepository = emailVerificationRepository;
        _passwordService = passwordService;
        _tokenService = tokenService;
        _kafkaProducer = kafkaProducer;
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
        {
            return RegisterResult.FailResult("Password must be at least 6 characters", "INVALID_PASSWORD");
        }

        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            return RegisterResult.FailResult("Email already exists", "EMAIL_EXISTS");
        }

        using var transaction = await _unitOfWork.BeginTransactionAsync();
        try
        {
            var user = new User
            {
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = _passwordService.HashPassword(request.Password),
                FullName = request.FullName,
                Role = UserRole.Member,
                EmailConfirmed = false,
                IsActive = true
            };

            await _userRepository.CreateAsync(user);

            var verificationToken = _tokenService.GenerateRefreshToken();
            var emailVerification = new EmailVerification
            {
                UserId = user.Id,
                Token = verificationToken,
                ExpiresAt = DateTime.UtcNow.AddDays(7)
            };

            await _emailVerificationRepository.CreateAsync(emailVerification);

            await _unitOfWork.SaveChangesAsync();
            await _unitOfWork.CommitTransactionAsync();

            await _kafkaProducer.PublishUserCreatedAsync(user.Id, user.Email, user.Role.ToString());

            _logger.LogInformation("User registered: {Email}", user.Email);

            return RegisterResult.SuccessResult(user.Id, verificationToken);
        }
        catch (Exception ex)
        {
            await _unitOfWork.RollbackTransactionAsync();
            _logger.LogError(ex, "Error registering user with email: {Email}", request.Email);
            throw;
        }
    }
}


