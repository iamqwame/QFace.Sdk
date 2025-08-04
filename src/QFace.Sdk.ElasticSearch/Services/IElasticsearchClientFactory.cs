namespace QFace.Sdk.Elasticsearch.Services;

/// <summary>
/// Interface for creating OpenSearch/Elasticsearch client instances
/// </summary>
public interface IElasticsearchClientFactory
{
    /// <summary>
    /// Gets an OpenSearch client instance
    /// </summary>
    /// <returns>OpenSearch client</returns>
    IOpenSearchClient GetClient();
}
