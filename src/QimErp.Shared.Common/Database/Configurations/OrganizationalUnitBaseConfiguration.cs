namespace QimErp.Shared.Common.Database.Configurations;

/// <summary>
/// Base configuration for OrganizationalUnitBase entities.
/// Configures common properties shared across all module-specific OrganizationalUnit entities.
/// </summary>
public abstract class OrganizationalUnitBaseConfiguration<TOrganizationalUnit> : AuditableEntityConfiguration<TOrganizationalUnit>
    where TOrganizationalUnit : OrganizationalUnitBase
{
    public override void Configure(EntityTypeBuilder<TOrganizationalUnit> builder)
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

        builder.Property(e => e.Description)
            .HasMaxLength(500);

        // Indexes
        builder.HasIndex(e => new { e.Code, e.TenantId })
            .IsUnique()
            .HasFilter("\"Code\" IS NOT NULL")
            .HasDatabaseName("IX_OrganizationalUnits_TenantId_Code");

        // Ignore computed properties
        builder.Ignore(e => e.IsActive);
    }
}
