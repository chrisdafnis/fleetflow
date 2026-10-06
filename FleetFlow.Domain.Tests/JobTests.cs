using FleetFlow.Domain;

namespace FleetFlow.Domain.Tests;

public class JobTests
{
    [Fact]
    public void NewJob_HasPendingStatus()
    {
        // Arrange
        var tenantId = Guid.NewGuid();

        // Act
        var job = new Job(
            tenantId,
            "JOB-001",
            "Deliver 10 pallets");

        // Assert
        Assert.Equal(JobStatus.Pending, job.Status);
    }

    [Fact]
    public void Start_PendingJob_ChangesStatusToInProgress()
    {
        // Arrange
        var job = CreateJob();

        // Act
        job.Start();

        // Assert
        Assert.Equal(JobStatus.InProgress, job.Status);
    }

    [Fact]
    public void Complete_InProgressJob_ChangesStatusToCompleted()
    {
        // Arrange
        var job = CreateJob();
        job.Start();

        // Act
        job.Complete();

        // Assert
        Assert.Equal(JobStatus.Completed, job.Status);
    }

    [Fact]
    public void Complete_PendingJob_ThrowsException()
    {
        // Arrange
        var job = CreateJob();

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => job.Complete());
    }

    private static Job CreateJob()
    {
        return new Job(
            Guid.NewGuid(),
            "JOB-001",
            "Test delivery");
    }
}
