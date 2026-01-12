namespace QimErp.Shared.Common.Database.Configurations;

/// <summary>
/// Base configuration for StationBase entities.
/// Configures common properties shared across all module-specific Station entities.
/// </summary>
public abstract class StationBaseConfiguration<TStation> : AuditableEntityConfiguration<TStation>
    where TStation : StationBase
{
    public override void Configure(EntityTypeBuilder<TStation> builder)
    {
        base.Configure(builder);

        // Configure Id as Guid
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever();

        // Basic Properties
        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Code)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => new { e.Code, e.TenantId })
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL")
            .HasDatabaseName("IX_Stations_TenantId_Code");

        builder.HasIndex(e => e.OrganizationalUnitId)
            .HasFilter("\"OrganizationalUnitId\" IS NOT NULL")
            .HasDatabaseName("IX_Stations_OrganizationalUnitId");

        // Ignore computed properties
        builder.Ignore(e => e.IsActive);
    }
}
