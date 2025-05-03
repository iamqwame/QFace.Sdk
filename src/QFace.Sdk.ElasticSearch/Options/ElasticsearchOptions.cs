namespace QFace.Sdk.Elasticsearch.Options;

/// <summary>
/// Configuration options for Elasticsearch connections
/// </summary>
public class ElasticsearchOptions
{
    /// <summary>
    /// Comma-separated list of Elasticsearch node URLs
    /// </summary>
    public string NodeUrls { get; set; } = string.Empty;
    
    /// <summary>
    /// Default index prefix for all indices
    /// </summary>
    public string DefaultIndexPrefix { get; set; } = string.Empty;
    
    /// <summary>
    /// Username for basic authentication
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// Password for basic authentication
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// API key for authentication (alternative to username/password)
    /// </summary>
    public string ApiKey { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to use SSL/TLS for connections
    /// </summary>
    public bool EnableSsl { get; set; } = true;
    
    /// <summary>
    /// Whether to validate SSL certificates
    /// </summary>
    public bool ValidateSslCertificate { get; set; } = true;
    
    /// <summary>
    /// Connection timeout in seconds
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Request timeout in seconds
    /// </summary>
    public int RequestTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum number of retries for requests
    /// </summary>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Retry timeout in seconds
    /// </summary>
    public int RetryTimeoutSeconds { get; set; } = 60;
    
    /// <summary>
    /// Whether to enable request/response sniffing for debugging
    /// </summary>
    public bool EnableDebugMode { get; set; } = false;
    
    /// <summary>
    /// Sniffing configuration
    /// </summary>
    public SniffingOptions Sniffing { get; set; } = new SniffingOptions();
    
    /// <summary>
    /// Index naming configuration
    /// </summary>
    public IndexNamingOptions IndexNaming { get; set; } = new IndexNamingOptions();
    
    /// <summary>
    /// Validates that required options are set
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    [JsonIgnore]
    public bool IsValid => !string.IsNullOrEmpty(NodeUrls);
    
    /// <summary>
    /// Gets the node URLs as an array
    /// </summary>
    [JsonIgnore]
    public string[] Nodes => NodeUrls.Split(',', StringSplitOptions.RemoveEmptyEntries);
}