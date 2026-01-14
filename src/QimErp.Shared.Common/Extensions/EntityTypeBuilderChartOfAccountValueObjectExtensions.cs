namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring ChartOfAccountValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderChartOfAccountValueObjectExtensions
{
    /// <summary>
    /// Configures a required ChartOfAccountValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the ChartOfAccountValueObject property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "ExpenseAccount" will create "ExpenseAccountId", "ExpenseAccountCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureChartOfAccount(x => x.ExpenseAccount, "ExpenseAccount");
    /// builder.ConfigureChartOfAccount(x => x.RevenueAccount, "RevenueAccount");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureChartOfAccount<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, ChartOfAccountValueObject>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, account =>
        {
            account.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            account.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50)
                .IsRequired();

            account.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            account.Property(p => p.AccountType)
                .HasColumnName($"{columnNamePrefix}Type")
                .IsRequired();

            account.Property(p => p.NormalBalance)
                .HasColumnName($"{columnNamePrefix}NormalBalance")
                .IsRequired();

            account.Property(p => p.IsPostingAccount)
                .HasColumnName($"{columnNamePrefix}IsPostingAccount")
                .IsRequired();

            account.Property(p => p.IsContraAccount)
                .HasColumnName($"{columnNamePrefix}IsContraAccount")
                .IsRequired();

            account.Property(p => p.AccountCategoryName)
                .HasColumnName($"{columnNamePrefix}CategoryName")
                .HasMaxLength(200);
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional ChartOfAccountValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the ChartOfAccountValueObject? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "TaxAccount" will create "TaxAccountId", "TaxAccountCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalChartOfAccount(x => x.TaxAccount, "TaxAccount");
    /// builder.ConfigureOptionalChartOfAccount(x => x.OptionalAccount, "OptionalAccount");
    /// </code>
    /// </example>
    /// <remarks>
    /// Note: For optional properties, use ConfigureOptionalChartOfAccount instead of ConfigureChartOfAccount.
    /// C# cannot distinguish between Expression&lt;Func&lt;TEntity, ChartOfAccountValueObject&gt;&gt; and
    /// Expression&lt;Func&lt;TEntity, ChartOfAccountValueObject?&gt;&gt; for method overloading.
    /// </remarks>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalChartOfAccount<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, ChartOfAccountValueObject?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, account =>
        {
            account.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            account.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50)
                .IsRequired();

            account.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            account.Property(p => p.AccountType)
                .HasColumnName($"{columnNamePrefix}Type")
                .IsRequired();

            account.Property(p => p.NormalBalance)
                .HasColumnName($"{columnNamePrefix}NormalBalance")
                .IsRequired();

            account.Property(p => p.IsPostingAccount)
                .HasColumnName($"{columnNamePrefix}IsPostingAccount")
                .IsRequired();

            account.Property(p => p.IsContraAccount)
                .HasColumnName($"{columnNamePrefix}IsContraAccount")
                .IsRequired();

            account.Property(p => p.AccountCategoryName)
                .HasColumnName($"{columnNamePrefix}CategoryName")
                .HasMaxLength(200);
        });

        return builder;
    }
}
