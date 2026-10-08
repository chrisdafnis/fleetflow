using FleetFlow.Application.Jobs.GetJob;
using FleetFlow.Domain;

namespace FleetFlow.Application.Tests.Jobs;

public class GetJobServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithExistingJob_ReturnsJob()
    {
        // Arrange
        var repository = new FakeJobRepository();

        var tenantId = Guid.NewGuid();

        var job = new Job(
            tenantId,
            "JOB-001",
            "Deliver 10 pallets");

        await repository.AddAsync(job);

        var service = new GetJobService(repository);

        // Act
        var result = await service.ExecuteAsync(
            tenantId,
            job.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(job.Id, result.Id);
        Assert.Equal("JOB-001", result.Reference);
    }

    [Fact]
    public async Task ExecuteAsync_WithUnknownJob_ReturnsNull()
    {
        // Arrange
        var repository = new FakeJobRepository();
        var service = new GetJobService(repository);

        // Act
        var result = await service.ExecuteAsync(
            Guid.NewGuid(),
            Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task ExecuteAsync_WithDifferentTenant_ReturnsNull()
    {
        // Arrange
        var repository = new FakeJobRepository();

        var tenantId = Guid.NewGuid();
        var differentTenantId = Guid.NewGuid();

        var job = new Job(
            tenantId,
            "JOB-001",
            "Deliver 10 pallets");

        await repository.AddAsync(job);

        var service = new GetJobService(repository);

        // Act
        var result = await service.ExecuteAsync(
            differentTenantId,
            job.Id);

        // Assert
        Assert.Null(result);
    }
}
