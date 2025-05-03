namespace QFace.Sdk.Elasticsearch.Services;

/// <summary>
/// Interface for service that determines Elasticsearch index names
/// </summary>
public interface IIndexNamingService
{
    /// <summary>
    /// Gets the index name for a type based on configured naming strategy
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <returns>The index name</returns>
    string GetIndexName<T>() where T : class;
    
    /// <summary>
    /// Gets the index name for a type name based on configured naming strategy
    /// </summary>
    /// <param name="typeName">The type name</param>
    /// <returns>The index name</returns>
    string GetIndexName(string typeName);
    
    /// <summary>
    /// Gets the index name with a date suffix for time-based indices
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <param name="date">The date to use in the index name</param>
    /// <param name="format">The date format (default: yyyy.MM)</param>
    /// <returns>The time-based index name</returns>
    string GetTimeBasedIndexName<T>(DateTime date, string format = "yyyy.MM") where T : class;
    
    /// <summary>
    /// Gets the index alias for a type based on configured naming strategy
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <returns>The index alias</returns>
    string GetIndexAlias<T>() where T : class;
}

/// <summary>
/// Interface for factory that creates Elasticsearch client instances
/// </summary>
public interface IElasticsearchClientFactory
{
    /// <summary>
    /// Gets an Elasticsearch client
    /// </summary>
    /// <returns>Elasticsearch client</returns>
    IElasticClient GetClient();
}