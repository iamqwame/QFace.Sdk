using System.Linq.Expressions;

namespace QFace.Sdk.MongoDb.Repositories;

/// <summary>
/// Interface for MongoDB repository operations
/// </summary>
/// <typeparam name="TDocument">The document type</typeparam>
public interface IMongoRepository<TDocument> where TDocument : BaseDocument
{
    /// <summary>
    /// Gets the name of the collection
    /// </summary>
    string CollectionName { get; }
    
    /// <summary>
    /// Gets all documents in the collection
    /// </summary>
    /// <param name="includeInactive">Whether to include inactive documents</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>All documents</returns>
    Task<IEnumerable<TDocument>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Gets a document by its ID
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The document</returns>
    Task<TDocument> GetByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds a single document matching the filter expression
    /// </summary>
    /// <param name="filterExpression">The filter expression</param>
    /// <param name="includeInactive">Whether to include inactive documents</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The matching document</returns>
    Task<TDocument> FindOneAsync(
        Expression<Func<TDocument, bool>> filterExpression, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Finds all documents matching the filter expression
    /// </summary>
    /// <param name="filterExpression">The filter expression</param>
    /// <param name="includeInactive">Whether to include inactive documents</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The matching documents</returns>
    Task<IEnumerable<TDocument>> FindAsync(
        Expression<Func<TDocument, bool>> filterExpression, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Inserts a document
    /// </summary>
    /// <param name="document">The document to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion task</returns>
    Task InsertOneAsync(TDocument document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Inserts multiple documents
    /// </summary>
    /// <param name="documents">The documents to insert</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion task</returns>
    Task InsertManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Updates a document
    /// </summary>
    /// <param name="document">The document to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> UpdateAsync(TDocument document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Replaces a document
    /// </summary>
    /// <param name="document">The document to replace</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> ReplaceOneAsync(TDocument document, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Deletes a document by ID (hard delete)
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Soft deletes a document by ID (sets IsActive to false)
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> SoftDeleteByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Restores a soft-deleted document
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if successful, false otherwise</returns>
    Task<bool> RestoreByIdAsync(string id, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Creates indexes for the collection
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Completion task</returns>
    Task CreateIndexesAsync(CancellationToken cancellationToken = default);
}