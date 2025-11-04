using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using user_service.Models;
using user_service.Repositories;
using user_service.Services;
using user_service.UnitOfWork;

namespace user_service.Tests.Services;

public class AuthServiceVerifyEmailTests
{
    [Fact]
    public async Task VerifyEmailAsync_Should_Return_Fail_When_Token_Not_Found()
    {
        var emailRepo = new Mock<IEmailVerificationRepository>();
        emailRepo.Setup(r => r.GetByTokenAsync(It.IsAny<string>()))
            .ReturnsAsync((EmailVerification?)null);

        var userRepo = new Mock<IUserRepository>();
        var kafka = new Mock<IKafkaProducerService>();
        var logger = new Mock<ILogger<AuthService>>();
        var uow = new Mock<IUnitOfWork>();
        var tx = new Mock<Microsoft.EntityFrameworkCore.Storage.IDbContextTransaction>();
        uow.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(tx.Object);

        var svc = new AuthService(
            userRepo.Object,
            emailRepo.Object,
            kafka.Object,
            logger.Object,
            uow.Object);

        var result = await svc.VerifyEmailAsync("invalid-token");
        result.Success.Should().BeFalse();
    }
}


