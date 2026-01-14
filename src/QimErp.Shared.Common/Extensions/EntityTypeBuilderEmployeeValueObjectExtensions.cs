namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring EmployeeValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderEmployeeValueObjectExtensions
{
    /// <summary>
    /// Configures a required EmployeeValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the EmployeeValueObject property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "Employee" will create "EmployeeId", "EmployeeName", etc.). Defaults to "Employee".</param>
    /// <param name="includeIndex">Whether to create an index on the EmployeeId column. Defaults to false.</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureEmployee(x => x.Employee);
    /// builder.ConfigureEmployee(x => x.CreatedBy, "CreatedBy", includeIndex: true);
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureEmployee<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, EmployeeValueObject>> navigationExpression,
        string columnNamePrefix = "Employee",
        bool includeIndex = false)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, employee =>
        {
            employee.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            employee.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            employee.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50)
                .IsRequired();

            employee.Property(p => p.Email)
                .HasColumnName($"{columnNamePrefix}Email")
                .HasMaxLength(255);

            employee.Property(p => p.Picture)
                .HasColumnName($"{columnNamePrefix}ProfilePicture")
                .HasMaxLength(500);

            if (includeIndex)
            {
                employee.HasIndex(p => p.Id);
            }
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional EmployeeValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the EmployeeValueObject? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "Employee" will create "EmployeeId", "EmployeeName", etc.). Defaults to "Employee".</param>
    /// <param name="includeIndex">Whether to create an index on the EmployeeId column. Defaults to false.</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalEmployee(x => x.Supervisor, "Supervisor");
    /// builder.ConfigureOptionalEmployee(x => x.Manager, "Manager", includeIndex: true);
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalEmployee<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, EmployeeValueObject?>> navigationExpression,
        string columnNamePrefix = "Employee",
        bool includeIndex = false)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, employee =>
        {
            employee.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();
            
            employee.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();
            
            employee.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50)
                .IsRequired();
            
            employee.Property(p => p.Email)
                .HasColumnName($"{columnNamePrefix}Email")
                .HasMaxLength(255);
            
            employee.Property(p => p.Picture)
                .HasColumnName($"{columnNamePrefix}ProfilePicture")
                .HasMaxLength(500);
            
            if (includeIndex)
            {
                employee.HasIndex(p => p.Id);
            }
        });
        
        return builder;
    }
}
