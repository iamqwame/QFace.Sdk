namespace QimErp.Shared.Common.Database.Configurations;

public class ImportConfiguration : AuditableEntityConfiguration<Import>
{
    public override void Configure(EntityTypeBuilder<Import> builder)
    {
        base.Configure(builder);

        builder.ToTable("Imports");

        builder.Property(i => i.ImportType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.FileName)
            .HasMaxLength(255);

        builder.Property(i => i.ContentType)
            .HasMaxLength(100);

        builder.Property(i => i.TotalRows)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.ProcessedRows)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.SuccessfulImports)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.FailedImports)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.Percentage)
            .IsRequired()
            .HasDefaultValue(0)
            .HasColumnType("decimal(5,2)");

        builder.Property(i => i.StartedAt)
            .IsRequired();

        builder.Property(i => i.LastUpdatedAt)
            .IsRequired();

        builder.Property(i => i.ErrorMessage)
            .HasMaxLength(1000);

        builder.Property(i => i.BatchesQueued)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.BatchesSaved)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(i => i.BatchesFailed)
            .IsRequired()
            .HasDefaultValue(0);

        builder.HasIndex(i => new { i.TenantId, i.Status });
        builder.HasIndex(i => new { i.TenantId, i.ImportType });
        builder.HasIndex(i => new { i.TenantId, i.Created }).HasDatabaseName("IX_Imports_TenantId_Created");
    }
}

