using FleetFlow.Application.Jobs;
using FleetFlow.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;

namespace FleetFlow.Infrastructure.Persistence;

public sealed class JobRepository(FleetFlowDbContext dbContext)
    : IJobRepository
{
    public async Task AddAsync(
        Job job,
        CancellationToken cancellationToken = default)
    {
        var referenceExists = await dbContext.Jobs.AnyAsync(
            existing => existing.TenantId == job.TenantId
                && existing.Reference == job.Reference,
            cancellationToken);

        if (referenceExists)
        {
            throw new DuplicateJobReferenceException(job.Reference);
        }

        await dbContext.Jobs.AddAsync(job, cancellationToken);

        try
        {
            await dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (
            ex.InnerException is SqlException sqlException
            && (sqlException.Number == 2601
                || sqlException.Number == 2627))
        {
            // Confirm the violated uniqueness rule was the job reference.
            var duplicateReference = await dbContext.Jobs.AnyAsync(
                existing => existing.TenantId == job.TenantId
                    && existing.Reference == job.Reference,
                cancellationToken);

            if (duplicateReference)
            {
                throw new DuplicateJobReferenceException(
                    job.Reference,
                    ex);
            }

            throw;
        }
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

    public async Task UpdateAsync(
        Job job,
        CancellationToken cancellationToken = default)
    {
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
