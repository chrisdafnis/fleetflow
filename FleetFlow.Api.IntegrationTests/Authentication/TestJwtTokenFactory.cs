
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace FleetFlow.Api.IntegrationTests.Authentication;

public static class TestJwtTokenFactory
{
    public const string Issuer = "FleetFlow.IntegrationTests";
    public const string Audience = "FleetFlow.TestClient";

    // Test-only signing key. Never use in production.
    public const string Key =
        "FleetFlowIntegrationTestsOnlySigningKey123456789";


    public static string CreateToken(
        Guid userId,
        Guid? tenantId = null,
        bool isAdministrator = false)
    {
        var signingKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(Key));

        var credentials = new SigningCredentials(
            signingKey,
            SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
    {
        new Claim(
            ClaimTypes.NameIdentifier,
            userId.ToString())
    };

        if (tenantId.HasValue)
        {
            claims.Add(new Claim(
                "tenant_id",
                tenantId.Value.ToString()));
        }

        if (isAdministrator)
        {
            claims.Add(new Claim(
                ClaimTypes.Role,
                "Administrator"));
        }

        var token = new JwtSecurityToken(
            issuer: Issuer,
            audience: Audience,
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(30),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }


}

