using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using QimErp.Shared.Common.Database;
using QimErp.Shared.Common.Entities;
using QimErp.Shared.Common.Extensions;
using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Tests.Database;

public class PlainThing : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public OwnedDetail? Detail { get; set; }
}

public class DerivedThing : PlainThing
{
    public string Extra { get; set; } = string.Empty;
}

public class OwnedDetail : AuditableEntity
{
    public string Note { get; set; } = string.Empty;
}

public class CustomFilteredThing : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
}

public class SelfScopedThing : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
    public bool IsVisibleAcrossCompanies { get; set; }
}

public class TenantWideThing : BaseAuditableEntity<Guid>, ITenantWideEntity
{
    public string Name { get; set; } = string.Empty;
}

/// <summary>Reachable only through its configuration — no DbSet. The EmployeeProvisioningStatus case.</summary>
public class ConfigurationOnlyThing : BaseAuditableEntity<Guid>
{
    public string Name { get; set; } = string.Empty;
}

public class ConfigurationOnlyThingConfiguration : IEntityTypeConfiguration<ConfigurationOnlyThing>
{
    public void Configure(EntityTypeBuilder<ConfigurationOnlyThing> builder)
    {
        builder.ToTable("configuration_only_thing");
        builder.HasKey(e => e.Id);
    }
}

public sealed class ConventionTestDbContext(
    DbContextOptions<ConventionTestDbContext> options,
    ITenantContext tenantContext)
    : ApplicationDbContext<ConventionTestDbContext>(options, tenantContext)
{
    public DbSet<PlainThing> PlainThings => Set<PlainThing>();
    public DbSet<DerivedThing> DerivedThings => Set<DerivedThing>();
    public DbSet<CustomFilteredThing> CustomFilteredThings => Set<CustomFilteredThing>();
    public DbSet<SelfScopedThing> SelfScopedThings => Set<SelfScopedThing>();
    public DbSet<TenantWideThing> TenantWideThings => Set<TenantWideThing>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ITenantQueryFilterContext tenant = this;
        IScopedQueryFilterContext scoped = this;

        modelBuilder.Entity<PlainThing>().OwnsOne(e => e.Detail);
        modelBuilder.Entity<DerivedThing>();

        // Same shape as the 17 Accounting configurations: replaces the global filter, no company clause.
        modelBuilder.Entity<CustomFilteredThing>().HasQueryFilter(e =>
            e.DataStatus == DataState.Active && (e.IsGlobal || e.TenantId == tenant.CurrentTenantId));

        modelBuilder.Entity<SelfScopedThing>().HasQueryFilter(e =>
            e.DataStatus != DataState.Deleted
            && (e.IsGlobal || e.TenantId == scoped.CurrentTenantId)
            && (scoped.CompanyFilterActive == false
                || e.IsVisibleAcrossCompanies
                || scoped.AllowedCompanyIds.Contains(e.CompanyId)));

        modelBuilder.ApplyConfiguration(new ConfigurationOnlyThingConfiguration());
    }
}
