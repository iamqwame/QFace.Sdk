namespace QimErp.Shared.Common.Extensions;

public static class EntityTypeBuilderCustomerValueObjectExtensions
{
    private static void MapColumns<TEntity>(
        OwnedNavigationBuilder<TEntity, CustomerValueObject> customer,
        string columnNamePrefix)
        where TEntity : class
    {
        customer.Property(p => p.Id).HasColumnName($"{columnNamePrefix}Id");
        customer.Property(p => p.Code).HasColumnName($"{columnNamePrefix}Code");
        customer.Property(p => p.Name).HasColumnName($"{columnNamePrefix}Name");
    }

    public static EntityTypeBuilder<TEntity> ConfigureCustomer<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, CustomerValueObject>> navigationExpression,
        string columnNamePrefix = "Customer",
        bool includeIndex = false)
        where TEntity : class
    {
#pragma warning disable CS8620
        builder.OwnsOne(navigationExpression, customer =>
        {
            MapColumns(customer, columnNamePrefix);
            if (includeIndex) customer.HasIndex(p => p.Id);
        });
#pragma warning restore CS8620

        return builder;
    }

    public static EntityTypeBuilder<TEntity> ConfigureOptionalCustomer<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, CustomerValueObject?>> navigationExpression,
        string columnNamePrefix = "Customer",
        bool includeIndex = false)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, customer =>
        {
            MapColumns(customer, columnNamePrefix);
            if (includeIndex) customer.HasIndex(p => p.Id);
        });

        return builder;
    }
}
