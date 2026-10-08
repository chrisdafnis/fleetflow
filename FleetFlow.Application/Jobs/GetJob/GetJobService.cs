
namespace FleetFlow.Application.Jobs.GetJob;

using FleetFlow.Application.Jobs;

public sealed class GetJobService(IJobRepository jobRepository)
{
    public async Task<JobDto?> ExecuteAsync(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetByIdAsync(
            tenantId,
            jobId,
            cancellationToken);

        if (job is null)
        {
            return null;
        }

        return new JobDto(
            job.Id,
            job.TenantId,
            job.Reference,
            job.Description,
            job.Status);
    }
}
