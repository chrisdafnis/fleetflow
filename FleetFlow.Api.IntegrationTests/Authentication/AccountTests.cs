using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using FleetFlow.Api.Authentication;
using FleetFlow.Api.Controllers;
using FleetFlow.Api.IntegrationTests.Infrastructure;
using FleetFlow.Infrastructure.Authentication;
using FleetFlow.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace FleetFlow.Api.IntegrationTests.Authentication;

public sealed class AccountTests
{
    private const string Password = "FleetFlow-Test-Password1!";
    private const string Email = "owner@example.com";

    [Fact]
    public async Task RegisterThenLogin_CanUseOwnTenantButCannotAccessAnotherTenant()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();
        var account = await Register(client);

        var login = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "OWNER@EXAMPLE.COM", Password });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        Assert.Equal("no-store", login.Headers.CacheControl?.ToString());
        var token = await login.Content.ReadFromJsonAsync<AccessTokenResponse>();
        Assert.NotNull(token);
        Assert.Equal(account.UserId, token.UserId);
        Assert.Equal(account.TenantId, token.TenantId);
        Assert.Equal("Bearer", token.TokenType);
        Assert.InRange(token.ExpiresAtUtc, DateTime.UtcNow.AddMinutes(29),
            DateTime.UtcNow.AddMinutes(31));
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        Assert.Equal(account.UserId.ToString(), jwt.Subject);
        Assert.Equal(account.TenantId.ToString(),
            jwt.Claims.Single(c => c.Type == "tenant_id").Value);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token.AccessToken);
        Assert.Equal(HttpStatusCode.OK,
            (await client.GetAsync("/api/auth/check")).StatusCode);

        var ownJob = await client.PostAsJsonAsync(
            $"/api/tenants/{account.TenantId}/jobs",
            new { Reference = "FIRST-001", Description = "First delivery" });
        Assert.Equal(HttpStatusCode.Created, ownJob.StatusCode);
        var otherTenant = await client.PostAsJsonAsync(
            $"/api/tenants/{Guid.NewGuid()}/jobs",
            new { Reference = "OTHER-001", Description = "Not permitted" });
        Assert.Equal(HttpStatusCode.Forbidden, otherTenant.StatusCode);

        // A newly registered user must not gain platform administrator privileges.
        Assert.Equal(HttpStatusCode.Forbidden,
            (await client.PostAsJsonAsync("/api/tenants",
                new { Name = "Another company" })).StatusCode);

        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FleetFlowDbContext>();
        var user = await db.Users.SingleAsync();
        Assert.NotEqual(Password, user.PasswordHash);
        Assert.False(string.IsNullOrWhiteSpace(user.PasswordHash));
    }

    [Fact]
    public async Task DuplicateEmail_DoesNotCreateAnExtraTenant()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();
        await Register(client);
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { CompanyName = "Duplicate", Email = "OWNER@EXAMPLE.COM", Password });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FleetFlowDbContext>();
        Assert.Equal(1, await db.Tenants.CountAsync());
        Assert.Equal(1, await db.Users.CountAsync());
    }

    [Theory]
    [InlineData("short")]
    [InlineData("alllowercasepassword")]
    public async Task WeakPassword_DoesNotPersistUserOrTenant(string password)
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { CompanyName = "Acme", Email, Password = password });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FleetFlowDbContext>();
        Assert.Equal(0, await db.Users.CountAsync());
        Assert.Equal(0, await db.Tenants.CountAsync());
    }

    [Theory]
    [InlineData("", "owner@example.com")]
    [InlineData("Acme", "not-an-email")]
    public async Task InvalidRegistration_ReturnsBadRequest(string company, string email)
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { CompanyName = company, Email = email, Password });
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task WrongPasswordAndUnknownEmail_ReturnSameUnauthorizedResponse()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();
        await Register(client);
        var wrong = await client.PostAsJsonAsync("/api/auth/login",
            new { Email, Password = "Incorrect-password1!" });
        var missing = await client.PostAsJsonAsync("/api/auth/login",
            new { Email = "missing@example.com", Password });
        Assert.Equal(HttpStatusCode.Unauthorized, wrong.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, missing.StatusCode);
        var wrongBody = await wrong.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        var missingBody = await missing.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        Assert.Equal(wrongBody!.Title, missingBody!.Title);
    }

    [Fact]
    public async Task FiveFailedLogins_LockAccountEvenWithCorrectPassword()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();
        await Register(client);
        for (var attempt = 0; attempt < 5; attempt++)
        {
            var response = await client.PostAsJsonAsync("/api/auth/login",
                new { Email, Password = "Incorrect-password1!" });
            Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
        }

        var locked = await client.PostAsJsonAsync("/api/auth/login", new { Email, Password });
        Assert.Equal(HttpStatusCode.Unauthorized, locked.StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<FleetFlowDbContext>();
        var user = await db.Users.SingleAsync();
        Assert.True(user.LockoutEnd > DateTimeOffset.UtcNow);
    }

    [Fact]
    public async Task Login_IgnoresClientSuppliedTenantAndRole()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();
        var account = await Register(client);
        var login = await client.PostAsJsonAsync("/api/auth/login", new
        {
            Email, Password, TenantId = Guid.NewGuid(), Role = "Administrator"
        });
        Assert.Equal(HttpStatusCode.OK, login.StatusCode);
        var token = await login.Content.ReadFromJsonAsync<AccessTokenResponse>();
        Assert.Equal(account.TenantId, token!.TenantId);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token.AccessToken);
        Assert.DoesNotContain(jwt.Claims, claim => claim.Value == "Administrator");
    }

    private static async Task<RegistrationResponse> Register(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register", new
        {
            CompanyName = "Acme Logistics", Email, Password
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var account = await response.Content.ReadFromJsonAsync<RegistrationResponse>();
        Assert.NotNull(account);
        return account;
    }
}
