
using FleetFlow.Application.Jobs.CreateJob;
using FleetFlow.Application.Jobs.GetJob;
using Microsoft.AspNetCore.Mvc;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/jobs")]
public sealed class JobsController(
    CreateJobService createJobService,
    GetJobService getJobService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult> Create(
        Guid tenantId,
        [FromBody] CreateJobBody request,
        CancellationToken cancellationToken)
    {
        var result = await createJobService.ExecuteAsync(
            new CreateJobRequest(
                tenantId,
                request.Reference,
                request.Description),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { tenantId, jobId = result.Id },
            result);
    }

    [HttpGet("{jobId:guid}")]
    public async Task<ActionResult> GetById(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        var result = await getJobService.ExecuteAsync(
            tenantId,
            jobId,
            cancellationToken);

        if (result is null)
        {
            return NotFound();
        }

        return Ok(result);
    }
}

public sealed record CreateJobBody(
    string Reference,
    string Description);
