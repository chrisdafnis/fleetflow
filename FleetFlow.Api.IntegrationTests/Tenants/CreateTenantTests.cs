using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FleetFlow.Api.IntegrationTests.Authentication;
using FleetFlow.Api.IntegrationTests.Infrastructure;
using AuthFactory = FleetFlow.Api.IntegrationTests.Authentication.TestJwtTokenFactory;

namespace FleetFlow.Api.IntegrationTests.Tenants;

public sealed class CreateTenantTests
{
    [Fact]
    public async Task CreateTenant_WithoutToken_ReturnsUnauthorized()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync(
            "/api/tenants",
            new { Name = "Test Tenant" });

        Assert.Equal(
            HttpStatusCode.Unauthorized,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithNormalUser_ReturnsForbidden()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        AuthenticateClient(client, isAdministrator: false);

        var response = await client.PostAsJsonAsync(
            "/api/tenants",
            new { Name = "Test Tenant" });

        Assert.Equal(
            HttpStatusCode.Forbidden,
            response.StatusCode);
    }

    [Fact]
    public async Task CreateTenant_WithAdministrator_ReturnsCreated()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        AuthenticateClient(client, isAdministrator: true);

        var response = await client.PostAsJsonAsync(
            "/api/tenants",
            new { Name = "Test Tenant" });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        using var json = JsonDocument.Parse(
            await response.Content.ReadAsStringAsync());

        var tenantId = json.RootElement
            .GetProperty("id")
            .GetGuid();

        Assert.NotEqual(Guid.Empty, tenantId);
    }

    private static void AuthenticateClient(
        HttpClient client,
        bool isAdministrator)
    {
        var token = AuthFactory.CreateToken(Guid.NewGuid(), isAdministrator: isAdministrator);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
    }
}
