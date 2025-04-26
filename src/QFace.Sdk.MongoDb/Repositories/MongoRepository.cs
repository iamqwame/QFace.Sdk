using System.Linq.Expressions;

namespace QFace.Sdk.MongoDb.Repositories;

/// <summary>
/// Base implementation of the MongoDB repository
/// </summary>
/// <typeparam name="TDocument">The document type</typeparam>
public class MongoRepository<TDocument> : IMongoRepository<TDocument> where TDocument : BaseDocument
{
    protected readonly IMongoCollection<TDocument> _collection;
    protected readonly ILogger<MongoRepository<TDocument>> _logger;

    /// <summary>
    /// Gets the name of the collection
    /// </summary>
    public string CollectionName { get; }

    /// <summary>
    /// Creates a new instance of the MongoDB repository
    /// </summary>
    /// <param name="database">The MongoDB database</param>
    /// <param name="collectionName">The collection name</param>
    /// <param name="logger">The logger</param>
    public MongoRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger<MongoRepository<TDocument>> logger)
    {
        _collection = database.GetCollection<TDocument>(collectionName);
        _logger = logger;
        CollectionName = collectionName;
        
        // Create indexes on initialization
        try
        {
            CreateIndexesAsync().GetAwaiter().GetResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating indexes for collection {CollectionName}", collectionName);
        }
    }

    /// <summary>
    /// Gets all documents in the collection
    /// </summary>
    public virtual async Task<IEnumerable<TDocument>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = includeInactive
                ? Builders<TDocument>.Filter.Empty
                : Builders<TDocument>.Filter.Eq(doc => doc.IsActive, true);

            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all documents from collection {CollectionName}", CollectionName);
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
            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving document {Id} from collection {CollectionName}", id, CollectionName);
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

            return await _collection.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding document in collection {CollectionName}", CollectionName);
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

            return await _collection.Find(filter).ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding documents in collection {CollectionName}", CollectionName);
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
            
            await _collection.InsertOneAsync(document, null, cancellationToken);
            _logger.LogInformation("Document {Id} inserted into collection {CollectionName}", document.Id, CollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting document into collection {CollectionName}", CollectionName);
            throw;
        }
    }

    /// <summary>
    /// Inserts multiple documents
    /// </summary>
    public virtual async Task InsertManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default)
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
            
            await _collection.InsertManyAsync(documents, null, cancellationToken);
            _logger.LogInformation("Multiple documents inserted into collection {CollectionName}", CollectionName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error inserting multiple documents into collection {CollectionName}", CollectionName);
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
            var result = await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = false }, cancellationToken);

            var success = result.IsAcknowledged && result.ModifiedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} updated in collection {CollectionName}", document.Id, CollectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not updated in collection {CollectionName}. Result: {Result}", 
                    document.Id, CollectionName, result.ToJson());
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating document {Id} in collection {CollectionName}", document.Id, CollectionName);
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
            var result = await _collection.ReplaceOneAsync(filter, document, new ReplaceOptions { IsUpsert = true }, cancellationToken);

            var success = result.IsAcknowledged && (result.ModifiedCount > 0 || result.UpsertedId != null);
            if (success)
            {
                _logger.LogInformation("Document {Id} replaced in collection {CollectionName}", document.Id, CollectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not replaced in collection {CollectionName}. Result: {Result}", 
                    document.Id, CollectionName, result.ToJson());
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error replacing document {Id} in collection {CollectionName}", document.Id, CollectionName);
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
            var result = await _collection.DeleteOneAsync(filter, cancellationToken);

            var success = result.IsAcknowledged && result.DeletedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} deleted from collection {CollectionName}", id, CollectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not deleted from collection {CollectionName}", id, CollectionName);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting document {Id} from collection {CollectionName}", id, CollectionName);
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

            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);

            var success = result.IsAcknowledged && result.ModifiedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} soft-deleted in collection {CollectionName}", id, CollectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not soft-deleted in collection {CollectionName}", id, CollectionName);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error soft-deleting document {Id} in collection {CollectionName}", id, CollectionName);
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

            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);

            var success = result.IsAcknowledged && result.ModifiedCount > 0;
            if (success)
            {
                _logger.LogInformation("Document {Id} restored in collection {CollectionName}", id, CollectionName);
            }
            else
            {
                _logger.LogWarning("Document {Id} not restored in collection {CollectionName}", id, CollectionName);
            }
            
            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error restoring document {Id} in collection {CollectionName}", id, CollectionName);
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

        return _collection.Indexes.CreateOneAsync(indexModel, null, cancellationToken);
    }
}