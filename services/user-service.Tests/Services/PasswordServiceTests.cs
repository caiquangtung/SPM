using FluentAssertions;
using Xunit;
using user_service.Services;

namespace user_service.Tests.Services;

public class PasswordServiceTests
{
    [Fact]
    public void Hash_And_Verify_Should_Succeed()
    {
        var svc = new PasswordService();
        var hash = svc.HashPassword("secret123");
        svc.VerifyPassword("secret123", hash).Should().BeTrue();
    }

    [Fact]
    public void Verify_Should_Fail_For_Wrong_Password()
    {
        var svc = new PasswordService();
        var hash = svc.HashPassword("secret123");
        svc.VerifyPassword("wrong", hash).Should().BeFalse();
    }
}


