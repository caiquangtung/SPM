using FluentValidation;
using user_service.DTOs;

namespace user_service.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format")
            .MaximumLength(255).WithMessage("Email must not exceed 255 characters");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters")
            .MaximumLength(100).WithMessage("Password must not exceed 100 characters");

        RuleFor(x => x.FullName)
            .MaximumLength(255).WithMessage("Full name must not exceed 255 characters")
            .When(x => !string.IsNullOrWhiteSpace(x.FullName));
    }
}

