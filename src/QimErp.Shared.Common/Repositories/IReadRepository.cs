namespace QimErp.Shared.Common.Repositories;

/// <summary>
/// Read-only repository interface for query operations only.
/// Use this when you need to enforce read-only access to data.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The key type (Guid, string, int, etc.)</typeparam>
public interface IReadRepository<TEntity, TKey>
    where TEntity : class
{
    /// <summary>
    /// Gets an entity by its primary key.
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="isTracking">If false, uses AsNoTracking for better read performance. Default is true.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The entity if found, otherwise null</returns>
    Task<TEntity?> GetByIdAsync(TKey id, bool isTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets all entities as a queryable for complex queries.
    /// </summary>
    /// <param name="isTracking">If false, uses AsNoTracking for better read performance. Default is true.</param>
    /// <returns>Queryable collection of entities</returns>
    IQueryable<TEntity> GetQueryable(bool isTracking = true);
    
    /// <summary>
    /// Gets all entities.
    /// </summary>
    /// <param name="isTracking">If false, uses AsNoTracking for better read performance. Default is true.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of all entities</returns>
    Task<List<TEntity>> GetAllAsync(bool isTracking = true, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Checks if an entity exists by its primary key.
    /// </summary>
    /// <param name="id">The primary key value</param>
    /// <param name="isTracking">If false, uses AsNoTracking for better read performance. Default is true.</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if entity exists, otherwise false</returns>
    Task<bool> ExistsAsync(TKey id, bool isTracking = true, CancellationToken cancellationToken = default);
}

