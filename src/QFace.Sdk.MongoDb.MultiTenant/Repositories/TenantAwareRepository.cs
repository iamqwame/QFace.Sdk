namespace QFace.Sdk.MongoDb.MultiTenant.Repositories;

/// <summary>
/// Decorator for repository that automatically applies tenant ID from the tenant accessor
/// </summary>
/// <typeparam name="TDocument">The document type</typeparam>
public class TenantAwareRepository<TDocument> : IMongoRepository<TDocument> 
    where TDocument : TenantBaseDocument
{
    private readonly IMongoRepository<TDocument> _innerRepository;
    private readonly ITenantAccessor _tenantAccessor;
    private readonly ILogger<TenantAwareRepository<TDocument>> _logger;
        
    /// <summary>
    /// Gets the name of the collection
    /// </summary>
    public string CollectionName => _innerRepository.CollectionName;
        
    /// <summary>
    /// Creates a new tenant-aware repository
    /// </summary>
    /// <param name="innerRepository">The inner repository to decorate</param>
    /// <param name="tenantAccessor">The tenant accessor</param>
    /// <param name="logger">The logger</param>
    public TenantAwareRepository(
        IMongoRepository<TDocument> innerRepository,
        ITenantAccessor tenantAccessor,
        ILogger<TenantAwareRepository<TDocument>> logger)
    {
        _innerRepository = innerRepository ?? throw new ArgumentNullException(nameof(innerRepository));
        _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
        
    /// <summary>
    /// Gets all documents in the collection with tenant filtering
    /// </summary>
    public async Task<IEnumerable<TDocument>> GetAllAsync(
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for GetAllAsync operation");
            return new List<TDocument>();
        }
            
        // Use custom filter to include tenant ID constraint
        return await _innerRepository.FindAsync(
            doc => doc.TenantId == tenantId,
            includeInactive,
            cancellationToken);
    }
        
    /// <summary>
    /// Gets a document by its ID with tenant validation
    /// </summary>
    public async Task<TDocument> GetByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for GetByIdAsync operation");
            return null;
        }
            
        // Use custom filter to include tenant ID constraint
        return await _innerRepository.FindOneAsync(
            doc => doc.Id == id && doc.TenantId == tenantId,
            true, // includeInactive = true to make sure we find it regardless of IsActive
            cancellationToken);
    }
        
    /// <summary>
    /// Finds a single document matching the filter expression with tenant filtering
    /// </summary>
    public async Task<TDocument> FindOneAsync(
        Expression<Func<TDocument, bool>> filterExpression,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for FindOneAsync operation");
            return null;
        }
            
        // Combine the filter expression with tenant ID constraint
        var tenantFilter = BuildTenantFilter(filterExpression, tenantId);
            
        return await _innerRepository.FindOneAsync(
            tenantFilter,
            includeInactive,
            cancellationToken);
    }
        
    /// <summary>
    /// Finds all documents matching the filter expression with tenant filtering
    /// </summary>
    public async Task<IEnumerable<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filterExpression,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for FindAsync operation");
            return new List<TDocument>();
        }
            
        // Combine the filter expression with tenant ID constraint
        var tenantFilter = BuildTenantFilter(filterExpression, tenantId);
            
        return await _innerRepository.FindAsync(
            tenantFilter,
            includeInactive,
            cancellationToken);
    }
        
    /// <summary>
    /// Inserts a document with tenant ID
    /// </summary>
    public async Task InsertOneAsync(
        TDocument document,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for InsertOneAsync operation");
            throw new InvalidOperationException("Cannot insert document: No tenant context available");
        }
            
        // Set tenant ID on the document
        document.TenantId = tenantId;
            
        await _innerRepository.InsertOneAsync(
            document,
            cancellationToken);
    }
        
    /// <summary>
    /// Inserts multiple documents with tenant ID
    /// </summary>
    public async Task InsertManyAsync(
        IEnumerable<TDocument> documents,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for InsertManyAsync operation");
            throw new InvalidOperationException("Cannot insert documents: No tenant context available");
        }
            
        // Set tenant ID on all documents
        foreach (var document in documents)
        {
            document.TenantId = tenantId;
        }
            
        await _innerRepository.InsertManyAsync(
            documents,
            cancellationToken);
    }
        
    /// <summary>
    /// Updates a document with tenant validation
    /// </summary>
    public async Task<bool> UpdateAsync(
        TDocument document,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for UpdateAsync operation");
            return false;
        }
            
        // First check if document belongs to current tenant
        var existingDoc = await GetByIdAsync(document.Id, cancellationToken);
        if (existingDoc == null)
        {
            _logger.LogWarning("Document {Id} not found or belongs to a different tenant", document.Id);
            return false;
        }
            
        // Ensure tenant ID is preserved on update
        document.TenantId = tenantId;
            
        return await _innerRepository.UpdateAsync(
            document,
            cancellationToken);
    }
        
    /// <summary>
    /// Replaces a document with tenant validation
    /// </summary>
    public async Task<bool> ReplaceOneAsync(
        TDocument document,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for ReplaceOneAsync operation");
            return false;
        }
            
        // First check if document belongs to current tenant
        var existingDoc = await GetByIdAsync(document.Id, cancellationToken);
        if (existingDoc == null && !string.IsNullOrEmpty(document.Id))
        {
            _logger.LogWarning("Document {Id} not found or belongs to a different tenant", document.Id);
            return false;
        }
            
        // Ensure tenant ID is set on new/replacement document
        document.TenantId = tenantId;
            
        return await _innerRepository.ReplaceOneAsync(
            document,
            cancellationToken);
    }
        
    /// <summary>
    /// Deletes a document by ID with tenant validation
    /// </summary>
    public async Task<bool> DeleteByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for DeleteByIdAsync operation");
            return false;
        }
            
        // First check if document belongs to current tenant
        var existingDoc = await GetByIdAsync(id, cancellationToken);
        if (existingDoc == null)
        {
            _logger.LogWarning("Document {Id} not found or belongs to a different tenant", id);
            return false;
        }
            
        // For deletion, we need a custom filter with tenant constraint
        // We can't just use the inner repository's DeleteByIdAsync because it doesn't filter by tenant
        var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Eq(doc => doc.Id, id),
            Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                
        var collection = GetCollection();
        var result = await collection.DeleteOneAsync(filter, cancellationToken);
            
        var success = result.IsAcknowledged && result.DeletedCount > 0;
        if (success)
        {
            _logger.LogInformation("Document {Id} deleted for tenant {TenantId}", id, tenantId);
        }
            
        return success;
    }
        
    /// <summary>
    /// Soft deletes a document by ID with tenant validation
    /// </summary>
    public async Task<bool> SoftDeleteByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for SoftDeleteByIdAsync operation");
            return false;
        }
            
        // First check if document belongs to current tenant
        var existingDoc = await GetByIdAsync(id, cancellationToken);
        if (existingDoc == null)
        {
            _logger.LogWarning("Document {Id} not found or belongs to a different tenant", id);
            return false;
        }
            
        // For soft deletion, we need a custom filter with tenant constraint
        var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Eq(doc => doc.Id, id),
            Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                
        var update = Builders<TDocument>.Update
            .Set(doc => doc.IsActive, false)
            .Set(doc => doc.LastModifiedDate, DateTime.UtcNow);
                
        var collection = GetCollection();
        var result = await collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
        var success = result.IsAcknowledged && result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Document {Id} soft-deleted for tenant {TenantId}", id, tenantId);
        }
            
        return success;
    }
        
    /// <summary>
    /// Restores a soft-deleted document with tenant validation
    /// </summary>
    public async Task<bool> RestoreByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for RestoreByIdAsync operation");
            return false;
        }
            
        // First check if document belongs to current tenant
        var existingDoc = await GetByIdAsync(id, cancellationToken);
        if (existingDoc == null)
        {
            _logger.LogWarning("Document {Id} not found or belongs to a different tenant", id);
            return false;
        }
            
        // For restoration, we need a custom filter with tenant constraint
        var filter = Builders<TDocument>.Filter.And(
            Builders<TDocument>.Filter.Eq(doc => doc.Id, id),
            Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                
        var update = Builders<TDocument>.Update
            .Set(doc => doc.IsActive, true)
            .Set(doc => doc.LastModifiedDate, DateTime.UtcNow);
                
        var collection = GetCollection();
        var result = await collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
        var success = result.IsAcknowledged && result.ModifiedCount > 0;
        if (success)
        {
            _logger.LogInformation("Document {Id} restored for tenant {TenantId}", id, tenantId);
        }
            
        return success;
    }
        
    /// <summary>
    /// Creates indexes for the collection
    /// </summary>
    public async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        // Create standard indexes
        await _innerRepository.CreateIndexesAsync(cancellationToken);
            
        // Also create tenant index for better performance
        var collection = GetCollection();
        var indexModel = new CreateIndexModel<TDocument>(
            Builders<TDocument>.IndexKeys.Ascending(x => x.TenantId),
            new CreateIndexOptions { Background = true, Name = "tenant_idx" }
        );
            
        await collection.Indexes.CreateOneAsync(indexModel, cancellationToken: cancellationToken);
    }
        
    /// <summary>
    /// Counts documents with tenant filtering
    /// </summary>
    public async Task<long> CountAsync(
        Expression<Func<TDocument, bool>> filterExpression = null,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for CountAsync operation");
            return 0;
        }
            
        // Combine the filter expression with tenant ID constraint
        var tenantFilter = filterExpression != null 
            ? BuildTenantFilter(filterExpression, tenantId)
            : doc => doc.TenantId == tenantId;
            
        return await _innerRepository.CountAsync(
            tenantFilter,
            includeInactive,
            cancellationToken);
    }
        
    /// <summary>
    /// Executes a bulk write operation with tenant validation
    /// </summary>
    public async Task<BulkWriteResult<TDocument>> BulkWriteAsync(
        IEnumerable<WriteModel<TDocument>> writeModels,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for BulkWriteAsync operation");
            throw new InvalidOperationException("Cannot execute bulk write: No tenant context available");
        }
            
        // Need to modify write models to include tenant constraints
        var tenantWriteModels = new List<WriteModel<TDocument>>();
            
        foreach (var writeModel in writeModels)
        {
            if (writeModel is InsertOneModel<TDocument> insertModel)
            {
                // Set tenant ID on document
                var document = insertModel.Document;
                document.TenantId = tenantId;
                    
                tenantWriteModels.Add(new InsertOneModel<TDocument>(document));
            }
            else if (writeModel is UpdateOneModel<TDocument> updateModel)
            {
                // Add tenant filter to update filter
                var tenantFilter = Builders<TDocument>.Filter.And(
                    updateModel.Filter,
                    Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                    
                tenantWriteModels.Add(new UpdateOneModel<TDocument>(tenantFilter, updateModel.Update)
                {
                    IsUpsert = updateModel.IsUpsert,
                    ArrayFilters = updateModel.ArrayFilters
                });
            }
            else if (writeModel is UpdateManyModel<TDocument> updateManyModel)
            {
                // Add tenant filter to update filter
                var tenantFilter = Builders<TDocument>.Filter.And(
                    updateManyModel.Filter,
                    Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                    
                tenantWriteModels.Add(new UpdateManyModel<TDocument>(tenantFilter, updateManyModel.Update)
                {
                    IsUpsert = updateManyModel.IsUpsert,
                    ArrayFilters = updateManyModel.ArrayFilters
                });
            }
            else if (writeModel is ReplaceOneModel<TDocument> replaceModel)
            {
                // Add tenant filter to replace filter
                var tenantFilter = Builders<TDocument>.Filter.And(
                    replaceModel.Filter,
                    Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                    
                // Set tenant ID on replacement document
                var replacement = replaceModel.Replacement;
                replacement.TenantId = tenantId;
                    
                tenantWriteModels.Add(new ReplaceOneModel<TDocument>(tenantFilter, replacement)
                {
                    IsUpsert = replaceModel.IsUpsert
                });
            }
            else if (writeModel is DeleteOneModel<TDocument> deleteModel)
            {
                // Add tenant filter to delete filter
                var tenantFilter = Builders<TDocument>.Filter.And(
                    deleteModel.Filter,
                    Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                    
                tenantWriteModels.Add(new DeleteOneModel<TDocument>(tenantFilter));
            }
            else if (writeModel is DeleteManyModel<TDocument> deleteManyModel)
            {
                // Add tenant filter to delete filter
                var tenantFilter = Builders<TDocument>.Filter.And(
                    deleteManyModel.Filter,
                    Builders<TDocument>.Filter.Eq(doc => doc.TenantId, tenantId));
                    
                tenantWriteModels.Add(new DeleteManyModel<TDocument>(tenantFilter));
            }
            else
            {
                // Unknown write model type, add as-is (shouldn't happen with standard MongoDB operations)
                tenantWriteModels.Add(writeModel);
                _logger.LogWarning("Unknown write model type: {WriteModelType}", writeModel.GetType().Name);
            }
        }
            
        // Use collection directly for bulk write with tenant-scoped write models
        var collection = GetCollection();
        return await collection.BulkWriteAsync(tenantWriteModels, null, cancellationToken);
    }
        
    /// <summary>
    /// Finds documents with paging, sorting, and tenant filtering
    /// </summary>
    public async Task<(IEnumerable<TDocument> Items, long TotalCount)> FindWithPagingAsync(
        Expression<Func<TDocument, bool>> filterExpression,
        Expression<Func<TDocument, object>> sortExpression,
        bool sortDescending = false,
        int page = 1,
        int pageSize = 20,
        bool includeInactive = false,
        CancellationToken cancellationToken = default)
    {
        var tenantId = _tenantAccessor.GetCurrentTenantId();
            
        if (string.IsNullOrEmpty(tenantId))
        {
            _logger.LogWarning("No tenant ID available for FindWithPagingAsync operation");
            return (new List<TDocument>(), 0);
        }
            
        // Combine the filter expression with tenant ID constraint
        var tenantFilter = BuildTenantFilter(filterExpression, tenantId);
            
        return await _innerRepository.FindWithPagingAsync(
            tenantFilter,
            sortExpression,
            sortDescending,
            page,
            pageSize,
            includeInactive,
            cancellationToken);
    }
        
    /// <summary>
    /// Builds a combined filter expression that includes tenant ID constraint
    /// </summary>
    private Expression<Func<TDocument, bool>> BuildTenantFilter(
        Expression<Func<TDocument, bool>> filterExpression, 
        string tenantId)
    {
        // If we have a filter, combine it with tenant filter using expression trees
        var parameter = Expression.Parameter(typeof(TDocument), "doc");
            
        // Original filter body with parameter replaced
        var originalBody = ParameterReplacer.Replace(filterExpression.Body, filterExpression.Parameters[0], parameter);
            
        // Tenant filter: doc.TenantId == tenantId
        var tenantProperty = Expression.Property(parameter, nameof(TenantBaseDocument.TenantId));
        var tenantValue = Expression.Constant(tenantId);
        var tenantEquality = Expression.Equal(tenantProperty, tenantValue);
            
        // Combine filters: originalFilter && tenantFilter
        var combinedBody = Expression.AndAlso(originalBody, tenantEquality);
            
        // Create new lambda expression with combined body
        return Expression.Lambda<Func<TDocument, bool>>(combinedBody, parameter);
    }
        
    /// <summary>
    /// Gets the underlying MongoDB collection
    /// </summary>
    private IMongoCollection<TDocument> GetCollection()
    {
        // Use reflection to get the collection field from the inner repository
        var fieldInfo = _innerRepository.GetType()
            .GetField("_collection", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
                
        if (fieldInfo == null)
        {
            throw new InvalidOperationException("Could not access collection field on inner repository");
        }
            
        var collection = fieldInfo.GetValue(_innerRepository) as IMongoCollection<TDocument>;
            
        if (collection == null)
        {
            throw new InvalidOperationException("Collection field is null or not of expected type");
        }
            
        return collection;
    }
}
    
/// <summary>
/// Helper class for replacing parameters in expression trees
/// </summary>
internal class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly Expression _newParameter;
        
    private ParameterReplacer(ParameterExpression oldParameter, Expression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }
        
    public static Expression Replace(Expression expression, ParameterExpression oldParameter, Expression newParameter)
    {
        return new ParameterReplacer(oldParameter, newParameter).Visit(expression);
    }
        
    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : node;
    }
}