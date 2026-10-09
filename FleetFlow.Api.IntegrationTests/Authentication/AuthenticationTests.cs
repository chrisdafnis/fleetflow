
using System.Net;
using System.Net.Http.Headers;
using FleetFlow.Api.IntegrationTests.Infrastructure;

namespace FleetFlow.Api.IntegrationTests.Authentication;

public sealed class AuthenticationTests
{
    [Fact]
    public async Task ProtectedEndpoint_WithoutToken_ReturnsUnauthorized()
    {
        // Arrange
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        // Act
        var response = await client.GetAsync("/api/auth/check");

        // Assert
        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task ProtectedEndpoint_WithValidToken_ReturnsOk()
    {
        // Arrange
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        var userId = Guid.NewGuid();

        var token = TestJwtTokenFactory.CreateToken(userId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);

        // Act
        var response = await client.GetAsync("/api/auth/check");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);
    }
}

