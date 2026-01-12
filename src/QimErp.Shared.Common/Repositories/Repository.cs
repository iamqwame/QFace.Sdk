namespace QimErp.Shared.Common.Repositories;

/// <summary>
/// Generic repository implementation for common CRUD operations.
/// Provides base functionality that can be extended by entity-specific repositories.
/// </summary>
/// <typeparam name="TEntity">The entity type</typeparam>
/// <typeparam name="TKey">The key type (Guid, string, int, etc.)</typeparam>
/// <typeparam name="TDbContext">The DbContext type</typeparam>
public abstract class Repository<TEntity, TKey, TDbContext> : IRepository<TEntity, TKey>
    where TEntity : class
    where TDbContext : DbContext
{
    protected readonly TDbContext Context;
    protected readonly ILogger Logger;

    protected Repository(TDbContext context, ILogger logger)
    {
        Context = context ?? throw new ArgumentNullException(nameof(context));
        Logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Gets the DbSet for the entity. Must be implemented by derived classes.
    /// </summary>
    protected abstract DbSet<TEntity> DbSet { get; }

    /// <summary>
    /// Gets an expression to access the entity's primary key property.
    /// Must be implemented by derived classes.
    /// </summary>
    protected abstract Expression<Func<TEntity, TKey>> KeySelector { get; }

    public virtual async Task<TEntity?> GetByIdAsync(TKey id, bool isTracking = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(EntityIdEquals(id));
        
        if (!isTracking)
        {
            query = query.AsNoTracking();
        }
        
        return await query.FirstOrDefaultAsync(cancellationToken);
    }

    public virtual IQueryable<TEntity> GetQueryable(bool isTracking = true)
    {
        var query = DbSet.AsQueryable();
        
        if (!isTracking)
        {
            query = query.AsNoTracking();
        }
        
        return query;
    }

    public virtual async Task<List<TEntity>> GetAllAsync(bool isTracking = true, CancellationToken cancellationToken = default)
    {
        var query = GetQueryable(isTracking);
        return await query.ToListAsync(cancellationToken);
    }

    public virtual async Task<bool> ExistsAsync(TKey id, bool isTracking = true, CancellationToken cancellationToken = default)
    {
        var query = DbSet.Where(EntityIdEquals(id));
        
        if (!isTracking)
        {
            query = query.AsNoTracking();
        }
        
        return await query.AnyAsync(cancellationToken);
    }

    public virtual async Task AddAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        await DbSet.AddAsync(entity, cancellationToken);
    }

    public virtual async Task UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Update(entity);
        await Task.CompletedTask;
    }

    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        DbSet.Remove(entity);
        await Task.CompletedTask;
    }

    public virtual async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await Context.SaveChangesAsync(cancellationToken);
    }

    /// <summary>
    /// Creates an expression to check if entity's key equals the provided id.
    /// </summary>
    private Expression<Func<TEntity, bool>> EntityIdEquals(TKey id)
    {
        var parameter = Expression.Parameter(typeof(TEntity), "e");
        var property = Expression.Invoke(KeySelector, parameter);
        var constant = Expression.Constant(id, typeof(TKey));
        var equals = Expression.Equal(property, constant);
        return Expression.Lambda<Func<TEntity, bool>>(equals, parameter);
    }
}

