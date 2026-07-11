namespace QimErp.Shared.Common.Database.Configurations;

public class TenantPluginFlagConfiguration : AuditableEntityConfiguration<TenantPluginFlag>
{
    public override void Configure(EntityTypeBuilder<TenantPluginFlag> builder)
    {
        base.Configure(builder);

        builder.ToTable("TenantPluginFlags");

        builder.Property(x => x.PluginKey).IsRequired();

        builder.HasIndex(x => new { x.TenantId, x.PluginKey }).IsUnique();
    }
}
