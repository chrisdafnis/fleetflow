using FleetFlow.Application.Jobs;
using FleetFlow.Domain;

namespace FleetFlow.Application.Tests.Jobs;

public sealed class FakeJobRepository : IJobRepository
{
    private readonly List<Job> _jobs = [];

    public Task AddAsync(
        Job job,
        CancellationToken cancellationToken = default)
    {
        _jobs.Add(job);

        return Task.CompletedTask;
    }

    public Task<Job?> GetByIdAsync(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = _jobs.FirstOrDefault(
            x => x.TenantId == tenantId && x.Id == jobId);

        return Task.FromResult(job);
    }

    public Task<IReadOnlyList<Job>> GetByTenantAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        IReadOnlyList<Job> jobs = _jobs
            .Where(x => x.TenantId == tenantId)
            .ToList();

        return Task.FromResult(jobs);
    }

    public Task UpdateAsync(
        Job job,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}
