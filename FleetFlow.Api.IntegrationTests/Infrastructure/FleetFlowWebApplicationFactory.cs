
using FleetFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace FleetFlow.Api.IntegrationTests.Infrastructure;

public sealed class FleetFlowWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove the production SQL Server configuration.
            services.RemoveAll<DbContextOptions<FleetFlowDbContext>>();

            // Remove the SQL Server provider configuration.
            services.RemoveAll<
                IDbContextOptionsConfiguration<FleetFlowDbContext>>();

            _connection = new SqliteConnection(
                "Data Source=:memory:");

            _connection.Open();

            services.AddDbContext<FleetFlowDbContext>(options =>
                options.UseSqlite(_connection));

            // Create the database schema for the tests.
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();

            var dbContext = scope.ServiceProvider
                .GetRequiredService<FleetFlowDbContext>();

            dbContext.Database.EnsureCreated();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection?.Dispose();
        }
    }
}
