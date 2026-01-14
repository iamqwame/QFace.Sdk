namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring JobTitleValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderJobTitleValueObjectExtensions
{
    /// <summary>
    /// Configures a required JobTitleValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the JobTitleValueObject property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "JobTitle" will create "JobTitleId", "JobTitleCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureJobTitle(x => x.JobTitle, "JobTitle");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureJobTitle<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, JobTitleValueObject>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, jobTitle =>
        {
            jobTitle.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            jobTitle.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);

            jobTitle.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            jobTitle.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional JobTitleValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the JobTitleValueObject? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "JobTitle" will create "JobTitleId", "JobTitleCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalJobTitle(x => x.JobTitle, "JobTitle");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalJobTitle<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, JobTitleValueObject?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, jobTitle =>
        {
            jobTitle.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();
            
            jobTitle.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);
            
            jobTitle.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();
            
            jobTitle.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });
        
        return builder;
    }
}
