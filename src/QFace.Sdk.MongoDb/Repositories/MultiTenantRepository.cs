using System.Linq.Expressions;

namespace QFace.Sdk.MongoDb.Repositories;

/// <summary>
/// Base class for multi-tenant repositories
/// </summary>
/// <typeparam name="TDocument">The document type</typeparam>
public class MultiTenantRepository<TDocument> : IMongoRepository<TDocument> where TDocument : BaseDocument
{
    private readonly IMongoDbProvider _dbProvider;
    protected readonly ILogger<MultiTenantRepository<TDocument>> _logger;
    protected readonly string _collectionName;
    protected readonly string _tenantId;
    protected readonly string _databaseNameFormat;

    /// <summary>
    /// Gets the collection for the current tenant
    /// </summary>
    protected IMongoCollection<TDocument> Collection =>
        _dbProvider.GetTenantCollection<TDocument>(_tenantId, _collectionName, _databaseNameFormat);

    /// <summary>
    /// Gets the name of the collection
    /// </summary>
    public string CollectionName => _collectionName;

    /// <summary>
    /// Creates a new multi-tenant repository
    /// </summary>
    /// <param name="dbProvider">The MongoDB provider</param>
    /// <param name="collectionName">The collection name</param>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="logger">The logger</param>
    /// <param name="databaseNameFormat">The database name format</param>
    public MultiTenantRepository(
        IMongoDbProvider dbProvider,
        string collectionName,
        string tenantId,
        ILogger<MultiTenantRepository<TDocument>> logger,
        string databaseNameFormat = "{0}_db")
    {
        _dbProvider = dbProvider;
        _collectionName = collectionName;
        _tenantId = tenantId;
        _logger = logger;
        _databaseNameFormat = databaseNameFormat;

        // Create indexes on initialization
        try
        {
            CreateIndexesAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating indexes for tenant {TenantId}, collection {CollectionName}",
                _tenantId, _collectionName);
        }
    }

    /// <summary>
    /// Gets all documents in the collection
    /// </summary>
    public virtual async Task<IEnumerable<TDocument>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = includeInactive
                ? Builders<TDocument>.Filter.Empty
                : Builders<TDocument>.Filter.Eq(doc => doc.IsActive, true);

            return await Collection.Find(filter).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all documents from tenant {TenantId}, collection {CollectionName}",
                _tenantId, _collectionName);
            return Enumerable.Empty<TDocument>();
        }
    }

    /// <summary>
    /// Gets a document by its ID
    /// </summary>
    public virtual async Task<TDocument> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {Id} from tenant {TenantId}, collection {CollectionName}",
                id, _tenantId, _collectionName);
            return null;
        }
    }

    /// <summary>
    /// Finds a single document matching the filter expression
    /// </summary>
    public virtual async Task<TDocument> FindOneAsync(
        Expression<Func<TDocument, bool>> filterExpression,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = includeInactive
                ? filterExpression
                : Builders<TDocument>.Filter.And(
                    Builders<TDocument>.Filter.Where(filterExpression),
                    Builders<TDocument>.Filter.Eq(doc => doc.IsActive, true));

            return await Collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding document in tenant {TenantId}, collection {CollectionName}",
                _tenantId, _collectionName);
            return null;
        }
    }

    /// <summary>
    /// Finds all documents matching the filter expression
    /// </summary>
    public virtual async Task<IEnumerable<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filterExpression,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = includeInactive
                ? filterExpression
                : Builders<TDocument>.Filter.And(
                    Builders<TDocument>.Filter.Where(filterExpression),
                    Builders<TDocument>.Filter.Eq(doc => doc.IsActive, true));

            return await Collection.Find(filter).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding documents in tenant {TenantId}, collection {CollectionName}",
                _tenantId, _collectionName);
            return Enumerable.Empty<TDocument>();
        }
    }

    /// <summary>
    /// Inserts a document
    /// </summary>
    public virtual async Task InsertOneAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            // Set audit fields
            document.CreatedDate = DateTime.UtcNow;
            document.LastModifiedDate = DateTime.UtcNow;

            await Collection.InsertOneAsync(document, null, cancellationToken);
            _logger.LogInformation("Document {Id} inserted into tenant {TenantId}, collection {CollectionName}",
                document.Id, _tenantId, _collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting document into tenant {TenantId}, collection {CollectionName}",
                _tenantId, _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Inserts multiple documents
    /// </summary>
    public virtual async Task InsertManyAsync(
        IEnumerable<TDocument> documents,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Set audit fields
            var now = DateTime.UtcNow;
            foreach (var document in documents)
            {
                document.CreatedDate = now;
                document.LastModifiedDate = now;
            }

            await Collection.InsertManyAsync(documents, null, cancellationToken);
            _logger.LogInformation("Multiple documents inserted into tenant {TenantId}, collection {CollectionName}",
                _tenantId, _collectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Error inserting multiple documents into tenant {TenantId}, collection {CollectionName}",
                _tenantId, _collectionName);
            throw;
        }
    }

    /// <summary>
    /// Updates a document
    /// </summary>
    public virtual async Task<bool> UpdateAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            // Update audit field
            document.LastModifiedDate = DateTime.UtcNow;

            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            var result = await Collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = false },
                cancellationToken);

            var success = result.IsAcknowledged && result.ModifiedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} updated in tenant {TenantId}, collection {CollectionName}",
                    document.Id, _tenantId, _collectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not updated in tenant {TenantId}, collection {CollectionName}",
                    document.Id, _tenantId, _collectionName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {Id} in tenant {TenantId}, collection {CollectionName}",
                document.Id, _tenantId, _collectionName);
            return false;
        }
    }

    /// <summary>
    /// Replaces a document
    /// </summary>
    public virtual async Task<bool> ReplaceOneAsync(TDocument document, CancellationToken cancellationToken = default)
    {
        try
        {
            // Update audit field
            document.LastModifiedDate = DateTime.UtcNow;

            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, document.Id);
            var result = await Collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true },
                cancellationToken);

            var success = result.IsAcknowledged && (result.ModifiedCount > 0 || result.UpsertedId != null);
            if (success)
            {
                _logger.LogInformation("Document {Id} replaced in tenant {TenantId}, collection {CollectionName}",
                    document.Id, _tenantId, _collectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not replaced in tenant {TenantId}, collection {CollectionName}",
                    document.Id, _tenantId, _collectionName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replacing document {Id} in tenant {TenantId}, collection {CollectionName}",
                document.Id, _tenantId, _collectionName);
            return false;
        }
    }

    /// <summary>
    /// Deletes a document by ID (hard delete)
    /// </summary>
    public virtual async Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            var result = await Collection.DeleteOneAsync(filter, cancellationToken);

            var success = result.IsAcknowledged && result.DeletedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} deleted from tenant {TenantId}, collection {CollectionName}",
                    id, _tenantId, _collectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not deleted from tenant {TenantId}, collection {CollectionName}",
                    id, _tenantId, _collectionName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id} from tenant {TenantId}, collection {CollectionName}",
                id, _tenantId, _collectionName);
            return false;
        }
    }

    /// <summary>
    /// Soft deletes a document by ID (sets IsActive to false)
    /// </summary>
    public virtual async Task<bool> SoftDeleteByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            var update = Builders<TDocument>.Update
                .Set(doc => doc.IsActive, false)
                .Set(doc => doc.LastModifiedDate, DateTime.UtcNow);

            var result = await Collection.UpdateOneAsync(filter, update, null, cancellationToken);

            var success = result.IsAcknowledged && result.ModifiedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} soft-deleted in tenant {TenantId}, collection {CollectionName}",
                    id, _tenantId, _collectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not soft-deleted in tenant {TenantId}, collection {CollectionName}",
                    id, _tenantId, _collectionName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft-deleting document {Id} in tenant {TenantId}, collection {CollectionName}",
                id, _tenantId, _collectionName);
            return false;
        }
    }

    /// <summary>
    /// Restores a soft-deleted document
    /// </summary>
    public virtual async Task<bool> RestoreByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<TDocument>.Filter.Eq(doc => doc.Id, id);
            var update = Builders<TDocument>.Update
                .Set(doc => doc.IsActive, true)
                .Set(doc => doc.LastModifiedDate, DateTime.UtcNow);

            var result = await Collection.UpdateOneAsync(filter, update, null, cancellationToken);

            var success = result.IsAcknowledged && result.ModifiedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} restored in tenant {TenantId}, collection {CollectionName}",
                    id, _tenantId, _collectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not restored in tenant {TenantId}, collection {CollectionName}",
                    id, _tenantId, _collectionName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring document {Id} in tenant {TenantId}, collection {CollectionName}",
                id, _tenantId, _collectionName);
            return false;
        }
    }

    /// <summary>
    /// Creates indexes for the collection
    /// </summary>
    public virtual Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        // Base implementation creates a text index on all fields
        // Override this in derived classes for specific indexes
        var indexModel = new CreateIndexModel<TDocument>(
            Builders<TDocument>.IndexKeys.Text("$**"),
            new CreateIndexOptions { Background = true }
        );

        return Collection.Indexes.CreateOneAsync(indexModel, null, cancellationToken);
    }
}