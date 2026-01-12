namespace QimErp.Shared.Common.Database.Configurations;

/// <summary>
/// Base configuration for CostCenterBase entities.
/// Configures common properties shared across all module-specific CostCenter entities.
/// </summary>
public abstract class CostCenterBaseConfiguration<TCostCenter> : AuditableEntityConfiguration<TCostCenter>
    where TCostCenter : CostCenterBase
{
    public override void Configure(EntityTypeBuilder<TCostCenter> builder)
    {
        base.Configure(builder);

        // Configure Id as Guid
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever();

        // Basic Properties
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => new { e.Code, e.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_CostCenters_TenantId_Code");

        // Ignore computed properties
        builder.Ignore(e => e.IsActive);
    }
}
