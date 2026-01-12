namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring Quantity owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderQuantityExtensions
{
    /// <summary>
    /// Configures a required Quantity property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the Quantity property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "Quantity" will create "Quantity", "QuantityUnitOfMeasure").</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureQuantity(x => x.Quantity, "Quantity");
    /// builder.ConfigureQuantity(x => x.OrderQuantity, "OrderQuantity");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureQuantity<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, Quantity>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, quantity =>
        {
            quantity.Property(q => q.Amount)
                .IsRequired()
                .HasPrecision(18, 4)
                .HasColumnName(columnNamePrefix);

            quantity.Property(q => q.UnitOfMeasure)
                .HasMaxLength(20)
                .HasColumnName($"{columnNamePrefix}UnitOfMeasure");
        });

        return builder;
    }
}
