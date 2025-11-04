using FluentAssertions;
using Xunit;
using user_service.DTOs;
using user_service.Validators;

namespace user_service.Tests.Validators;

public class LoginRequestValidatorTests
{
    [Fact]
    public void Should_Fail_When_Email_Invalid_Or_Empty()
    {
        var validator = new LoginRequestValidator();

        var invalid = validator.Validate(new LoginRequest { Email = "not-email", Password = "p" });
        invalid.IsValid.Should().BeFalse();

        var empty = validator.Validate(new LoginRequest { Email = "", Password = "p" });
        empty.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Fail_When_Password_Empty()
    {
        var validator = new LoginRequestValidator();
        var result = validator.Validate(new LoginRequest { Email = "user@example.com", Password = "" });
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Should_Pass_With_Valid_Data()
    {
        var validator = new LoginRequestValidator();
        var result = validator.Validate(new LoginRequest { Email = "user@example.com", Password = "123456" });
        result.IsValid.Should().BeTrue();
    }
}


