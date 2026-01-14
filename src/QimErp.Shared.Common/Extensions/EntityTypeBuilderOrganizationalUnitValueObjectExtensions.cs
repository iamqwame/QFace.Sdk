namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring OrganizationalUnitValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderOrganizationalUnitValueObjectExtensions
{
    /// <summary>
    /// Configures a required OrganizationalUnitValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the OrganizationalUnitValueObject property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "OrganizationalUnit" will create "OrganizationalUnitId", "OrganizationalUnitCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOrganizationalUnit(x => x.OrganizationalUnit, "OrganizationalUnit");
    /// builder.ConfigureOrganizationalUnit(x => x.Department, "Department");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureOrganizationalUnit<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, OrganizationalUnitValueObject>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, orgUnit =>
        {
            orgUnit.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            orgUnit.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);

            orgUnit.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            orgUnit.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional OrganizationalUnitValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the OrganizationalUnitValueObject? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "OrganizationalUnit" will create "OrganizationalUnitId", "OrganizationalUnitCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalOrganizationalUnit(x => x.OrganizationalUnit, "OrganizationalUnit");
    /// </code>
    /// </example>
    /// <remarks>
    /// Note: For optional properties, use ConfigureOptionalOrganizationalUnit instead of ConfigureOrganizationalUnit.
    /// C# cannot distinguish between Expression&lt;Func&lt;TEntity, OrganizationalUnitValueObject&gt;&gt; and
    /// Expression&lt;Func&lt;TEntity, OrganizationalUnitValueObject?&gt;&gt; for method overloading.
    /// </remarks>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalOrganizationalUnit<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, OrganizationalUnitValueObject?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, orgUnit =>
        {
            orgUnit.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            orgUnit.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);

            orgUnit.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            orgUnit.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });

        return builder;
    }
}
