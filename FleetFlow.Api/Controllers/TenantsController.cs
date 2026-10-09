using FleetFlow.Application.Tenants.CreateTenant;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize(Policy = "AdministratorOnly")]
public sealed class TenantsController(
    CreateTenantService createTenantService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<CreateTenantResponse>> Create(
        [FromBody] CreateTenantBody request,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return BadRequest("Tenant name is required.");
        }

        var tenantId = await createTenantService.ExecuteAsync(
            new CreateTenantRequest(request.Name),
            cancellationToken);

        return StatusCode(
            StatusCodes.Status201Created,
            new CreateTenantResponse(tenantId, request.Name.Trim()));
    }
}

public sealed record CreateTenantBody(string Name);

public sealed record CreateTenantResponse(Guid Id, string Name);
