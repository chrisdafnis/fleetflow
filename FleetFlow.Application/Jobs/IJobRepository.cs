
using FleetFlow.Domain;

namespace FleetFlow.Application.Jobs;

public interface IJobRepository
{
    Task AddAsync(
        Job job,
        CancellationToken cancellationToken = default);

    Task<Job?> GetByIdAsync(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Job>> GetByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default);
}