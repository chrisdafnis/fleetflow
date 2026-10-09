using FleetFlow.Domain;
using Microsoft.EntityFrameworkCore;

namespace FleetFlow.Infrastructure.Persistence;

public sealed class FleetFlowDbContext(
    DbContextOptions<FleetFlowDbContext> options)
    : DbContext(options)
{
    public DbSet<Tenant> Tenants => Set<Tenant>();

    public DbSet<Job> Jobs => Set<Job>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Tenant>(entity =>
        {
            entity.ToTable("Tenants");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(200);
        });

        modelBuilder.Entity<Job>(entity =>
        {
            entity.ToTable("Jobs");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Reference)
                .IsRequired()
                .HasMaxLength(50);

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.Status)
                .IsRequired();

            entity.HasIndex(x => new
            {
                x.TenantId,
                x.Reference
            })
            .IsUnique();

            entity.HasOne<Tenant>()
                .WithMany(x => x.Jobs)
                .HasForeignKey(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}