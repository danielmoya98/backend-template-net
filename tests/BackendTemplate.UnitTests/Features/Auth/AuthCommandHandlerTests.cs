using BackendTemplate.Application.Common.Interfaces;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.Application.Features.Auth.Commands.ChangePassword;
using BackendTemplate.Application.Features.Auth.Commands.Login;
using BackendTemplate.Application.Features.Auth.Commands.RefreshToken;
using BackendTemplate.Application.Features.Auth.Commands.Register;
using BackendTemplate.Application.Features.Auth.Commands.RevokeToken;
using BackendTemplate.Domain.Common;
using FluentAssertions;
using Moq;
using Xunit;

namespace BackendTemplate.UnitTests.Features.Auth;

public class AuthCommandHandlerTests
{
    private readonly Mock<IIdentityService> _mockIdentityService;
    private readonly Mock<ICurrentUserService> _mockCurrentUserService;

    public AuthCommandHandlerTests()
    {
        _mockIdentityService = new Mock<IIdentityService>();
        _mockCurrentUserService = new Mock<ICurrentUserService>();
    }

    [Fact]
    public async Task LoginCommandHandler_ShouldCallIdentityService()
    {
        // Arrange
        var expectedResponse = new AuthResponse(
            "user-1", "user@test.com", "John", "Doe",
            "access-token", "refresh-token", DateTime.UtcNow.AddDays(7), new List<string> { "User" });

        _mockIdentityService.Setup(s => s.LoginAsync(It.IsAny<LoginRequest>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Result<AuthResponse>.Success(expectedResponse));

        var handler = new LoginCommandHandler(_mockIdentityService.Object);
        var command = new LoginCommand("user@test.com", "Password123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Email.Should().Be("user@test.com");
        result.Value.AccessToken.Should().Be("access-token");
    }

    [Fact]
    public async Task RegisterCommandHandler_ShouldCallIdentityService()
    {
        // Arrange
        _mockIdentityService.Setup(s => s.RegisterAsync(It.IsAny<RegisterRequest>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Result<string>.Success("new-user-id"));

        var handler = new RegisterCommandHandler(_mockIdentityService.Object);
        var command = new RegisterCommand("new@test.com", "Password123!", "Jane", "Doe");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("new-user-id");
    }

    [Fact]
    public async Task RefreshTokenCommandHandler_ShouldCallIdentityService()
    {
        // Arrange
        var expectedResponse = new AuthResponse(
            "user-1", "user@test.com", "John", "Doe",
            "new-access-token", "new-refresh-token", DateTime.UtcNow.AddDays(7), new List<string> { "User" });

        _mockIdentityService.Setup(s => s.RefreshTokenAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Result<AuthResponse>.Success(expectedResponse));

        var handler = new RefreshTokenCommandHandler(_mockIdentityService.Object);
        var command = new RefreshTokenCommand("old-access-token", "old-refresh-token");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.AccessToken.Should().Be("new-access-token");
    }

    [Fact]
    public async Task RevokeTokenCommandHandler_ShouldCallIdentityService()
    {
        // Arrange
        _mockIdentityService.Setup(s => s.RevokeTokenAsync(It.IsAny<string>(), It.IsAny<string?>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Result.Success());

        var handler = new RevokeTokenCommandHandler(_mockIdentityService.Object);
        var command = new RevokeTokenCommand("token-to-revoke");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task ChangePasswordCommandHandler_ShouldCallIdentityService()
    {
        // Arrange
        _mockCurrentUserService.Setup(c => c.UserId).Returns("user-1");
        _mockIdentityService.Setup(s => s.ChangePasswordAsync("user-1", It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                            .ReturnsAsync(Result.Success());

        var handler = new ChangePasswordCommandHandler(_mockIdentityService.Object, _mockCurrentUserService.Object);
        var command = new ChangePasswordCommand("OldPass123!", "NewPass123!");

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
    }
}
