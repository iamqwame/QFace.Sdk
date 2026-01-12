namespace QimErp.Shared.Common.Database.Configurations;

/// <summary>
/// Base EF Core configuration for lookup tables.
/// Provides common configuration for all lookup entities.
/// </summary>
public abstract class LookupBaseConfiguration<TLookup> : AuditableEntityConfiguration<TLookup>
    where TLookup : LookupBase
{
    public override void Configure(EntityTypeBuilder<TLookup> builder)
    {
        base.Configure(builder);

        // Configure properties
        builder.Property(l => l.Code)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(l => l.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(l => l.Description)
            .HasMaxLength(500);

        builder.Property(l => l.DisplayOrder)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(l => l.IsSystemDefault)
            .IsRequired()
            .HasDefaultValue(false);

        builder.Property(l => l.LookupType)
            .IsRequired()
            .HasMaxLength(100);

        // Unique constraint: Code + LookupType + TenantId must be unique
        builder.HasIndex(l => new { l.TenantId, l.LookupType, l.Code })
            .IsUnique();

        // Indexes for common queries
        builder.HasIndex(l => new { l.TenantId, l.LookupType, l.DataStatus });
        builder.HasIndex(l => new { l.TenantId, l.LookupType, l.DisplayOrder });
        builder.HasIndex(l => l.LookupType);
    }
}

