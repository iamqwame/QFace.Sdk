namespace QimErp.Shared.Common.Database.Configurations;

/// <summary>
/// Base configuration for ChartOfAccountBase entities.
/// Configures common properties shared across all module-specific ChartOfAccount entities.
/// </summary>
public abstract class ChartOfAccountBaseConfiguration<TChartOfAccount> : AuditableEntityConfiguration<TChartOfAccount>
    where TChartOfAccount : ChartOfAccountBase
{
    public override void Configure(EntityTypeBuilder<TChartOfAccount> builder)
    {
        base.Configure(builder);

        // Configure Id as Guid
        builder.Property(e => e.Id)
            .IsRequired()
            .ValueGeneratedNever();

        // Basic Properties
        builder.Property(e => e.Code)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(e => e.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(e => e.AccountType)
            .IsRequired();

        builder.Property(e => e.NormalBalance)
            .IsRequired();

        builder.Property(e => e.IsPostingAccount)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(e => e.GlAccountId)
            .IsRequired();

        // Indexes
        builder.HasIndex(e => new { e.Code, e.TenantId })
            .IsUnique()
            .HasDatabaseName("IX_ChartOfAccounts_TenantId_Code");

        builder.HasIndex(e => new { e.GlAccountId, e.TenantId })
            .HasDatabaseName("IX_ChartOfAccounts_GlAccountId_TenantId");

        builder.HasIndex(e => new { e.AccountType, e.TenantId })
            .HasDatabaseName("IX_ChartOfAccounts_AccountType_TenantId");

        // Ignore computed properties
        builder.Ignore(e => e.IsActive);
    }
}
