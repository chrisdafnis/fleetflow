namespace FleetFlow.Application.Jobs.GetJob;

public sealed class GetJobsService(IJobRepository jobRepository)
{
    public async Task<IReadOnlyList<JobDto>> ExecuteAsync(
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        var jobs = await jobRepository.GetByTenantAsync(
            tenantId,
            cancellationToken);

        return jobs
            .Select(job => new JobDto(
                job.Id,
                job.TenantId,
                job.Reference,
                job.Description,
                job.Status))
            .ToList();
    }
}