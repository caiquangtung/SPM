using Microsoft.AspNetCore.Mvc;
using user_service.DTOs;
using user_service.Extensions;
using user_service.Services;

namespace user_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);

        if (!result.Success)
        {
            return this.BadRequestResponse(result.ErrorMessage!, result.ErrorCode);
        }

        return this.OkResponse(
            new { userId = result.UserId },
            "User registered successfully. Please check your email to verify your account."
        );
    }

    [HttpPost("verify-email")]
    public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        var result = await _authService.VerifyEmailAsync(request.Token);

        if (!result.Success)
        {
            return this.BadRequestResponse(result.ErrorMessage!, result.ErrorCode);
        }

        return this.OkResponse("Email verified successfully");
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var response = await _authService.LoginAsync(request);
        return this.OkResponse(response, "Login successful");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _authService.RefreshTokenAsync(request.RefreshToken);
        return this.OkResponse(response, "Token refreshed successfully");
    }
}

