
using System.Net;
using System.Net.Http.Json;
using FleetFlow.Api.IntegrationTests.Infrastructure;

namespace FleetFlow.Api.IntegrationTests.Tenants;

public sealed class CreateTenantTests
{
    [Fact]
    public async Task CreateTenant_WithValidName_ReturnsCreated()
    {
        // Arrange
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        var request = new
        {
            Name = "Acme Logistics"
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/tenants",
            request);

        // Assert
        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<CreateTenantResponse>();

        Assert.NotNull(result);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal("Acme Logistics", result.Name);
    }

    private sealed record CreateTenantResponse(
        Guid Id,
        string Name);
}
