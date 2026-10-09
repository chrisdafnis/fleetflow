using FleetFlow.Application.Tenants;
using FleetFlow.Domain;

namespace FleetFlow.Infrastructure.Persistence;

public sealed class TenantRepository(
    FleetFlowDbContext dbContext) : ITenantRepository
{
    public async Task AddAsync(
        Tenant tenant,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Tenants.AddAsync(
            tenant,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
