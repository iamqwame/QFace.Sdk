namespace QFace.Sdk.Elasticsearch.Services;

/// <summary>
/// Factory for creating Elasticsearch client instances
/// </summary>
public class ElasticsearchClientFactory : IElasticsearchClientFactory
{
    private readonly ElasticsearchOptions _options;
    private readonly ILogger<ElasticsearchClientFactory> _logger;
    private readonly object _lock = new object();
    private IElasticClient _client;

    /// <summary>
    /// Creates a new Elasticsearch client factory
    /// </summary>
    /// <param name="options">Elasticsearch options</param>
    /// <param name="logger">Logger</param>
    public ElasticsearchClientFactory(
        IOptions<ElasticsearchOptions> options,
        ILogger<ElasticsearchClientFactory> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        // Validate options
        if (!_options.IsValid)
        {
            var error = "Invalid Elasticsearch configuration. NodeUrls is required.";
            _logger.LogError(error);
            throw new InvalidOperationException(error);
        }
    }
    
    /// <summary>
    /// Gets an Elasticsearch client
    /// </summary>
    /// <returns>Elasticsearch client</returns>
    public IElasticClient GetClient()
    {
        if (_client == null)
        {
            lock (_lock)
            {
                if (_client == null)
                {
                    _client = CreateClient();
                }
            }
        }
        
        return _client;
    }
    
    /// <summary>
    /// Creates a new Elasticsearch client
    /// </summary>
    private IElasticClient CreateClient()
    {
        try
        {
            _logger.LogInformation("Creating Elasticsearch client with nodes: {NodeUrls}", 
                MaskConnectionString(_options.NodeUrls));
            
            // Configure connection settings
            var uris = _options.Nodes.Select(node => new Uri(node.Trim())).ToArray();
            var connectionPool = new StaticConnectionPool(uris);
            
            var settings = new ConnectionSettings(connectionPool);
            
            // Configure authentication
            if (!string.IsNullOrEmpty(_options.Username) && !string.IsNullOrEmpty(_options.Password))
            {
                settings = settings.BasicAuthentication(_options.Username, _options.Password);
            }
            else if (!string.IsNullOrEmpty(_options.ApiKey))
            {
                // Use the id and api key format supported by Elasticsearch
                var apiKeyParts = _options.ApiKey.Split(':');
                if (apiKeyParts.Length == 2)
                {
                    settings = settings.ApiKeyAuthentication(apiKeyParts[0], apiKeyParts[1]);
                }
                else
                {
                    _logger.LogWarning("Invalid API Key format. Expected 'id:api_key'");
                }
            }
            
            // Configure default index
            if (!string.IsNullOrEmpty(_options.DefaultIndexPrefix))
            {
                settings = settings.DefaultIndex(_options.DefaultIndexPrefix);
            }
            
            // Configure SSL
            if (_options.EnableSsl)
            {
                if (!_options.ValidateSslCertificate)
                {
                    settings = settings.ServerCertificateValidationCallback((_, __, ___, ____) => true);
                }
            }
            
            // Configure timeouts
            settings = settings
                .RequestTimeout(TimeSpan.FromSeconds(_options.RequestTimeoutSeconds))
                .PingTimeout(TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds))
                .MaximumRetries(_options.MaxRetries)
                .MaxRetryTimeout(TimeSpan.FromSeconds(_options.RetryTimeoutSeconds));
            
            // Configure sniffing
            if (_options.Sniffing.Enabled)
            {
                settings = settings
                    .SniffOnStartup(_options.Sniffing.SniffOnStartup)
                    .SniffOnConnectionFault(_options.Sniffing.SniffOnConnectionFailure)
                    .SniffLifeSpan(TimeSpan.FromSeconds(_options.Sniffing.IntervalSeconds));
            }
            
            // Configure debugging
            if (_options.EnableDebugMode)
            {
                settings = settings
                    .DisableDirectStreaming()
                    .EnableDebugMode()
                    .OnRequestCompleted(details =>
                    {
                        _logger.LogDebug("Elasticsearch Request: {Request}", details.DebugInformation);
                    });
            }
            
            // Create client
            var client = new ElasticClient(settings);
            
            // Test connection
            var pingResponse = client.Ping();
            if (!pingResponse.IsValid)
            {
                _logger.LogWarning("Elasticsearch ping failed: {Error}", 
                    pingResponse.DebugInformation);
            }
            else
            {
                _logger.LogInformation("Successfully connected to Elasticsearch cluster");
            }
            
            return client;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating Elasticsearch client");
            throw;
        }
    }
    
    /// <summary>
    /// Masks sensitive parts of the connection string for logging
    /// </summary>
    private string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }
        
        // Mask password in connection string
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            "(?<=//[^:]+:)[^@]+(?=@)",
            "********"
        );
    }
}