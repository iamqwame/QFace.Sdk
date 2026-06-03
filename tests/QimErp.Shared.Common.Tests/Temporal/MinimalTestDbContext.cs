using Microsoft.EntityFrameworkCore;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Tests.Temporal;

/// <summary>
/// Minimal DbContext for background service TenantId stamping tests.
/// Only includes EntityCodeConfig — a simple entity with no complex property types
/// that cause issues with EF Core's in-memory provider.
/// </summary>
public sealed class MinimalTestDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;

    public MinimalTestDbContext(DbContextOptions<MinimalTestDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<EntityCodeConfig> EntityCodeConfigs { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Ignore CustomFields (Dictionary<string,string>) — in-memory provider cannot map it
        modelBuilder.Entity<EntityCodeConfig>()
            .Ignore(e => e.CustomFields)
            .HasQueryFilter(e => e.TenantId == _tenantContext.TenantId);
    }
}
