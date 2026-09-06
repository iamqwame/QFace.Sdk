using Npgsql.EntityFrameworkCore.PostgreSQL.Infrastructure;

namespace QimErp.Shared.Common.Extensions;

public static class NpgsqlDbContextOptionsBuilderExtensions
{
    // Pins EF's parameterized-collection translation to a single `= ANY(@p)` array parameter
    // (npgsql => npgsql.TranslateParameterizedCollectionsToParameters()) explicitly, so a future
    // EF/Npgsql default change can't silently poison the query plan cache per distinct collection.
    public static DbContextOptionsBuilder UseQimErpNpgsql(
        this DbContextOptionsBuilder optionsBuilder,
        string? connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        return optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.TranslateParameterizedCollectionsToParameters();
            npgsqlOptionsAction?.Invoke(npgsql);
        });
    }

    public static DbContextOptionsBuilder UseQimErpNpgsql(
        this DbContextOptionsBuilder optionsBuilder,
        NpgsqlDataSource dataSource,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null)
    {
        return optionsBuilder.UseNpgsql(dataSource, npgsql =>
        {
            npgsql.TranslateParameterizedCollectionsToParameters();
            npgsqlOptionsAction?.Invoke(npgsql);
        });
    }

    public static DbContextOptionsBuilder<TContext> UseQimErpNpgsql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        string? connectionString,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null) where TContext : DbContext
    {
        return optionsBuilder.UseNpgsql(connectionString, npgsql =>
        {
            npgsql.TranslateParameterizedCollectionsToParameters();
            npgsqlOptionsAction?.Invoke(npgsql);
        });
    }

    public static DbContextOptionsBuilder<TContext> UseQimErpNpgsql<TContext>(
        this DbContextOptionsBuilder<TContext> optionsBuilder,
        NpgsqlDataSource dataSource,
        Action<NpgsqlDbContextOptionsBuilder>? npgsqlOptionsAction = null) where TContext : DbContext
    {
        return optionsBuilder.UseNpgsql(dataSource, npgsql =>
        {
            npgsql.TranslateParameterizedCollectionsToParameters();
            npgsqlOptionsAction?.Invoke(npgsql);
        });
    }
}
