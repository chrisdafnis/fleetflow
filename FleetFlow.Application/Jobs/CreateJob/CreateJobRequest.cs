
namespace FleetFlow.Application.Jobs.CreateJob;

public sealed record CreateJobRequest(
    Guid TenantId,
    string Reference,
    string Description);
