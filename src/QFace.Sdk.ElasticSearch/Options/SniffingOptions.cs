namespace QFace.Sdk.Elasticsearch.Options;

/// <summary>
/// Configuration options for Elasticsearch sniffing
/// </summary>
public class SniffingOptions
{
    /// <summary>
    /// Whether to enable cluster sniffing
    /// </summary>
    public bool Enabled { get; set; } = false;
    
    /// <summary>
    /// Interval in seconds between sniffing operations
    /// </summary>
    public int IntervalSeconds { get; set; } = 60;
    
    /// <summary>
    /// Whether to sniff on startup
    /// </summary>
    public bool SniffOnStartup { get; set; } = false;
    
    /// <summary>
    /// Whether to sniff on connection failure
    /// </summary>
    public bool SniffOnConnectionFailure { get; set; } = true;
}

/// <summary>
/// Configuration options for Elasticsearch index naming conventions
/// </summary>
public class IndexNamingOptions
{
    /// <summary>
    /// Index naming strategy (Raw, LowerCase, PrefixedLowerCase)
    /// </summary>
    public string Strategy { get; set; } = "PrefixedLowerCase";
    
    /// <summary>
    /// Whether to use .net type name (true) or [ElasticsearchType] attribute name (false)
    /// </summary>
    public bool UseTypeNameAsDefault { get; set; } = true;
    
    /// <summary>
    /// Whether to include the environment name in the index name
    /// </summary>
    public bool IncludeEnvironmentName { get; set; } = true;
    
    /// <summary>
    /// Environment name to include in index names
    /// </summary>
    public string EnvironmentName { get; set; } = "dev";
}