namespace QimErp.Shared.Common.Database.Configurations;

public class TaxationSharedConfiguration : AuditableEntityConfiguration<TaxationShared>
{
    public override void Configure(EntityTypeBuilder<TaxationShared> builder)
    {
        base.Configure(builder);
        builder.ToTable("Tax");
        builder.Property(e => e.Name).IsRequired().HasMaxLength(100);
        builder.Property(e => e.InvoiceDistributions).HasColumnType("jsonb");
        builder.Property(e => e.RefundDistributions).HasColumnType("jsonb");
    }
}