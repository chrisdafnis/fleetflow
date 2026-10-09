
using FleetFlow.Application.Tenants;
using FleetFlow.Domain;

namespace FleetFlow.Application.Tests.Tenants;

public sealed class FakeTenantRepository : ITenantRepository
{
    private readonly List<Tenant> _tenants = [];

    public IReadOnlyList<Tenant> Tenants => _tenants.AsReadOnly();

    public Task AddAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default)
    {
        _tenants.Add(tenant);

        return Task.CompletedTask;
    }
}

