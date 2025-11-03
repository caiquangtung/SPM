using user_service.DTOs;

namespace user_service.Services;

public class TokenManagementService : ITokenManagementService
{
    private readonly ITokenService _tokenService;
    private readonly IConfiguration _configuration;

    public TokenManagementService(
        ITokenService tokenService,
        IConfiguration configuration)
    {
        _tokenService = tokenService;
        _configuration = configuration;
    }

    public async Task<AuthResponse> RefreshTokenAsync(string refreshToken)
    {
        var validationResult = await _tokenService.ValidateRefreshTokenAsync(refreshToken);

        if (!validationResult.IsValid || validationResult.User == null)
        {
            throw new UnauthorizedAccessException(validationResult.Error ?? "Invalid refresh token");
        }

        var user = validationResult.User;
        var accessToken = _tokenService.GenerateAccessToken(user);

        var jwtSettings = _configuration.GetSection("JWT");
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
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


