namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for configuring EmployeeValueObject owned entities in Entity Framework Core.
/// </summary>
public static class EntityTypeBuilderEmployeeValueObjectExtensions
{
    /// <summary>
    /// Maps every column of EmployeeValueObject — the slim 5 (Id, Name, Code,
    /// Email, ProfilePicture) plus the eight denormalised job-info columns
    /// (JobTitle, JobTitleCode, OrganizationalUnit, OrganizationalUnitCode,
    /// Station, StationCode, Rank, RankCode). The eight extras have no max
    /// length — open-ended PostgreSQL `text`.
    /// </summary>
    private static void MapColumns<TEntity>(
        OwnedNavigationBuilder<TEntity, EmployeeValueObject> employee,
        string columnNamePrefix)
        where TEntity : class
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

        employee.Property(p => p.JobTitle).HasColumnName($"{columnNamePrefix}JobTitle");
        employee.Property(p => p.JobTitleCode).HasColumnName($"{columnNamePrefix}JobTitleCode");
        employee.Property(p => p.OrganizationalUnit).HasColumnName($"{columnNamePrefix}OrganizationalUnit");
        employee.Property(p => p.OrganizationalUnitCode).HasColumnName($"{columnNamePrefix}OrganizationalUnitCode");
        employee.Property(p => p.Station).HasColumnName($"{columnNamePrefix}Station");
        employee.Property(p => p.StationCode).HasColumnName($"{columnNamePrefix}StationCode");
        employee.Property(p => p.Rank).HasColumnName($"{columnNamePrefix}Rank");
        employee.Property(p => p.RankCode).HasColumnName($"{columnNamePrefix}RankCode");
    }

    /// <summary>
    /// Configures a required EmployeeValueObject property as an owned entity
    /// with consistent column naming for all 13 columns (5 identity +
    /// 8 denormalised job/org/station/rank fields).
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureEmployee<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, EmployeeValueObject>> navigationExpression,
        string columnNamePrefix = "Employee",
        bool includeIndex = false)
        where TEntity : class
    {
#pragma warning disable CS8620
        builder.OwnsOne(navigationExpression, employee =>
        {
            MapColumns(employee, columnNamePrefix);
            if (includeIndex) employee.HasIndex(p => p.Id);
        });
#pragma warning restore CS8620

        return builder;
    }

    /// <summary>
    /// Configures an optional EmployeeValueObject property as an owned entity
    /// with consistent column naming for all 13 columns.
    /// </summary>
    public static EntityTypeBuilder<TEntity> ConfigureOptionalEmployee<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, EmployeeValueObject?>> navigationExpression,
        string columnNamePrefix = "Employee",
        bool includeIndex = false)
        where TEntity : class
    {
        builder.OwnsOne(navigationExpression, employee =>
        {
            MapColumns(employee, columnNamePrefix);
            if (includeIndex) employee.HasIndex(p => p.Id);
        });

        return builder;
    }
}
