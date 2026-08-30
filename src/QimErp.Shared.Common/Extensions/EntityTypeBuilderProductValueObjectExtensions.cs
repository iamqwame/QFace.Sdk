namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring ProductValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderProductValueObjectExtensions
{
    /// <summary>
    /// Maps every column of ProductValueObject — all 10 properties (Id, Code, Name,
    /// Sku, ImageUrl, Category, CategoryId, Brand, Measurement, BarcodeGtin). None are
    /// marked required or max-length: these columns are retrofitted onto existing
    /// nullable/text columns downstream, so no constraints are added here.
    /// </summary>
    private static void MapColumns<TEntity>(
        OwnedNavigationBuilder<TEntity, ProductValueObject> product,
        string columnNamePrefix)
        where TEntity : class
    {
        product.Property(p => p.Id).HasColumnName($"{columnNamePrefix}Id");
        product.Property(p => p.Name).HasColumnName($"{columnNamePrefix}Name");
        product.Property(p => p.Code).HasColumnName($"{columnNamePrefix}Code");
        product.Property(p => p.Sku).HasColumnName($"{columnNamePrefix}Sku");
        product.Property(p => p.ImageUrl).HasColumnName($"{columnNamePrefix}ImageUrl");
        product.Property(p => p.Category).HasColumnName($"{columnNamePrefix}Category");
        product.Property(p => p.CategoryId).HasColumnName($"{columnNamePrefix}CategoryId");
        product.Property(p => p.Brand).HasColumnName($"{columnNamePrefix}Brand");
        product.Property(p => p.Measurement).HasColumnName($"{columnNamePrefix}Measurement");
        product.Property(p => p.BarcodeGtin).HasColumnName($"{columnNamePrefix}BarcodeGtin");
    }

    /// <summary>
    /// Configures a required ProductValueObject property as an owned entity
    /// with consistent column naming for all 10 columns.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureProduct<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, ProductValueObject>> navigationExpression,
        string columnNamePrefix = "Product",
        bool includeIndex = false)
        where TEntity : class
    {
#pragma warning disable CS8620
        builder.OwnsOne(navigationExpression, product =>
        {
            MapColumns(product, columnNamePrefix);
            if (includeIndex) product.HasIndex(p => p.Id);
        });
#pragma warning restore CS8620

        return builder;
    }

    /// <summary>
    /// Configures an optional ProductValueObject property as an owned entity
    /// with consistent column naming for all 10 columns.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalProduct<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, ProductValueObject?>> navigationExpression,
        string columnNamePrefix = "Product",
        bool includeIndex = false)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, product =>
        {
            MapColumns(product, columnNamePrefix);
            if (includeIndex) product.HasIndex(p => p.Id);
        });

        return builder;
    }
}
