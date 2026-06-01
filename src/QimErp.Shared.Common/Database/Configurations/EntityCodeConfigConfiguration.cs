namespace QimErp.Shared.Common.Database.Configurations;

public class EntityCodeConfigConfiguration : AuditableEntityConfiguration<EntityCodeConfig>
{
    public override void Configure(EntityTypeBuilder<EntityCodeConfig> builder)
    {
        base.Configure(builder);

        builder.ToTable("EntityCodeConfigs");

        builder.Property(e => e.EntityType).IsRequired().HasMaxLength(100);
        builder.Property(e => e.Prefix).IsRequired().HasMaxLength(20);
        builder.Property(e => e.Separator).IsRequired().HasMaxLength(5);
        builder.Property(e => e.PaddingWidth).IsRequired();
        builder.Property(e => e.LastSequence).IsRequired();
        builder.Property(e => e.ManualHighWaterMark).IsRequired();
        builder.Property(e => e.LastResetPeriodKey).HasMaxLength(20);

        builder.Property(e => e.Mode)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<CodeGenerationMode>());

        builder.Property(e => e.ResetPeriod)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<CodeResetPeriod>());

        // One config per tenant + entity type (no subsidiaries in v1)
        builder.HasIndex(e => new { e.TenantId, e.EntityType })
            .IsUnique()
            .HasDatabaseName("IX_EntityCodeConfigs_TenantId_EntityType");
    }
}
