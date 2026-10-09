
using FleetFlow.Domain;

namespace FleetFlow.Application.Jobs.ChangeJobStatus;

public sealed class ChangeJobStatusService(
    IJobRepository jobRepository)
{
    public async Task<JobDto?> ExecuteAsync(
        ChangeJobStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var job = await jobRepository.GetByIdAsync(
            request.TenantId,
            request.JobId,
            cancellationToken);

        if (job is null)
        {
            return null;
        }

        switch (request.Action)
        {
            case JobStatusAction.Start:
                job.Start();
                break;

            case JobStatusAction.Complete:
                job.Complete();
                break;

            case JobStatusAction.Cancel:
                job.Cancel();
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request.Action),
                    request.Action,
                    "Unsupported job status action.");
        }

        await jobRepository.UpdateAsync(
            job,
            cancellationToken);

        return new JobDto(
            job.Id,
            job.TenantId,
            job.Reference,
            job.Description,
            job.Status);
    }
}

