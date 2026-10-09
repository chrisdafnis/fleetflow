
using System.Net;
using System.Net.Http.Json;
using FleetFlow.Api.IntegrationTests.Infrastructure;
using FleetFlow.Domain;

namespace FleetFlow.Api.IntegrationTests.Jobs;

public sealed class JobLifecycleTests
{
    [Fact]
    public async Task JobLifecycle_CreatesStartsAndCompletesJob()
    {
        // Arrange
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        var tenantId = await CreateTenantAsync(client);

        // Create job
        var createResponse = await client.PostAsJsonAsync(
            $"/api/tenants/{tenantId}/jobs",
            new
            {
                Reference = "JOB-001",
                Description = "Deliver 10 pallets"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            createResponse.StatusCode);

        var createdJob = await createResponse.Content
            .ReadFromJsonAsync<JobResponse>();

        Assert.NotNull(createdJob);
        Assert.NotEqual(Guid.Empty, createdJob.Id);
        Assert.Equal(tenantId, createdJob.TenantId);
        Assert.Equal(JobStatus.Pending, createdJob.Status);

        var jobUrl =
            $"/api/tenants/{tenantId}/jobs/{createdJob.Id}";

        // Retrieve job
        var getResponse = await client.GetAsync(jobUrl);

        Assert.Equal(
            HttpStatusCode.OK,
            getResponse.StatusCode);

        var retrievedJob = await getResponse.Content
            .ReadFromJsonAsync<JobResponse>();

        Assert.NotNull(retrievedJob);
        Assert.Equal(JobStatus.Pending, retrievedJob.Status);

        // Start job
        var startResponse = await client.PatchAsync(
            $"{jobUrl}/start",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            startResponse.StatusCode);

        var startedJob = await startResponse.Content
            .ReadFromJsonAsync<JobResponse>();

        Assert.NotNull(startedJob);
        Assert.Equal(JobStatus.InProgress, startedJob.Status);

        // Complete job
        var completeResponse = await client.PatchAsync(
            $"{jobUrl}/complete",
            null);

        Assert.Equal(
            HttpStatusCode.OK,
            completeResponse.StatusCode);

        var completedJob = await completeResponse.Content
            .ReadFromJsonAsync<JobResponse>();

        Assert.NotNull(completedJob);
        Assert.Equal(JobStatus.Completed, completedJob.Status);

        // Verify persisted status through another request
        var finalResponse = await client.GetAsync(jobUrl);

        Assert.Equal(
            HttpStatusCode.OK,
            finalResponse.StatusCode);

        var finalJob = await finalResponse.Content
            .ReadFromJsonAsync<JobResponse>();

        Assert.NotNull(finalJob);
        Assert.Equal(JobStatus.Completed, finalJob.Status);
    }

    [Fact]
    public async Task Start_CompletedJob_ReturnsConflict()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        var tenantId = await CreateTenantAsync(client);
        var jobId = await CreateJobAsync(client, tenantId);

        var jobUrl =
            $"/api/tenants/{tenantId}/jobs/{jobId}";

        var startResponse = await client.PatchAsync(
            $"{jobUrl}/start",
            null);

        Assert.Equal(HttpStatusCode.OK, startResponse.StatusCode);

        var completeResponse = await client.PatchAsync(
            $"{jobUrl}/complete",
            null);

        Assert.Equal(HttpStatusCode.OK, completeResponse.StatusCode);

        // Attempt invalid transition
        var invalidResponse = await client.PatchAsync(
            $"{jobUrl}/start",
            null);

        Assert.Equal(
            HttpStatusCode.Conflict,
            invalidResponse.StatusCode);
    }

    [Fact]
    public async Task GetJob_WithDifferentTenant_ReturnsNotFound()
    {
        using var factory = new FleetFlowWebApplicationFactory();
        using var client = factory.CreateClient();

        var tenantId = await CreateTenantAsync(client);
        var jobId = await CreateJobAsync(client, tenantId);

        // Create a second, different tenant
        var otherTenantId = await CreateTenantAsync(client);

        var response = await client.GetAsync(
            $"/api/tenants/{otherTenantId}/jobs/{jobId}");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private static async Task<Guid> CreateTenantAsync(
        HttpClient client)
    {
        var response = await client.PostAsJsonAsync(
            "/api/tenants",
            new { Name = "Acme Logistics" });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var tenant = await response.Content
            .ReadFromJsonAsync<TenantResponse>();

        Assert.NotNull(tenant);

        return tenant.Id;
    }

    private static async Task<Guid> CreateJobAsync(
        HttpClient client,
        Guid tenantId)
    {
        var response = await client.PostAsJsonAsync(
            $"/api/tenants/{tenantId}/jobs",
            new
            {
                Reference = "JOB-001",
                Description = "Test delivery"
            });

        Assert.Equal(
            HttpStatusCode.Created,
            response.StatusCode);

        var job = await response.Content
            .ReadFromJsonAsync<JobResponse>();

        Assert.NotNull(job);

        return job.Id;
    }

    private sealed record TenantResponse(
        Guid Id,
        string Name);

    private sealed record JobResponse(
        Guid Id,
        Guid TenantId,
        string Reference,
        string Description,
        JobStatus Status);
}
