using FluentValidation;
using user_service.DTOs;

namespace user_service.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .RequiredEmail()
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .RequiredPassword(6)
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");

        RuleFor(x => x.FullName)
            .MaxLengthIfNotNull(255, "Full name")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));
    }
}

