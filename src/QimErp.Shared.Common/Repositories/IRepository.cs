namespace QimErp.Shared.Common.Repositories;

/// <summary>
/// Generic repository interface for common CRUD operations.
/// Provides a consistent pattern for data access across all modules.
/// Combines read and write operations in a single interface.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The key type (Guid, string, int, etc.)</typeparam>
public interface IRepository<TEntity, TKey> : IReadRepository<TEntity, TKey>, IWriteRepository<TEntity>
    where TEntity : class
{
    // All methods are inherited from IReadRepository<TEntity, TKey> and IWriteRepository<TEntity>
}

