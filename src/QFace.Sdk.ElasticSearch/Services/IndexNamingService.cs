using System.Reflection;
using Microsoft.Extensions.Options;
using QFace.Sdk.Elasticsearch.Options;
using QFace.Sdk.Elasticsearch.Models;

namespace QFace.Sdk.Elasticsearch.Services;

/// <summary>
/// Service for determining Elasticsearch index names based on configuration
/// </summary>
public class IndexNamingService : IIndexNamingService
{
    private readonly IndexNamingOptions _options;
    private readonly string _defaultPrefix;
    
    /// <summary>
    /// Creates a new index naming service
    /// </summary>
    /// <param name="options">Elasticsearch options</param>
    public IndexNamingService(IOptions<ElasticsearchOptions> options)
    {
        _options = options.Value.IndexNaming;
        _defaultPrefix = options.Value.DefaultIndexPrefix;
    }
    
    /// <summary>
    /// Gets the index name for a type based on configured naming strategy
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <returns>The index name</returns>
    public string GetIndexName<T>() where T : class
    {
        var typeName = typeof(T).Name;
        
        // Check for ElasticsearchType attribute
        if (!_options.UseTypeNameAsDefault)
        {
            var attribute = typeof(T).GetCustomAttributes(typeof(ElasticsearchTypeAttribute), true)
                .FirstOrDefault() as ElasticsearchTypeAttribute;
                
            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                typeName = attribute.Name;
            }
        }
        
        return GetIndexName(typeName);
    }
    
    /// <summary>
    /// Gets the index name for a type name based on configured naming strategy
    /// </summary>
    /// <param name="typeName">The type name</param>
    /// <returns>The index name</returns>
    public string GetIndexName(string typeName)
    {
        // Clean up the type name (e.g., remove "Document" or "Index" suffix if present)
        var cleanName = typeName;
        if (cleanName.EndsWith("Document"))
        {
            cleanName = cleanName.Substring(0, cleanName.Length - 8);
        }
        else if (cleanName.EndsWith("Index"))
        {
            cleanName = cleanName.Substring(0, cleanName.Length - 5);
        }
        
        var indexName = _options.Strategy.ToLowerInvariant() switch
        {
            "lowercase" => cleanName.ToLowerInvariant(),
            "prefixedlowercase" => $"{GetPrefix()}{cleanName.ToLowerInvariant()}",
            _ => cleanName // Raw strategy
        };
        
        return indexName;
    }
    
    /// <summary>
    /// Gets the index name with a date suffix for time-based indices
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <param name="date">The date to use in the index name</param>
    /// <param name="format">The date format (default: yyyy.MM)</param>
    /// <returns>The time-based index name</returns>
    public string GetTimeBasedIndexName<T>(DateTime date, string format = "yyyy.MM") where T : class
    {
        var baseIndexName = GetIndexName<T>();
        return $"{baseIndexName}-{date.ToString(format)}";
    }
    
    /// <summary>
    /// Gets the index alias for a type based on configured naming strategy
    /// </summary>
    /// <typeparam name="T">The document type</typeparam>
    /// <returns>The index alias</returns>
    public string GetIndexAlias<T>() where T : class
    {
        return GetIndexName<T>();
    }
    
    /// <summary>
    /// Gets the index prefix based on configuration
    /// </summary>
    private string GetPrefix()
    {
        if (string.IsNullOrEmpty(_defaultPrefix))
        {
            return _options.IncludeEnvironmentName ? $"{_options.EnvironmentName}-" : string.Empty;
        }
        
        return _options.IncludeEnvironmentName 
            ? $"{_defaultPrefix}-{_options.EnvironmentName}-" 
            : $"{_defaultPrefix}-";
    }
}