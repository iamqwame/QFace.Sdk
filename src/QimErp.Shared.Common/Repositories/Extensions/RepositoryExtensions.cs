namespace QimErp.Shared.Common.Repositories.Extensions;

/// <summary>
/// Extension methods for repository operations.
/// Provides helper methods for common query patterns.
/// </summary>
public static class RepositoryExtensions
{
    /// <summary>
    /// Gets a paginated list of entities with optional search filtering.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing the list of entities and total count</returns>
    public static async Task<(List<TEntity> Items, int TotalCount)> GetPaginatedAsync<TEntity>(
        this IQueryable<TEntity> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    /// <summary>
    /// Gets a paginated list of entities with ordering.
    /// </summary>
    /// <typeparam name="TEntity">The entity type</typeparam>
    /// <typeparam name="TKey">The type to order by</typeparam>
    /// <param name="query">The queryable source</param>
    /// <param name="orderBy">The ordering expression</param>
    /// <param name="pageNumber">The page number (1-based)</param>
    /// <param name="pageSize">The number of items per page</param>
    /// <param name="ascending">Whether to sort ascending (default) or descending</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Tuple containing the list of entities and total count</returns>
    public static async Task<(List<TEntity> Items, int TotalCount)> GetPaginatedAsync<TEntity, TKey>(
        this IQueryable<TEntity> query,
        Expression<Func<TEntity, TKey>> orderBy,
        int pageNumber,
        int pageSize,
        bool ascending = true,
        CancellationToken cancellationToken = default)
        where TEntity : class
    {
        var orderedQuery = ascending
            ? query.OrderBy(orderBy)
            : query.OrderByDescending(orderBy);

        return await orderedQuery.GetPaginatedAsync(pageNumber, pageSize, cancellationToken);
    }

    // Note: EF Core already provides FirstOrDefaultAsync, AnyAsync, and ToListAsync with predicates
    // These extension methods are intentionally omitted to avoid ambiguity
}

