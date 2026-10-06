using FleetFlow.Domain;

namespace FleetFlow.Domain.Tests;

public class TenantTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesTenant()
    {
        // Arrange
        const string name = "Acme Logistics";

        // Act
        var tenant = new Tenant(name);

        // Assert
        Assert.NotEqual(Guid.Empty, tenant.Id);
        Assert.Equal(name, tenant.Name);
        Assert.Empty(tenant.Jobs);
    }

    [Fact]
    public void Constructor_WithEmptyName_ThrowsException()
    {
        // Act & Assert
        Assert.Throws<ArgumentException>(
            () => new Tenant(string.Empty));
    }

    [Fact]
    public void Rename_WithValidName_ChangesTenantName()
    {
        // Arrange
        var tenant = new Tenant("Acme Logistics");

        // Act
        tenant.Rename("Acme Transport");

        // Assert
        Assert.Equal("Acme Transport", tenant.Name);
    }
}