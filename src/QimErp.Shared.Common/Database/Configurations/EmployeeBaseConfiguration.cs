namespace QimErp.Shared.Common.Database.Configurations;

/// <summary>
/// Base configuration for EmployeeBase entities.
/// Configures common properties shared across all module-specific Employee entities.
/// </summary>
public abstract class EmployeeBaseConfiguration<TEmployee> : AuditableEntityConfiguration<TEmployee>
    where TEmployee : EmployeeBase
{
    public override void Configure(EntityTypeBuilder<TEmployee> builder)
    {
        base.Configure(builder);

        // Configure Id as Guid
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever();

        // Basic Information Properties
        builder.Property(e => e.Code)
            .IsRequired();

        builder.Property(e => e.FirstName)
            .IsRequired();

        builder.Property(e => e.LastName)
            .IsRequired();

        // Indexes — per-tenant composite uniqueness (Code and Email are unique within a tenant, not globally)
        builder.HasIndex(e => new { e.TenantId, e.Code })
            .IsUnique();

        builder.HasIndex(e => new { e.TenantId, e.Email })
            .IsUnique()
            .HasFilter("\"Email\" IS NOT NULL");
        
        builder.HasIndex(e => e.CurrentSupervisorId);
        
        builder.HasIndex(e => e.CurrentOrganizationalUnitId);
        
        builder.HasIndex(e => e.CurrentJobTitleId);
        
        builder.HasIndex(e => e.CurrentStationId);
        
        builder.HasIndex(e => e.CurrentJobStatusId);

        // Ignore computed properties
        builder.Ignore(e => e.IsActive);
        builder.Ignore(e => e.FullName);
        builder.Ignore(e => e.IsFemale);
    }
}

