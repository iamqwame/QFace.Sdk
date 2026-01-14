namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring StationValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderStationValueObjectExtensions
{
    /// <summary>
    /// Configures a required StationValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the StationValueObject property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "Station" will create "StationId", "StationCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureStation(x => x.Station, "Station");
    /// builder.ConfigureStation(x => x.Location, "Location");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureStation<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, StationValueObject>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, station =>
        {
            station.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            station.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);

            station.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional StationValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the StationValueObject? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "Station" will create "StationId", "StationCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalStation(x => x.Station, "Station");
    /// builder.ConfigureOptionalStation(x => x.Location, "Location");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalStation<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, StationValueObject?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, station =>
        {
            station.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();
            
            station.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);
            
            station.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();
        });
        
        return builder;
    }
}
