using System.Net;
using System.Net.Http.Json;
using BackendTemplate.Api.Common.Responses;
using BackendTemplate.Application.Common.Models;
using BackendTemplate.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace BackendTemplate.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class AuthIntegrationTests
{
    private readonly HttpClient _client;

    public AuthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Register_ShouldReturnOk_WhenRequestIsValid()
    {
        // Arrange
        var uniqueEmail = $"user_{Guid.NewGuid():N}@test.com";
        var request = new RegisterRequest(uniqueEmail, "ValidPassword123!", "Jane", "Doe");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var result = await response.Content.ReadFromJsonAsync<ApiResponse<string>>();
        result.Should().NotBeNull();
        result!.Success.Should().BeTrue();
        result.Data.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenCredentialsAreInvalid()
    {
        // Arrange
        var request = new LoginRequest("nonexistent@test.com", "WrongPassword123!");

        // Act
        var response = await _client.PostAsJsonAsync("/api/auth/login", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        var error = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
        error.Should().NotBeNull();
        error!.Success.Should().BeFalse();
    }

    [Fact]
    public async Task Students_ShouldReturnUnauthorized_WhenNoBearerTokenProvided()
    {
        // Act
        var response = await _client.GetAsync("/api/students");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }
}
