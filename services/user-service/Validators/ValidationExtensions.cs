using FluentValidation;

namespace user_service.Validators;

public static class ValidationExtensions
{
    public static IRuleBuilderOptions<T, string> RequiredEmail<T>(this IRuleBuilder<T, string> rule)
    {
        return rule
            .NotEmpty().WithMessage("Email is required")
            .EmailAddress().WithMessage("Invalid email format");
    }

    public static IRuleBuilderOptions<T, string> RequiredPassword<T>(this IRuleBuilder<T, string> rule, int minLength = 0)
    {
        var builder = rule
            .NotEmpty().WithMessage("Password is required");

        if (minLength > 0)
        {
            builder = builder.MinimumLength(minLength)
                .WithMessage($"Password must be at least {minLength} characters");
        }

        return builder;
    }

    public static IRuleBuilderOptions<T, string> RequiredNonEmpty<T>(this IRuleBuilder<T, string> rule, string fieldDisplay)
    {
        return rule.NotEmpty().WithMessage($"{fieldDisplay} is required");
    }

    public static IRuleBuilderOptions<T, string?> MaxLengthIfNotNull<T>(this IRuleBuilder<T, string?> rule, int max, string fieldDisplay)
    {
        return rule.MaximumLength(max).WithMessage($"{fieldDisplay} must be at most {max} characters");
    }
}


