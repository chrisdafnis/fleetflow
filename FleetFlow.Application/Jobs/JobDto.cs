
using FleetFlow.Domain;

namespace FleetFlow.Application.Jobs;

public sealed record JobDto(
    Guid Id,
    Guid TenantId,
    string Reference,
    string Description,
    JobStatus Status);