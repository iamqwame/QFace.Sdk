namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring Money owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderMoneyExtensions
{
    /// <summary>
    /// Configures a required Money property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the Money property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "BudgetAmount" will create "BudgetAmount", "BudgetAmountCurrencyCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureMoney(x => x.BudgetAmount, "BudgetAmount");
    /// builder.ConfigureMoney(x => x.TotalCost, "TotalCost");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureMoney<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Money>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnName(columnNamePrefix);

            money.Property(m => m.CurrencyCode)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("GHS")
                .HasColumnName($"{columnNamePrefix}CurrencyCode");

            money.Property(m => m.ExchangeRate)
                .IsRequired()
                .HasPrecision(18, 6)
                .HasDefaultValue(1.0m)
                .HasColumnName($"{columnNamePrefix}ExchangeRate");

            money.Property(m => m.BaseCurrencyAmount)
                .HasPrecision(18, 2)
                .HasColumnName($"{columnNamePrefix}BaseCurrencyAmount");
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional Money property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the Money? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "DebitAmount" will create "DebitAmount", "DebitAmountCurrencyCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalMoney(x => x.DebitAmount, "DebitAmount");
    /// builder.ConfigureOptionalMoney(x => x.TaxAmount, "TaxAmount");
    /// </code>
    /// </example>
    /// <remarks>
    /// Note: For optional properties, use ConfigureOptionalMoney instead of ConfigureMoney.
    /// C# cannot distinguish between Expression&lt;Func&lt;TEntity, Money&gt;&gt; and
    /// Expression&lt;Func&lt;TEntity, Money?&gt;&gt; for method overloading.
    /// </remarks>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalMoney<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Money?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, money =>
        {
            money.Property(m => m.Amount)
                .IsRequired()
                .HasPrecision(18, 2)
                .HasColumnName(columnNamePrefix);

            money.Property(m => m.CurrencyCode)
                .IsRequired()
                .HasMaxLength(10)
                .HasDefaultValue("GHS")
                .HasColumnName($"{columnNamePrefix}CurrencyCode");

            money.Property(m => m.ExchangeRate)
                .IsRequired()
                .HasPrecision(18, 6)
                .HasDefaultValue(1.0m)
                .HasColumnName($"{columnNamePrefix}ExchangeRate");

            money.Property(m => m.BaseCurrencyAmount)
                .HasPrecision(18, 2)
                .HasColumnName($"{columnNamePrefix}BaseCurrencyAmount");
        });

        return builder;
    }
}
