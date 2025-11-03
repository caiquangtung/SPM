using FluentValidation;
using user_service.DTOs;

namespace user_service.Validators;

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken)
            .RequiredNonEmpty("Refresh token");
    }
}

