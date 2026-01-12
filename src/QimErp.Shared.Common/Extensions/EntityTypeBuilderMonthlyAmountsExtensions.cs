using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using QimErp.Shared.Common.Entities.ValueObjects;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring MonthlyAmounts owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderMonthlyAmountsExtensions
{
    /// <summary>
    /// Configures a MonthlyAmounts property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the MonthlyAmounts? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "Monthly" will create "MonthlyJanuary", "MonthlyFebruary", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureMonthlyAmounts(x => x.MonthlyAmounts, "Monthly");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureMonthlyAmounts<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, MonthlyAmounts?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        var ownsOne = builder.OwnsOne(navigationExpression, monthly =>
        {
            // Configure January - mark Amount as required to identify entity existence
            monthly.OwnsOne(m => m.January, janMoney =>
            {
                janMoney.Property(m => m.Amount)
                    .IsRequired()
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}January");

                janMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}JanuaryCurrencyCode");

                janMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}JanuaryExchangeRate");

                janMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}JanuaryBaseCurrencyAmount");
            });

            // Configure February
            monthly.OwnsOne(m => m.February, febMoney =>
            {
                febMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}February");

                febMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}FebruaryCurrencyCode");

                febMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}FebruaryExchangeRate");

                febMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}FebruaryBaseCurrencyAmount");
            });

            // Configure March
            monthly.OwnsOne(m => m.March, marMoney =>
            {
                marMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}March");

                marMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}MarchCurrencyCode");

                marMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}MarchExchangeRate");

                marMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}MarchBaseCurrencyAmount");
            });

            // Configure April
            monthly.OwnsOne(m => m.April, aprMoney =>
            {
                aprMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}April");

                aprMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}AprilCurrencyCode");

                aprMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}AprilExchangeRate");

                aprMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}AprilBaseCurrencyAmount");
            });

            // Configure May
            monthly.OwnsOne(m => m.May, mayMoney =>
            {
                mayMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}May");

                mayMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}MayCurrencyCode");

                mayMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}MayExchangeRate");

                mayMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}MayBaseCurrencyAmount");
            });

            // Configure June
            monthly.OwnsOne(m => m.June, junMoney =>
            {
                junMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}June");

                junMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}JuneCurrencyCode");

                junMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}JuneExchangeRate");

                junMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}JuneBaseCurrencyAmount");
            });

            // Configure July
            monthly.OwnsOne(m => m.July, julMoney =>
            {
                julMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}July");

                julMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}JulyCurrencyCode");

                julMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}JulyExchangeRate");

                julMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}JulyBaseCurrencyAmount");
            });

            // Configure August
            monthly.OwnsOne(m => m.August, augMoney =>
            {
                augMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}August");

                augMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}AugustCurrencyCode");

                augMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}AugustExchangeRate");

                augMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}AugustBaseCurrencyAmount");
            });

            // Configure September
            monthly.OwnsOne(m => m.September, sepMoney =>
            {
                sepMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}September");

                sepMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}SeptemberCurrencyCode");

                sepMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}SeptemberExchangeRate");

                sepMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}SeptemberBaseCurrencyAmount");
            });

            // Configure October
            monthly.OwnsOne(m => m.October, octMoney =>
            {
                octMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}October");

                octMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}OctoberCurrencyCode");

                octMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}OctoberExchangeRate");

                octMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}OctoberBaseCurrencyAmount");
            });

            // Configure November
            monthly.OwnsOne(m => m.November, novMoney =>
            {
                novMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}November");

                novMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}NovemberCurrencyCode");

                novMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}NovemberExchangeRate");

                novMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}NovemberBaseCurrencyAmount");
            });

            // Configure December
            monthly.OwnsOne(m => m.December, decMoney =>
            {
                decMoney.Property(m => m.Amount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}December");

                decMoney.Property(m => m.CurrencyCode)
                    .HasMaxLength(10)
                    .HasDefaultValue("GHS")
                    .HasColumnName($"{columnNamePrefix}DecemberCurrencyCode");

                decMoney.Property(m => m.ExchangeRate)
                    .HasPrecision(18, 6)
                    .HasDefaultValue(1.0m)
                    .HasColumnName($"{columnNamePrefix}DecemberExchangeRate");

                decMoney.Property(m => m.BaseCurrencyAmount)
                    .HasPrecision(18, 2)
                    .HasColumnName($"{columnNamePrefix}DecemberBaseCurrencyAmount");
            });
        });
        
        return builder;
    }
}
