
using FleetFlow.Application.Jobs;
using FleetFlow.Domain;

namespace FleetFlow.Application.Jobs.CreateJob;

public sealed class CreateJobService(IJobRepository jobRepository)
{
    public async Task<JobDto> ExecuteAsync(
        CreateJobRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = new Job(
            request.TenantId,
            request.Reference,
            request.Description);

        await jobRepository.AddAsync(job, cancellationToken);

        return new JobDto(
            job.Id,
            job.TenantId,
            job.Reference,
            job.Description,
            job.Status);
    }
}
