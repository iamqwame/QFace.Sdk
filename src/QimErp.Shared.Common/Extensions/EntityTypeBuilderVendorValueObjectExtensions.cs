namespace QimErp.Shared.Common.Extensions;

public static class EntityTypeBuilderVendorValueObjectExtensions
{
    private static void MapColumns<TEntity>(
        OwnedNavigationBuilder<TEntity, VendorValueObject> vendor,
        string columnNamePrefix)
        where TEntity : class
    {
        vendor.Property(p => p.Id).HasColumnName($"{columnNamePrefix}Id");
        vendor.Property(p => p.Code).HasColumnName($"{columnNamePrefix}Code");
        vendor.Property(p => p.Name).HasColumnName($"{columnNamePrefix}Name");
    }

    public static EntityTypeBuilder<TEntity> ConfigureVendor<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, VendorValueObject>> navigationExpression,
        string columnNamePrefix = "Vendor",
        bool includeIndex = false)
        where TEntity : class
    {
#pragma warning disable CS8620
        builder.OwnsOne(navigationExpression, vendor =>
        {
            MapColumns(vendor, columnNamePrefix);
            if (includeIndex) vendor.HasIndex(p => p.Id);
        });
#pragma warning restore CS8620

        return builder;
    }

    public static EntityTypeBuilder<TEntity> ConfigureOptionalVendor<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, VendorValueObject?>> navigationExpression,
        string columnNamePrefix = "Vendor",
        bool includeIndex = false)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, vendor =>
        {
            MapColumns(vendor, columnNamePrefix);
            if (includeIndex) vendor.HasIndex(p => p.Id);
        });

        return builder;
    }
}
