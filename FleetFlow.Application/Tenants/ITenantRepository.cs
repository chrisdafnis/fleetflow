using FleetFlow.Domain;

namespace FleetFlow.Application.Tenants;

public interface ITenantRepository
{
    Task AddAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default);
}