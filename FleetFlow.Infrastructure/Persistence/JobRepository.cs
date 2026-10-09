using FleetFlow.Application.Jobs;
using FleetFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Infrastructure.Persistence;

public sealed class JobRepository(FleetFlowDbContext dbContext)
    : IJobRepository
{
    public async Task AddAsync(
        Job job,
        CancellationToken cancellationToken = default)
    {
        await dbContext.Jobs.AddAsync(
            job,
            cancellationToken);

        await dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<Job?> GetByIdAsync(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Jobs
            .FirstOrDefaultAsync(
                x => x.Id == jobId &&
                     x.TenantId == tenantId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Job>> GetByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        return await dbContext.Jobs
            .Where(x => x.TenantId == tenantId)
            .OrderBy(x => x.Reference)
            .ToListAsync(cancellationToken);
    }
}
