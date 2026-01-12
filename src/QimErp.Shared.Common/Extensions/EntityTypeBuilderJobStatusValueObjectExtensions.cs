using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using QimErp.Shared.Common.Entities.ValueObjects;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring JobStatusValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderJobStatusValueObjectExtensions
{
    /// <summary>
    /// Configures a required JobStatusValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the JobStatusValueObject property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "JobStatus" will create "JobStatusId", "JobStatusCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureJobStatus(x => x.JobStatus, "JobStatus");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureJobStatus<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, JobStatusValueObject>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, jobStatus =>
        {
            jobStatus.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            jobStatus.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);

            jobStatus.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            jobStatus.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional JobStatusValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the JobStatusValueObject? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "JobStatus" will create "JobStatusId", "JobStatusCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalJobStatus(x => x.JobStatus, "JobStatus");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalJobStatus<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, JobStatusValueObject?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, jobStatus =>
        {
            jobStatus.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();
            
            jobStatus.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);
            
            jobStatus.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();
            
            jobStatus.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });
        
        return builder;
    }
}
