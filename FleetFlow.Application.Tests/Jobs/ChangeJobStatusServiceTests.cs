
using FleetFlow.Application.Jobs.ChangeJobStatus;
using FleetFlow.Domain;

namespace FleetFlow.Application.Tests.Jobs;

public class ChangeJobStatusServiceTests
{
    [Fact]
    public async Task Start_PendingJob_ChangesStatusToInProgress()
    {
        var repository = new FakeJobRepository();
        var job = CreateJob();

        await repository.AddAsync(job);

        var service = new ChangeJobStatusService(repository);

        var result = await service.ExecuteAsync(
            new ChangeJobStatusRequest(
                job.TenantId,
                job.Id,
                JobStatusAction.Start));

        Assert.NotNull(result);
        Assert.Equal(JobStatus.InProgress, result.Status);
    }

    [Fact]
    public async Task Complete_InProgressJob_ChangesStatusToCompleted()
    {
        var repository = new FakeJobRepository();
        var job = CreateJob();

        job.Start();
        await repository.AddAsync(job);

        var service = new ChangeJobStatusService(repository);

        var result = await service.ExecuteAsync(
            new ChangeJobStatusRequest(
                job.TenantId,
                job.Id,
                JobStatusAction.Complete));

        Assert.NotNull(result);
        Assert.Equal(JobStatus.Completed, result.Status);
    }

    [Fact]
    public async Task Cancel_PendingJob_ChangesStatusToCancelled()
    {
        var repository = new FakeJobRepository();
        var job = CreateJob();

        await repository.AddAsync(job);

        var service = new ChangeJobStatusService(repository);

        var result = await service.ExecuteAsync(
            new ChangeJobStatusRequest(
                job.TenantId,
                job.Id,
                JobStatusAction.Cancel));

        Assert.NotNull(result);
        Assert.Equal(JobStatus.Cancelled, result.Status);
    }

    [Fact]
    public async Task Complete_PendingJob_ThrowsException()
    {
        var repository = new FakeJobRepository();
        var job = CreateJob();

        await repository.AddAsync(job);

        var service = new ChangeJobStatusService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                new ChangeJobStatusRequest(
                    job.TenantId,
                    job.Id,
                    JobStatusAction.Complete)));

        Assert.Equal(JobStatus.Pending, job.Status);
    }

    [Fact]
    public async Task Start_CompletedJob_ThrowsException()
    {
        var repository = new FakeJobRepository();
        var job = CreateJob();

        job.Start();
        job.Complete();

        await repository.AddAsync(job);

        var service = new ChangeJobStatusService(repository);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ExecuteAsync(
                new ChangeJobStatusRequest(
                    job.TenantId,
                    job.Id,
                    JobStatusAction.Start)));

        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public async Task ChangeStatus_UnknownJob_ReturnsNull()
    {
        var repository = new FakeJobRepository();
        var service = new ChangeJobStatusService(repository);

        var result = await service.ExecuteAsync(
            new ChangeJobStatusRequest(
                Guid.NewGuid(),
                Guid.NewGuid(),
                JobStatusAction.Start));

        Assert.Null(result);
    }

    [Fact]
    public async Task ChangeStatus_DifferentTenant_ReturnsNull()
    {
        var repository = new FakeJobRepository();
        var job = CreateJob();

        await repository.AddAsync(job);

        var service = new ChangeJobStatusService(repository);

        var result = await service.ExecuteAsync(
            new ChangeJobStatusRequest(
                Guid.NewGuid(),
                job.Id,
                JobStatusAction.Start));

        Assert.Null(result);
        Assert.Equal(JobStatus.Pending, job.Status);
    }

    private static Job CreateJob()
    {
        return new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test delivery");
    }
}

