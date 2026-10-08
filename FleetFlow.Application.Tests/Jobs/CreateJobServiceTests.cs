using FleetFlow.Application.Jobs.CreateJob;

namespace FleetFlow.Application.Tests.Jobs;

public class CreateJobServiceTests
{
    [Fact]
    public async Task ExecuteAsync_CreatesJob()
    {
        // Arrange
        var repository = new FakeJobRepository();
        var service = new CreateJobService(repository);
        var tenantId = Guid.NewGuid();

        var request = new CreateJobRequest(
            tenantId,
            "JOB-001",
            "Deliver 10 pallets");

        // Act
        var result = await service.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.Equal(tenantId, result.TenantId);
        Assert.Equal("JOB-001", result.Reference);
        Assert.Equal("Deliver 10 pallets", result.Description);
        Assert.Equal(
            FleetFlow.Domain.JobStatus.Pending,
            result.Status);
    }
}
