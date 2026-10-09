
using FleetFlow.Application.Jobs.CreateJob;
using FleetFlow.Application.Jobs.GetJob;
using FleetFlow.Application.Jobs.ChangeJobStatus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace FleetFlow.Api.Controllers;

[ApiController]
[Route("api/tenants/{tenantId:guid}/jobs")]
[Authorize(Policy = "TenantAccess")]
public sealed class JobsController(
    CreateJobService createJobService,
    GetJobService getJobService,
    ChangeJobStatusService changeJobStatusService) : ControllerBase
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


    [HttpPatch("{jobId:guid}/start")]
    public Task<ActionResult> Start(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            tenantId,
            jobId,
            JobStatusAction.Start,
            cancellationToken);
    }

    [HttpPatch("{jobId:guid}/complete")]
    public Task<ActionResult> Complete(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            tenantId,
            jobId,
            JobStatusAction.Complete,
            cancellationToken);
    }

    [HttpPatch("{jobId:guid}/cancel")]
    public Task<ActionResult> Cancel(
        Guid tenantId,
        Guid jobId,
        CancellationToken cancellationToken)
    {
        return ChangeStatus(
            tenantId,
            jobId,
            JobStatusAction.Cancel,
            cancellationToken);
    }

    private async Task<ActionResult> ChangeStatus(
        Guid tenantId,
        Guid jobId,
        JobStatusAction action,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await changeJobStatusService.ExecuteAsync(
                new ChangeJobStatusRequest(
                    tenantId,
                    jobId,
                    action),
                cancellationToken);

            if (result is null)
            {
                return NotFound();
            }

            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Invalid job status transition",
                Detail = ex.Message
            });
        }
    }

}

public sealed record CreateJobBody(
    string Reference,
    string Description);
