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

        // Without an explicit default, every pre-existing row upgrades to the first enum member.
        builder.Property(x => x.Scope).IsRequired()
            .HasConversion(new EnumToStringConverter<AppSettingScope>())
            .HasDefaultValue(AppSettingScope.CompanyOverridable);

        // Configure ValidationRules as a simple JSON column without complex property mapping
        builder.Property(as_ => as_.ValidationRules)
            .HasColumnType("jsonb")
            .HasConversion(
                v => v.ToJson(),
                v => AppSettingValidationRules.FromJson(v) ?? AppSettingValidationRules.Create()
            );

        // Indexes
        builder.HasIndex(as_ => new { as_.TenantId, as_.CompanyId, as_.Key }).IsUnique();
        builder.HasIndex(as_ => as_.Category);
    }
}


