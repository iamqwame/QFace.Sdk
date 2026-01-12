namespace QimErp.Shared.Common.Database.Configurations;

public abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.DataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.Property(e => e.CustomFields).HasColumnType("jsonb").IsRequired(false);
        builder.Property(e => e.PreviousDataStatus).HasConversion(new EnumToStringConverter<DataState>());
        
        // Common indexes for auditable entities
        builder.HasIndex(e => e.DataStatus); // For filtering by status
        builder.HasIndex(e => e.Created); // For sorting by creation date
        builder.HasIndex(e => e.LastModified); // For sorting by last modified
        builder.HasIndex(e => new { e.DataStatus, e.Created }); // Composite for common queries
        builder.HasIndex(e => new { e.DataStatus, e.LastModified }); // Composite for recent changes
    }
}

