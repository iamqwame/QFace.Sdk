using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using QimErp.Shared.Common.Entities.ValueObjects;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring QuarterlyAmounts owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderQuarterlyAmountsExtensions
{
    /// <summary>
    /// Configures a QuarterlyAmounts property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the QuarterlyAmounts? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "Quarterly" will create "QuarterlyQ1Amount", "QuarterlyQ2Amount", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureQuarterlyAmounts(x => x.QuarterlyAmounts, "Quarterly");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureQuarterlyAmounts<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, QuarterlyAmounts?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        var ownsOne = builder.OwnsOne(navigationExpression, quarterly =>
        {
            // Configure Q1 - mark Amount as required to identify entity existence
            quarterly.OwnsOne(q => q.Q1, q1Money =>
            {
                q1Money.Property(m => m.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q1Amount");

                q1Money.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}Q1AmountCurrencyCode");

                q1Money.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}Q1AmountExchangeRate");

                q1Money.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q1AmountBaseCurrencyAmount");
            });

            // Configure Q2
            quarterly.OwnsOne(q => q.Q2, q2Money =>
            {
                q2Money.Property(m => m.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q2Amount");

                q2Money.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}Q2AmountCurrencyCode");

                q2Money.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}Q2AmountExchangeRate");

                q2Money.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q2AmountBaseCurrencyAmount");
            });

            // Configure Q3
            quarterly.OwnsOne(q => q.Q3, q3Money =>
            {
                q3Money.Property(m => m.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q3Amount");

                q3Money.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}Q3AmountCurrencyCode");

                q3Money.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}Q3AmountExchangeRate");

                q3Money.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q3AmountBaseCurrencyAmount");
            });

            // Configure Q4
            quarterly.OwnsOne(q => q.Q4, q4Money =>
            {
                q4Money.Property(m => m.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q4Amount");

                q4Money.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}Q4AmountCurrencyCode");

                q4Money.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}Q4AmountExchangeRate");

                q4Money.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}Q4AmountBaseCurrencyAmount");
            });
        });
        
        return builder;
    }
}
