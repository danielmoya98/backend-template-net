using System.Net;
using BackendTemplate.IntegrationTests.Common;
using FluentAssertions;
using Xunit;

namespace BackendTemplate.IntegrationTests.Controllers;

[Collection("IntegrationTests")]
public class HealthIntegrationTests
{
    private readonly HttpClient _client;

    public HealthIntegrationTests(CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task Get_HealthEndpoint_ReturnsOk()
    {
        // Act
        var response = await _client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Get_ApiHealthEndpoint_ReturnsSuccessApiResponse()
    {
        // Act
        var response = await _client.GetAsync("/api/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("\"success\":true");
        content.Should().Contain("Healthy");
    }
}
