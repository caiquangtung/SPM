using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using user_service.Models;
using user_service.Repositories;

namespace user_service.Services;

public class TokenService : ITokenService
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public TokenService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
    {
        _configuration = configuration;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public string GenerateAccessToken(User user)
    {
        var jwtSettings = _configuration.GetSection("JWT");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey is not configured");
        var issuer = jwtSettings["Issuer"] ?? "spm-api-gateway";
        var audience = jwtSettings["Audience"] ?? "spm-services";
        var expirationMinutes = int.Parse(jwtSettings["AccessTokenExpirationMinutes"] ?? "15");

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(expirationMinutes),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public string GenerateRefreshToken()
    {
        var randomNumber = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomNumber);
        return Convert.ToBase64String(randomNumber);
    }

    public async Task<TokenValidationResult> ValidateRefreshTokenAsync(string token)
    {
        var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

        if (refreshToken == null)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                Error = "Invalid refresh token"
            };
        }

        if (refreshToken.IsExpired)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                Error = "Refresh token has expired"
            };
        }

        if (refreshToken.IsRevoked)
        {
            return new TokenValidationResult
            {
                IsValid = false,
                Error = "Refresh token has been revoked"
            };
        }

        return new TokenValidationResult
        {
            IsValid = true,
            User = refreshToken.User
        };
    }
}

