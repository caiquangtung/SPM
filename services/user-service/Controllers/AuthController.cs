using Microsoft.AspNetCore.Mvc;
using user_service.DTOs;
using user_service.Extensions;
using user_service.Services;

namespace user_service.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IRegistrationService _registrationService;
    private readonly ILoginService _loginService;
    private readonly ITokenManagementService _tokenManagementService;
    private readonly IAuthService _authService; // for verify email

    public AuthController(
        IRegistrationService registrationService,
        ILoginService loginService,
        ITokenManagementService tokenManagementService,
        IAuthService authService)
    {
        _registrationService = registrationService;
        _loginService = loginService;
        _tokenManagementService = tokenManagementService;
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _registrationService.RegisterAsync(request);

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
        var response = await _loginService.LoginAsync(request);
        return this.OkResponse(response, "Login successful");
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request)
    {
        var response = await _tokenManagementService.RefreshTokenAsync(request.RefreshToken);
        return this.OkResponse(response, "Token refreshed successfully");
    }
}

