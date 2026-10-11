using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FleetFlow.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace FleetFlow.Api.Authentication;

public sealed class AccessTokenService(IOptionsMonitor<JwtBearerOptions> options)
{
    public AccessTokenResponse Create(ApplicationUser user, IEnumerable<string> roles)
    {
        var settings = options.Get(JwtBearerDefaults.AuthenticationScheme)
            .TokenValidationParameters;
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(30);
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new("tenant_id", user.TenantId.ToString())
        };
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var token = new JwtSecurityToken(
            issuer: settings.ValidIssuer,
            audience: settings.ValidAudience,
            claims: claims,
            notBefore: now,
            expires: expires,
            signingCredentials: new SigningCredentials(
                settings.IssuerSigningKey, SecurityAlgorithms.HmacSha256));

        return new AccessTokenResponse(
            new JwtSecurityTokenHandler().WriteToken(token),
            "Bearer", expires, user.Id, user.TenantId);
    }
}

public sealed record AccessTokenResponse(
    string AccessToken, string TokenType, DateTime ExpiresAtUtc,
    Guid UserId, Guid TenantId);
