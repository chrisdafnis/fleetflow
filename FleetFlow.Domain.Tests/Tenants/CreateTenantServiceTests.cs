using FleetFlow.Application.Tenants.CreateTenant;

namespace FleetFlow.Application.Tests.Tenants;

public class CreateTenantServiceTests
{
    [Fact]
    public async Task ExecuteAsync_WithValidName_CreatesTenant()
    {
        // Arrange
        var repository = new FakeTenantRepository();
        var service = new CreateTenantService(repository);

        var request = new CreateTenantRequest("Acme Logistics");

        // Act
        var tenantId = await service.ExecuteAsync(request);

        // Assert
        Assert.NotEqual(Guid.Empty, tenantId);
        Assert.Single(repository.Tenants);

        var tenant = repository.Tenants.Single();

        Assert.Equal(tenantId, tenant.Id);
        Assert.Equal("Acme Logistics", tenant.Name);
    }

    [Fact]
    public async Task ExecuteAsync_WithWhitespaceAroundName_TrimsName()
    {
        // Arrange
        var repository = new FakeTenantRepository();
        var service = new CreateTenantService(repository);

        var request = new CreateTenantRequest("  Acme Logistics  ");

        // Act
        await service.ExecuteAsync(request);

        // Assert
        var tenant = Assert.Single(repository.Tenants);

        Assert.Equal("Acme Logistics", tenant.Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("\t")]
    public async Task ExecuteAsync_WithInvalidName_ThrowsException(
        string name)
    {
        // Arrange
        var repository = new FakeTenantRepository();
        var service = new CreateTenantService(repository);

        var request = new CreateTenantRequest(name);

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => service.ExecuteAsync(request));

        Assert.Empty(repository.Tenants);
    }
}

