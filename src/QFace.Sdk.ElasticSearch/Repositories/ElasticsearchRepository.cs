namespace QFace.Sdk.Elasticsearch.Repositories
{
    /// <summary>
    /// Base implementation of the Elasticsearch repository
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    public class ElasticsearchRepository<TDocument> : IElasticsearchRepository<TDocument> where TDocument : EsBaseDocument
    {
        /// <summary>
        /// Elasticsearch client
        /// </summary>
        protected readonly IElasticClient _client;
        
        /// <summary>
        /// Logger instance
        /// </summary>
        protected readonly ILogger<ElasticsearchRepository<TDocument>> _logger;

        /// <summary>
        /// Gets the name of the index
        /// </summary>
        public string IndexName { get; }

        /// <summary>
        /// Creates a new instance of the Elasticsearch repository
        /// </summary>
        public ElasticsearchRepository(
            IElasticClient client,
            string indexName,
            ILogger<ElasticsearchRepository<TDocument>> logger)
        {
            _client = client;
            _logger = logger;
            IndexName = indexName;
            
            // Try to create index if it doesn't exist
            try
            {
                CreateIndexAsync().GetAwaiter().GetResult();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating index {IndexName}", indexName);
            }
        }

        /// <summary>
        /// Gets all documents in the index
        /// </summary>
        public virtual async Task<IEnumerable<TDocument>> GetAllAsync(
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var searchDescriptor = new SearchDescriptor<TDocument>()
                    .Index(IndexName)
                    .Size(1000); // Use a reasonable default size
                
                // Add active filter
                if (!includeInactive)
                {
                    searchDescriptor = searchDescriptor.Query(q => q
                        .Term(t => t
                            .Field(f => f.IsActive)
                            .Value(true)));
                }
                
                var response = await _client.SearchAsync<TDocument>(searchDescriptor, cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error retrieving all documents from index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return Enumerable.Empty<TDocument>();
                }
                
                // Update scores
                foreach (var hit in response.Hits)
                {
                    hit.Source.Score = hit.Score;
                }
                
                return response.Documents;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all documents from index {IndexName}", IndexName);
                return Enumerable.Empty<TDocument>();
            }
        }

        /// <summary>
        /// Gets a document by its ID
        /// </summary>
        public virtual async Task<TDocument> GetByIdAsync(
            string id,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.GetAsync<TDocument>(id, g => g.Index(IndexName), cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error retrieving document {Id} from index {IndexName}: {Error}", 
                        id, IndexName, response.DebugInformation);
                    return null;
                }
                
                if (!response.Found)
                {
                    return null;
                }
                
                return response.Source;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving document {Id} from index {IndexName}", 
                    id, IndexName);
                return null;
            }
        }

        /// <summary>
        /// Finds documents matching the search query
        /// </summary>
        public virtual async Task<(IEnumerable<TDocument> Documents, long Total)> SearchAsync(
            string searchText,
            string[] fields = null,
            int page = 1,
            int pageSize = 20,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var searchDescriptor = new SearchDescriptor<TDocument>()
                    .Index(IndexName)
                    .From((page - 1) * pageSize)
                    .Size(pageSize);
                
                // Build query based on search text and fields
                QueryContainer query = null;
                
                if (!string.IsNullOrEmpty(searchText))
                {
                    if (fields != null && fields.Length > 0)
                    {
                        var multiMatchQuery = new MultiMatchQuery
                        {
                            Fields = fields,
                            Query = searchText,
                            Type = TextQueryType.BestFields,
                            Fuzziness = Fuzziness.Auto
                        };
                        query = multiMatchQuery;
                    }
                    else
                    {
                        var queryStringQuery = new QueryStringQuery
                        {
                            Query = searchText,
                            DefaultOperator = Operator.And
                        };
                        query = queryStringQuery;
                    }
                }
                
                // Add active filter if needed
                if (!includeInactive)
                {
                    var activeFilter = new TermQuery
                    {
                        Field = new Field("isActive"),
                        Value = true
                    };
                    
                    if (query != null)
                    {
                        var boolQuery = new BoolQuery
                        {
                            Must = new List<QueryContainer> { query },
                            Filter = new List<QueryContainer> { activeFilter }
                        };
                        query = boolQuery;
                    }
                    else
                    {
                        query = activeFilter;
                    }
                }
                
                // Add query to search descriptor if we have one
                if (query != null)
                {
                    searchDescriptor = searchDescriptor.Query(_ => query);
                }
                
                var response = await _client.SearchAsync<TDocument>(searchDescriptor, cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error searching documents in index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return (Enumerable.Empty<TDocument>(), 0);
                }
                
                // Update scores
                foreach (var hit in response.Hits)
                {
                    hit.Source.Score = hit.Score;
                }
                
                return (response.Documents, response.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error searching documents in index {IndexName}", IndexName);
                return (Enumerable.Empty<TDocument>(), 0);
            }
        }

        /// <summary>
        /// Finds documents matching a query descriptor
        /// </summary>
        public virtual async Task<(IEnumerable<TDocument> Documents, long Total)> QueryAsync(
            Func<QueryContainerDescriptor<TDocument>, QueryContainer> queryDescriptor,
            int page = 1,
            int pageSize = 20,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var searchDescriptor = new SearchDescriptor<TDocument>()
                    .Index(IndexName)
                    .From((page - 1) * pageSize)
                    .Size(pageSize);
                
                if (queryDescriptor != null)
                {
                    if (!includeInactive)
                    {
                        searchDescriptor = searchDescriptor.Query(q => q
                            .Bool(b => b
                                .Must(queryDescriptor)
                                .Filter(f => f
                                    .Term(t => t
                                        .Field(fd => fd.IsActive)
                                        .Value(true)))));
                    }
                    else
                    {
                        searchDescriptor = searchDescriptor.Query(queryDescriptor);
                    }
                }
                else if (!includeInactive)
                {
                    searchDescriptor = searchDescriptor.Query(q => q
                        .Term(t => t
                            .Field(fd => fd.IsActive)
                            .Value(true)));
                }
                
                var response = await _client.SearchAsync<TDocument>(searchDescriptor, cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error querying documents in index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return (Enumerable.Empty<TDocument>(), 0);
                }
                
                // Update scores
                foreach (var hit in response.Hits)
                {
                    hit.Source.Score = hit.Score;
                }
                
                return (response.Documents, response.Total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error querying documents in index {IndexName}", IndexName);
                return (Enumerable.Empty<TDocument>(), 0);
            }
        }

        /// <summary>
        /// Finds documents matching a filter expression
        /// </summary>
        public virtual async Task<(IEnumerable<TDocument> Documents, long Total)> FilterAsync(
            Expression<Func<TDocument, bool>> filterExpression,
            int page = 1,
            int pageSize = 20,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Create a query from the filter expression
                string queryString = GetQueryFromExpression(filterExpression);
                return await QueryAsync(
                    q => q.QueryString(qs => qs.Query(queryString)),
                    page,
                    pageSize, 
                    includeInactive,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error filtering documents in index {IndexName}", IndexName);
                return (Enumerable.Empty<TDocument>(), 0);
            }
        }

        // Helper method for FilterAsync
        private string GetQueryFromExpression(Expression<Func<TDocument, bool>> expression)
        {
            // Simple implementation - ideally use a proper LINQ to Elasticsearch converter
            if (expression == null)
                return "*";
            
            var expressionString = expression.ToString();
            
            // Extract the condition part
            var arrowIndex = expressionString.IndexOf("=>");
            if (arrowIndex >= 0)
            {
                expressionString = expressionString.Substring(arrowIndex + 2).Trim();
            }
            
            // Replace parameter name
            var paramName = expression.Parameters[0].Name;
            expressionString = expressionString.Replace(paramName + ".", string.Empty);
            
            // Convert common operators
            expressionString = expressionString
                .Replace(" == ", ":")
                .Replace(" != ", ":-")
                .Replace(" > ", ":>")
                .Replace(" >= ", ":>=")
                .Replace(" < ", ":<")
                .Replace(" <= ", ":<=")
                .Replace(" && ", " AND ")
                .Replace(" || ", " OR ")
                .Replace(".Contains(", ":")
                .Replace(".StartsWith(", ":")
                .Replace(".EndsWith(", ":")
                .Replace(")", string.Empty);
            
            return expressionString;
        }

        /// <summary>
        /// Indexes a document
        /// </summary>
        public virtual async Task<bool> IndexAsync(
            TDocument document,
            Refresh refresh = Refresh.WaitFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Set audit fields
                document.LastModifiedDate = DateTime.UtcNow;
                
                // If it's a new document, set created date
                if (string.IsNullOrEmpty(document.Id))
                {
                    document.Id = Guid.NewGuid().ToString();
                    document.CreatedDate = DateTime.UtcNow;
                }
                
                var response = await _client.IndexAsync(document, i => i
                    .Index(IndexName)
                    .Id(document.Id)
                    .Refresh(refresh), 
                    cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error indexing document {Id} in index {IndexName}: {Error}", 
                        document.Id, IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Document {Id} indexed in {IndexName}", 
                    document.Id, IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error indexing document {Id} in index {IndexName}", 
                    document?.Id, IndexName);
                return false;
            }
        }

        /// <summary>
        /// Indexes multiple documents
        /// </summary>
        public virtual async Task<bool> IndexManyAsync(
            IEnumerable<TDocument> documents,
            Refresh refresh = Refresh.WaitFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Set audit fields
                var now = DateTime.UtcNow;
                foreach (var document in documents)
                {
                    document.LastModifiedDate = now;
                    
                    // If it's a new document, set created date
                    if (string.IsNullOrEmpty(document.Id))
                    {
                        document.Id = Guid.NewGuid().ToString();
                        document.CreatedDate = now;
                    }
                }
                
                var response = await _client.BulkAsync(b => b
                    .Index(IndexName)
                    .Refresh(refresh)
                    .IndexMany(documents), 
                    cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error bulk indexing documents in index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("{Count} documents indexed in {IndexName}", 
                    documents.Count(), IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error bulk indexing documents in index {IndexName}", IndexName);
                return false;
            }
        }

        /// <summary>
        /// Updates a document
        /// </summary>
        public virtual async Task<bool> UpdateAsync(
            TDocument document,
            Refresh refresh = Refresh.WaitFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(document.Id))
                {
                    _logger.LogError("Cannot update document with empty ID in index {IndexName}", IndexName);
                    return false;
                }
                
                // Set audit fields
                document.LastModifiedDate = DateTime.UtcNow;
                
                var response = await _client.UpdateAsync<TDocument>(document.Id, u => u
                    .Index(IndexName)
                    .Doc(document)
                    .Refresh(refresh), 
                    cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error updating document {Id} in index {IndexName}: {Error}", 
                        document.Id, IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Document {Id} updated in {IndexName}", 
                    document.Id, IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating document {Id} in index {IndexName}", 
                    document?.Id, IndexName);
                return false;
            }
        }

        /// <summary>
        /// Updates a document partially using a script
        /// </summary>
        public virtual async Task<bool> UpdatePartialAsync(
            string id,
            Func<ScriptDescriptor, IScript> scriptBuilder,
            Refresh refresh = Refresh.WaitFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogError("Cannot update document with empty ID in index {IndexName}", IndexName);
                    return false;
                }
                
                var response = await _client.UpdateAsync<TDocument>(id, u => u
                    .Index(IndexName)
                    .Script(scriptBuilder)
                    .Refresh(refresh), 
                    cancellationToken);
                
                // Update the last modified date in a separate request
                if (response.IsValid)
                {
                    await _client.UpdateAsync<TDocument>(id, u => u
                        .Index(IndexName)
                        .Script(s => s
                            .Source("ctx._source.lastModifiedDate = params.lastModified")
                            .Params(p => p.Add("lastModified", DateTime.UtcNow)))
                        .Refresh(refresh), 
                        cancellationToken);
                }
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error partially updating document {Id} in index {IndexName}: {Error}", 
                        id, IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Document {Id} partially updated in {IndexName}", 
                    id, IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error partially updating document {Id} in index {IndexName}", 
                    id, IndexName);
                return false;
            }
        }

        /// <summary>
        /// Deletes a document by ID
        /// </summary>
        public virtual async Task<bool> DeleteAsync(
            string id,
            Refresh refresh = Refresh.WaitFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                if (string.IsNullOrEmpty(id))
                {
                    _logger.LogError("Cannot delete document with empty ID in index {IndexName}", IndexName);
                    return false;
                }
                
                var response = await _client.DeleteAsync<TDocument>(id, d => d
                    .Index(IndexName)
                    .Refresh(refresh), 
                    cancellationToken);
                
                if (!response.IsValid && response.Result != Result.NotFound)
                {
                    _logger.LogError("Error deleting document {Id} from index {IndexName}: {Error}", 
                        id, IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Document {Id} deleted from {IndexName}", 
                    id, IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting document {Id} from index {IndexName}", 
                    id, IndexName);
                return false;
            }
        }

        /// <summary>
        /// Soft deletes a document by ID (sets IsActive to false)
        /// </summary>
        public virtual async Task<bool> SoftDeleteAsync(
            string id,
            Refresh refresh = Refresh.WaitFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.UpdateAsync<TDocument>(id, u => u
                    .Index(IndexName)
                    .Script(s => s
                        .Source("ctx._source.isActive = false")
                        .Lang("painless"))
                    .Refresh(refresh), 
                    cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error soft deleting document {Id} from index {IndexName}: {Error}", 
                        id, IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Document {Id} soft deleted from {IndexName}", 
                    id, IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error soft deleting document {Id} from index {IndexName}", 
                    id, IndexName);
                return false;
            }
        }

        /// <summary>
        /// Restores a soft-deleted document by ID (sets IsActive to true)
        /// </summary>
        public virtual async Task<bool> RestoreAsync(
            string id,
            Refresh refresh = Refresh.WaitFor,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.UpdateAsync<TDocument>(id, u => u
                    .Index(IndexName)
                    .Script(s => s
                        .Source("ctx._source.isActive = true")
                        .Lang("painless"))
                    .Refresh(refresh), 
                    cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error restoring document {Id} in index {IndexName}: {Error}", 
                        id, IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Document {Id} restored in {IndexName}", 
                    id, IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring document {Id} in index {IndexName}", 
                    id, IndexName);
                return false;
            }
        }

        /// <summary>
        /// Checks if an index exists
        /// </summary>
        public virtual async Task<bool> IndexExistsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.Indices.ExistsAsync(IndexName, ct: cancellationToken);
                return response.Exists;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if index {IndexName} exists", IndexName);
                return false;
            }
        }

        /// <summary>
        /// Creates an index if it doesn't exist
        /// </summary>
        public virtual async Task<bool> CreateIndexAsync(
            Func<CreateIndexDescriptor, ICreateIndexRequest> mappingSelector = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if index exists
                var exists = await IndexExistsAsync(cancellationToken);
                if (exists)
                {
                    return true; // Index already exists
                }
                
                // Create the index
                var descriptor = new CreateIndexDescriptor(IndexName);
                
                if (mappingSelector != null)
                {
                    descriptor = (CreateIndexDescriptor)mappingSelector(descriptor);
                }
                else
                {
                    descriptor = descriptor.Map<TDocument>(m => m.AutoMap());
                }
                
                var response = await _client.Indices.CreateAsync(descriptor, cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error creating index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Index {IndexName} created", IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating index {IndexName}", IndexName);
                return false;
            }
        }

        /// <summary>
        /// Deletes an index
        /// </summary>
        public virtual async Task<bool> DeleteIndexAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var response = await _client.Indices.DeleteAsync(IndexName, ct: cancellationToken);
                
                if (!response.IsValid && response.ServerError?.Status != 404) // Ignore 404 (index not found)
                {
                    _logger.LogError("Error deleting index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return false;
                }
                
                _logger.LogInformation("Index {IndexName} deleted", IndexName);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting index {IndexName}", IndexName);
                return false;
            }
        }

        /// <summary>
        /// Counts documents with optional filtering
        /// </summary>
        public virtual async Task<long> CountAsync(
            Expression<Func<TDocument, bool>> filterExpression = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Use a request approach instead of descriptor to avoid type conversion issues
                var countRequest = new CountRequest(IndexName);
                
                if (filterExpression != null)
                {
                    var filterQueryString = GetQueryFromExpression(filterExpression);
                    
                    if (!includeInactive)
                    {
                        countRequest.Query = new BoolQuery
                        {
                            Must = new List<QueryContainer> { new QueryStringQuery { Query = filterQueryString } },
                            Filter = new List<QueryContainer> { new TermQuery { Field = "isActive", Value = true } }
                        };
                    }
                    else
                    {
                        countRequest.Query = new QueryStringQuery { Query = filterQueryString };
                    }
                }
                else if (!includeInactive)
                {
                    countRequest.Query = new TermQuery { Field = "isActive", Value = true };
                }
                
                var response = await _client.CountAsync(countRequest, cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error counting documents in index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return 0;
                }
                
                return response.Count;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error counting documents in index {IndexName}", IndexName);
                return 0;
            }
        }

        /// <summary>
        /// Executes an aggregation query
        /// </summary>
        public virtual async Task<TAggregate> AggregateAsync<TAggregate>(
            Func<AggregationContainerDescriptor<TDocument>, IAggregationContainer> aggregationSelector,
            Expression<Func<TDocument, bool>> filterExpression = null,
            bool includeInactive = false,
            CancellationToken cancellationToken = default) where TAggregate : class
        {
            try
            {
                var searchDescriptor = new SearchDescriptor<TDocument>()
                    .Index(IndexName)
                    .Size(0) // No documents, only aggregations
                    .Aggregations(aggregationSelector);
                
                if (filterExpression != null)
                {
                    var filterQuery = new QueryStringQuery { Query = GetQueryFromExpression(filterExpression) };
                    
                    if (!includeInactive)
                    {
                        searchDescriptor = searchDescriptor.Query(q => q
                            .Bool(b => b
                                .Must(m => filterQuery)
                                .Filter(f => f
                                    .Term(t => t
                                        .Field(fd => fd.IsActive)
                                        .Value(true)))));
                    }
                    else
                    {
                        searchDescriptor = searchDescriptor.Query(q => filterQuery);
                    }
                }
                else if (!includeInactive)
                {
                    searchDescriptor = searchDescriptor.Query(q => q
                        .Term(t => t
                            .Field(fd => fd.IsActive)
                            .Value(true)));
                }
                
                var response = await _client.SearchAsync<TDocument>(searchDescriptor, cancellationToken);
                
                if (!response.IsValid)
                {
                    _logger.LogError("Error executing aggregation in index {IndexName}: {Error}", 
                        IndexName, response.DebugInformation);
                    return null;
                }
                
                return response.Aggregations as TAggregate;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error executing aggregation in index {IndexName}", IndexName);
                return null;
            }
        }
    }
}