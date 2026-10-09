
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace FleetFlow.Api.Authorization;

public sealed class TenantAccessHandler
    : AuthorizationHandler<TenantAccessRequirement>
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantAccessHandler(
        IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        TenantAccessRequirement requirement)
    {
        // The user must be authenticated.
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return Task.CompletedTask;
        }

        // Read the tenant ID from the JWT.
        var tenantClaim = context.User.FindFirst("tenant_id");

        if (tenantClaim is null ||
            !Guid.TryParse(tenantClaim.Value, out var tokenTenantId))
        {
            return Task.CompletedTask;
        }

        // Read the tenant ID from the route.
        var httpContext = _httpContextAccessor.HttpContext;

        if (httpContext is null)
        {
            return Task.CompletedTask;
        }

        var routeTenantIdValue =
            httpContext.Request.RouteValues["tenantId"]?.ToString();

        if (!Guid.TryParse(
            routeTenantIdValue,
            out var routeTenantId))
        {
            return Task.CompletedTask;
        }

        // Authorise only when the tenant IDs match.
        if (tokenTenantId == routeTenantId)
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
