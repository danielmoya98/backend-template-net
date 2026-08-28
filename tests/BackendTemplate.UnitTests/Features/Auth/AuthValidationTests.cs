using BackendTemplate.Application.Features.Auth.Commands.ChangePassword;
using BackendTemplate.Application.Features.Auth.Commands.Login;
using BackendTemplate.Application.Features.Auth.Commands.Register;
using FluentAssertions;
using Xunit;

namespace BackendTemplate.UnitTests.Features.Auth;

public class AuthValidationTests
{
    [Theory]
    [InlineData("", "password123", "Email is required.")]
    [InlineData("invalid-email", "password123", "A valid email address is required.")]
    [InlineData("test@example.com", "", "Password is required.")]
    public void LoginCommandValidator_ShouldFail_WhenInputIsInvalid(string email, string password, string expectedError)
    {
        // Arrange
        var validator = new LoginCommandValidator();
        var command = new LoginCommand(email, password);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == expectedError);
    }

    [Theory]
    [InlineData("", "Password123!", "John", "Doe")]
    [InlineData("john@example.com", "123", "John", "Doe")] // Too short
    [InlineData("john@example.com", "Password123!", "", "Doe")] // Empty first name
    [InlineData("john@example.com", "Password123!", "John", "")] // Empty last name
    public void RegisterCommandValidator_ShouldFail_WhenRequiredFieldsAreInvalid(
        string email, string password, string firstName, string lastName)
    {
        // Arrange
        var validator = new RegisterCommandValidator();
        var command = new RegisterCommand(email, password, firstName, lastName);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void ChangePasswordCommandValidator_ShouldPass_WhenInputsAreValid()
    {
        // Arrange
        var validator = new ChangePasswordCommandValidator();
        var command = new ChangePasswordCommand("OldPassword123!", "NewPassword123!");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }
}
