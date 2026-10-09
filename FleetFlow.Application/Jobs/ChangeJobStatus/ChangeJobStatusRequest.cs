
namespace FleetFlow.Application.Jobs.ChangeJobStatus;

public enum JobStatusAction
{
    Start,
    Complete,
    Cancel
}

public sealed record ChangeJobStatusRequest(
    Guid TenantId,
    Guid JobId,
    JobStatusAction Action);

