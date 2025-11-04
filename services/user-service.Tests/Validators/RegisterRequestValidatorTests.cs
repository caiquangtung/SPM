using FluentAssertions;
using Xunit;
using user_service.DTOs;
using user_service.Validators;

namespace user_service.Tests.Validators;

public class RegisterRequestValidatorTests
{
    [Fact]
    public void Should_Fail_When_Email_Invalid_Or_Empty()
    {
        var validator = new RegisterRequestValidator();

        var invalid = validator.Validate(new RegisterRequest { Email = "not-an-email", Password = "123456" });
        invalid.IsValid.Should().BeFalse();

        var empty = validator.Validate(new RegisterRequest { Email = "", Password = "123456" });
        empty.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Fail_When_Password_TooShort()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest { Email = "user@example.com", Password = "123" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Pass_With_Valid_Data()
    {
        var validator = new RegisterRequestValidator();
        var result = validator.Validate(new RegisterRequest { Email = "user@example.com", Password = "123456", FullName = "User" });
        result.IsValid.Should().BeTrue();
    }
}


