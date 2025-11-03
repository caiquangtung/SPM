using FluentValidation;
using user_service.DTOs;

namespace user_service.Validators;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .RequiredEmail();

        RuleFor(x => x.Password)
            .RequiredPassword();
    }
}

