namespace QimErp.Shared.Common.Database.Configurations;

public abstract class AuditableEntityConfiguration<TEntity> : IEntityTypeConfiguration<TEntity>
    where TEntity : AuditableEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.Property(e => e.DataStatus).HasConversion(new EnumToStringConverter<DataState>());
        builder.Property(e => e.CustomFields)
            .HasConversion(
                v => v == null ? null : System.Text.Json.JsonSerializer.Serialize(v, (System.Text.Json.JsonSerializerOptions?)null),
                v => v == null ? null : System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(v, (System.Text.Json.JsonSerializerOptions?)null))
            .HasColumnType("jsonb");
        builder.Property(e => e.PreviousDataStatus).HasConversion(new EnumToStringConverter<DataState>());
        
        // Common indexes for auditable entities
        builder.HasIndex(e => e.DataStatus); // For filtering by status
        builder.HasIndex(e => e.Created); // For sorting by creation date
        builder.HasIndex(e => e.LastModified); // For sorting by last modified
        builder.HasIndex(e => new { e.DataStatus, e.Created }); // Composite for common queries
        builder.HasIndex(e => new { e.DataStatus, e.LastModified }); // Composite for recent changes
    }
}

