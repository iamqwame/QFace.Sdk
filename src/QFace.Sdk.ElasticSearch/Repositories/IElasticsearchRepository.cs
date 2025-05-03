namespace QFace.Sdk.Elasticsearch.Repositories
{
    /// <summary>
    /// Interface for Elasticsearch repository operations
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    public interface IElasticsearchRepository<TDocument> where TDocument : EsBaseDocument
    {
        /// <summary>
        /// Gets the name of the index
        /// </summary>
        string IndexName { get; }
        
        /// <summary>
        /// Gets all documents in the index
        /// </summary>
        /// <param name="includeInactive">Whether to include inactive documents</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>All documents</returns>
        Task<IEnumerable<TDocument>> GetAllAsync(
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Gets a document by its ID
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The document</returns>
        Task<TDocument> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds documents matching the search query
        /// </summary>
        /// <param name="searchText">The search text</param>
        /// <param name="fields">The fields to search (null for all text fields)</param>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includeInactive">Whether to include inactive documents</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The search results</returns>
        Task<(IEnumerable<TDocument> Documents, long Total)> SearchAsync(
            string searchText,
            string[] fields = null,
            int page = 1,
            int pageSize = 20,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds documents matching a query descriptor
        /// </summary>
        /// <param name="queryDescriptor">The query descriptor</param>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includeInactive">Whether to include inactive documents</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The query results</returns>
        Task<(IEnumerable<TDocument> Documents, long Total)> QueryAsync(
            Func<QueryContainerDescriptor<TDocument>, QueryContainer> queryDescriptor,
            int page = 1,
            int pageSize = 20,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Finds documents matching a filter expression
        /// </summary>
        /// <param name="filterExpression">The filter expression</param>
        /// <param name="page">The page number (1-based)</param>
        /// <param name="pageSize">The page size</param>
        /// <param name="includeInactive">Whether to include inactive documents</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The filtered documents</returns>
        Task<(IEnumerable<TDocument> Documents, long Total)> FilterAsync(
            Expression<Func<TDocument, bool>> filterExpression,
            int page = 1,
            int pageSize = 20,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes a document
        /// </summary>
        /// <param name="document">The document to index</param>
        /// <param name="refresh">Whether to refresh the index immediately</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> IndexAsync(
            TDocument document,
            Refresh refresh= Refresh.WaitFor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Indexes multiple documents
        /// </summary>
        /// <param name="documents">The documents to index</param>
        /// <param name="refresh">Whether to refresh the index immediately</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> IndexManyAsync(
            IEnumerable<TDocument> documents,
            Refresh refresh= Refresh.WaitFor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a document
        /// </summary>
        /// <param name="document">The document to update</param>
        /// <param name="refresh">Whether to refresh the index immediately</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdateAsync(
            TDocument document,
            Refresh refresh= Refresh.WaitFor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Updates a document partially using a script
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="scriptBuilder">The script builder</param>
        /// <param name="refresh">Whether to refresh the index immediately</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> UpdatePartialAsync(
            string id,
            Func<ScriptDescriptor, IScript> scriptBuilder,
            Refresh refresh= Refresh.WaitFor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a document by ID
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="refresh">Whether to refresh the index immediately</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteAsync(
            string id,
            Refresh refresh= Refresh.WaitFor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Soft deletes a document by ID (sets IsActive to false)
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="refresh">Whether to refresh the index immediately</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> SoftDeleteAsync(
            string id,
             Refresh refresh= Refresh.WaitFor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Restores a soft-deleted document by ID (sets IsActive to true)
        /// </summary>
        /// <param name="id">The document ID</param>
        /// <param name="refresh">Whether to refresh the index immediately</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> RestoreAsync(
            string id,
             Refresh refresh= Refresh.WaitFor,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Checks if an index exists
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if the index exists, false otherwise</returns>
        Task<bool> IndexExistsAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Creates an index if it doesn't exist
        /// </summary>
        /// <param name="mappingSelector">The mapping selector for index configuration</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> CreateIndexAsync(
            Func<CreateIndexDescriptor, ICreateIndexRequest> mappingSelector = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes an index
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        Task<bool> DeleteIndexAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Counts documents with optional filtering
        /// </summary>
        /// <param name="filterExpression">Optional filter expression</param>
        /// <param name="includeInactive">Whether to include inactive documents</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The count of documents</returns>
        Task<long> CountAsync(
            Expression<Func<TDocument, bool>> filterExpression = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Executes an aggregation query
        /// </summary>
        /// <typeparam name="TAggregate">The aggregation result type</typeparam>
        /// <param name="aggregationSelector">The aggregation selector</param>
        /// <param name="filterExpression">Optional filter expression</param>
        /// <param name="includeInactive">Whether to include inactive documents</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The aggregation result</returns>
        Task<TAggregate> AggregateAsync<TAggregate>(
            Func<AggregationContainerDescriptor<TDocument>, IAggregationContainer> aggregationSelector,
            Expression<Func<TDocument, bool>> filterExpression = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default) where TAggregate : class;
    }
}