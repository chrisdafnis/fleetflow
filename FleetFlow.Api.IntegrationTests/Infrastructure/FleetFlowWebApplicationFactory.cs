
using FleetFlow.Api.IntegrationTests.Authentication;
using FleetFlow.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using System.Text;

namespace FleetFlow.Api.IntegrationTests.Infrastructure;

public sealed class FleetFlowWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Supply JWT configuration for integration tests.
        builder.UseSetting(
            "Jwt:Issuer",
            TestJwtTokenFactory.Issuer);

        builder.UseSetting(
            "Jwt:Audience",
            TestJwtTokenFactory.Audience);

        builder.UseSetting(
            "Jwt:Key",
            Convert.ToBase64String(
                Encoding.UTF8.GetBytes(
                    TestJwtTokenFactory.Key)));

        // Replace SQL Server with SQLite in-memory.
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<
                DbContextOptions<FleetFlowDbContext>>();

            services.RemoveAll<
                IDbContextOptionsConfiguration<FleetFlowDbContext>>();

            _connection = new SqliteConnection(
                "Data Source=:memory:");

            _connection.Open();

            services.AddDbContext<FleetFlowDbContext>(
                options => options.UseSqlite(_connection));

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
