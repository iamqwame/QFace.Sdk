namespace QimErp.Shared.Common.Database.Configurations;

public class EntityCodeConfigConfiguration : AuditableEntityConfiguration<EntityCodeConfig>
{
    public override void Configure(EntityTypeBuilder<EntityCodeConfig> builder)
    {
        base.Configure(builder);

        builder.ToTable("EntityCodeConfigs");

        builder.Property(e => e.EntityType).IsRequired();
        builder.Property(e => e.Prefix).IsRequired();
        builder.Property(e => e.Separator).IsRequired();
        builder.Property(e => e.PaddingWidth).IsRequired();
        builder.Property(e => e.LastSequence).IsRequired();
        builder.Property(e => e.ManualHighWaterMark).IsRequired();

        builder.Property(e => e.Mode)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<CodeGenerationMode>());

        builder.Property(e => e.ResetPeriod)
            .IsRequired()
            .HasConversion(new EnumToStringConverter<CodeResetPeriod>());

        builder.HasIndex(e => new { e.TenantId, e.CompanyId, e.EntityType })
            .IsUnique()
            .HasDatabaseName("IX_EntityCodeConfigs_TenantId_CompanyId_EntityType");
    }
}
