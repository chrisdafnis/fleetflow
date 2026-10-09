using FleetFlow.Api.IntegrationTests.Authentication;
using FleetFlow.Api.IntegrationTests.Infrastructure;
using FleetFlow.Domain;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

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

        // Switch to a user authorised for this tenant.
        AuthenticateClient(client, tenantId);

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

        // Switch from administrator to tenant user.
        AuthenticateClient(client, tenantId);

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

        // Create Tenant A.
        var tenantAId = await CreateTenantAsync(client);

        // Authenticate as Tenant A.
        AuthenticateClient(client, tenantAId);

        // Create a job belonging to Tenant A.
        var jobId = await CreateJobAsync(client, tenantAId);

        // Create Tenant B (helper uses administrator token).
        var tenantBId = await CreateTenantAsync(client);

        // Authenticate as Tenant B.
        AuthenticateClient(client, tenantBId);

        // Attempt to retrieve Tenant A's job through Tenant B's route.
        var response = await client.GetAsync(
            $"/api/tenants/{tenantBId}/jobs/{jobId}");

        // The route matches the authenticated tenant,
        // but the job belongs to another tenant.
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }


    private static async Task<Guid> CreateTenantAsync(
        HttpClient client)
    {
        var adminToken = TestJwtTokenFactory.CreateToken(
            Guid.NewGuid(),
            isAdministrator: true);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                adminToken);

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

    private static void AuthenticateClient(
        HttpClient client,
        Guid tenantId)
    {
        var token = TestJwtTokenFactory.CreateToken(
            Guid.NewGuid(),
            tenantId: tenantId);

        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue(
                "Bearer",
                token);
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
