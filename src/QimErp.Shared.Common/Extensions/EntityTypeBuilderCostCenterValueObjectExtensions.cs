using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;
using QimErp.Shared.Common.Entities.ValueObjects;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring CostCenterValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderCostCenterValueObjectExtensions
{
    /// <summary>
    /// Configures a required CostCenterValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the CostCenterValueObject property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "CostCenter" will create "CostCenterId", "CostCenterCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureCostCenter(x => x.CostCenter, "CostCenter");
    /// builder.ConfigureCostCenter(x => x.AllocationCostCenter, "AllocationCostCenter");
    /// </code>
    /// </example>
    public static EntityTypeBuilder<TEntity> ConfigureCostCenter<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, CostCenterValueObject>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, costCenter =>
        {
            costCenter.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            costCenter.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50)
                .IsRequired();

            costCenter.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            costCenter.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });

        return builder;
    }

    /// <summary>
    /// Configures an optional CostCenterValueObject property as an owned entity with standard column naming.
    /// </summary>
    /// <typeparam name="TEntity">The entity type.</typeparam>
    /// <param name="builder">The entity type builder.</param>
    /// <param name="navigationExpression">Expression selecting the CostCenterValueObject? property.</param>
    /// <param name="columnNamePrefix">Prefix for column names (e.g., "CostCenter" will create "CostCenterId", "CostCenterCode", etc.).</param>
    /// <returns>The entity type builder for chaining.</returns>
    /// <example>
    /// <code>
    /// builder.ConfigureOptionalCostCenter(x => x.CostCenter, "CostCenter");
    /// builder.ConfigureOptionalCostCenter(x => x.AllocationCostCenter, "AllocationCostCenter");
    /// </code>
    /// </example>
    /// <remarks>
    /// Note: For optional properties, use ConfigureOptionalCostCenter instead of ConfigureCostCenter.
    /// C# cannot distinguish between Expression&lt;Func&lt;TEntity, CostCenterValueObject&gt;&gt; and
    /// Expression&lt;Func&lt;TEntity, CostCenterValueObject?&gt;&gt; for method overloading.
    /// </remarks>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalCostCenter<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, CostCenterValueObject?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, costCenter =>
        {
            costCenter.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            costCenter.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50)
                .IsRequired();

            costCenter.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();

            costCenter.Property(p => p.Description)
                .HasColumnName($"{columnNamePrefix}Description")
                .HasMaxLength(500);
        });

        return builder;
    }
}
