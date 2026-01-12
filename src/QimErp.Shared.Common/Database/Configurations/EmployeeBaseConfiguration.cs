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
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(e => e.MiddleName)
            .HasMaxLength(100);

        builder.Property(e => e.Email)
            .HasMaxLength(255);

        builder.Property(e => e.ProfilePicture)
            .HasMaxLength(500);

        // Current Supervisor/Manager Properties
        builder.Property(e => e.CurrentSupervisorName)
            .HasMaxLength(200);

        builder.Property(e => e.CurrentSupervisorCode)
            .HasMaxLength(50);

        builder.Property(e => e.CurrentSupervisorTitle)
            .HasMaxLength(200);

        builder.Property(e => e.CurrentSupervisorEmail)
            .HasMaxLength(255);

        builder.Property(e => e.CurrentSupervisorPhone)
            .HasMaxLength(50);

        // Current Organizational Unit Properties
        builder.Property(e => e.CurrentOrganizationalUnitName)
            .HasMaxLength(200);

        builder.Property(e => e.CurrentOrganizationalUnitCode)
            .HasMaxLength(50);

        // Current Job Title Properties
        builder.Property(e => e.CurrentJobTitleName)
            .HasMaxLength(200);

        builder.Property(e => e.CurrentJobTitleCode)
            .HasMaxLength(50);

        // Current Station Properties
        builder.Property(e => e.CurrentStationName)
            .HasMaxLength(200);

        builder.Property(e => e.CurrentStationCode)
            .HasMaxLength(50);

        // Current Job Status Properties
        builder.Property(e => e.CurrentJobStatusName)
            .HasMaxLength(200);

        builder.Property(e => e.CurrentJobStatusCode)
            .HasMaxLength(50);

        // Indexes
        builder.HasIndex(e => e.Code)
            .IsUnique();

        builder.HasIndex(e => e.Email)
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
    }
}

