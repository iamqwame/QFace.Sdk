namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring RankValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderRankValueObjectExtensions
{
    /// <summary>
    /// Configures a required RankValueObject property as an owned entity with standard column naming.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureRank<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, RankValueObject>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
#pragma warning disable CS8620
        builder.OwnsOne(navigationExpression, rank =>
        {
            rank.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            rank.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);

            rank.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();
        });
#pragma warning restore CS8620

        return builder;
    }

    /// <summary>
    /// Configures an optional RankValueObject property as an owned entity with standard column naming.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalRank<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, RankValueObject?>> navigationExpression,
        string columnNamePrefix)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, rank =>
        {
            rank.Property(p => p.Id)
                .HasColumnName($"{columnNamePrefix}Id")
                .IsRequired();

            rank.Property(p => p.Code)
                .HasColumnName($"{columnNamePrefix}Code")
                .HasMaxLength(50);

            rank.Property(p => p.Name)
                .HasColumnName($"{columnNamePrefix}Name")
                .HasMaxLength(200)
                .IsRequired();
        });

        return builder;
    }
}
