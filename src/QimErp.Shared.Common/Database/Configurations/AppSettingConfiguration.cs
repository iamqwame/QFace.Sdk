namespace QimErp.Shared.Common.Database.Configurations;

public class AppSettingConfiguration : AuditableEntityConfiguration<AppSetting>
{
    public override void Configure(EntityTypeBuilder<AppSetting> builder)
    {
        base.Configure(builder);

        builder.ToTable("AppSettings");

        // Required properties
        builder.Property(as_ => as_.Key).IsRequired();
        builder.Property(as_ => as_.Value).IsRequired();
        builder.Property(as_ => as_.Category).IsRequired();

        // Optional properties
        builder.Property(x => x.DataType).IsRequired()
            .HasConversion(new EnumToStringConverter<AppSettingDataType>());
        
        // Configure ValidationRules as a simple JSON column without complex property mapping
        builder.Property(as_ => as_.ValidationRules)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v.ToJson(),
                v => AppSettingValidationRules.FromJson(v) ?? AppSettingValidationRules.Create()
            );

        // Indexes
        builder.HasIndex(as_ => new { as_.TenantId, as_.Key }).IsUnique();
        builder.HasIndex(as_ => as_.Category);
    }
}


