using FleetFlow.Domain;

namespace FleetFlow.Application.Tenants.CreateTenant;

public sealed class CreateTenantService(
    ITenantRepository tenantRepository)
{
    public async Task<Guid> ExecuteAsync(
        CreateTenantRequest request,
        CancellationToken cancellationToken = default)
    {
        var tenant = new Tenant(request.Name);

        await tenantRepository.AddAsync(
            tenant,
            cancellationToken);

        return tenant.Id;
    }
}