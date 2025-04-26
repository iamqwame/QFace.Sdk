namespace QFace.Sdk.MongoDb.Services;

/// <summary>
/// Factory for creating MongoDB client instances
/// </summary>
public class MongoDbClientFactory : IMongoDbClientFactory
{
    private readonly MongoDbOptions _options;
    private readonly ILogger<MongoDbClientFactory> _logger;
    private readonly object _lock = new();
    private IMongoClient _client;
    private IMongoDatabase _database;

    /// <summary>
    /// Creates a new MongoDB client factory
    /// </summary>
    /// <param name="options">MongoDB options</param>
    /// <param name="logger">Logger</param>
    public MongoDbClientFactory(
        IOptions<MongoDbOptions> options,
        ILogger<MongoDbClientFactory> logger)
    {
        _options = options.Value;
        _logger = logger;
        
        // Validate options
        if (!_options.IsValid)
        {
            var error = "Invalid MongoDB configuration. ConnectionString and DatabaseName are required.";
            _logger.LogError(error);
            throw new InvalidOperationException(error);
        }
    }
    
    /// <summary>
    /// Gets a MongoDB client
    /// </summary>
    /// <returns>MongoDB client</returns>
    public IMongoClient GetClient()
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
    /// Gets a MongoDB database
    /// </summary>
    /// <param name="databaseName">Optional database name (overrides configuration)</param>
    /// <returns>MongoDB database</returns>
    public IMongoDatabase GetDatabase(string databaseName = null)
    {
        if (_database == null || !string.IsNullOrEmpty(databaseName))
        {
            lock (_lock)
            {
                if (_database == null || !string.IsNullOrEmpty(databaseName))
                {
                    var client = GetClient();
                    var dbName = databaseName ?? _options.DatabaseName;
                    _database = client.GetDatabase(dbName);
                    _logger.LogInformation("Connected to MongoDB database: {DatabaseName}", dbName);
                }
            }
        }
        
        return _database;
    }
    
    /// <summary>
    /// Creates a new MongoDB client
    /// </summary>
    private IMongoClient CreateClient()
    {
        try
        {
            _logger.LogInformation("Creating MongoDB client with connection string: {ConnectionString}", 
                MaskConnectionString(_options.ConnectionString));
            
            // Configure client settings
            var settings = MongoClientSettings.FromConnectionString(_options.ConnectionString);
            
            // Apply additional settings from options
            settings.ConnectTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds);
            settings.MaxConnectionPoolSize = _options.ConnectionPool.MaxSize;
            settings.MinConnectionPoolSize = _options.ConnectionPool.MinSize;
            settings.MaxConnectionLifeTime = TimeSpan.FromMinutes(_options.ConnectionPool.MaxConnectionLifeTimeMinutes);
            settings.WaitQueueTimeout = TimeSpan.FromMilliseconds(_options.ConnectionPool.WaitQueueTimeoutMilliseconds);
            settings.ServerSelectionTimeout = TimeSpan.FromSeconds(_options.ConnectionTimeoutSeconds);
            settings.UseTls = _options.UseSsl;
            
            // Add server monitoring for better diagnostics
            settings.ClusterConfigurator = builder =>
            {
                builder.Subscribe<MongoDB.Driver.Core.Events.ClusterClosedEvent>(e =>
                {
                    _logger.LogInformation("MongoDB cluster closed: {ClusterId}", e.ClusterId);
                });
                
                builder.Subscribe<MongoDB.Driver.Core.Events.ClusterOpeningEvent>(e =>
                {
                    _logger.LogInformation("MongoDB cluster opening: {ClusterId}", e.ClusterId);
                });
                
                builder.Subscribe<MongoDB.Driver.Core.Events.ConnectionPoolOpenedEvent>(e =>
                {
                    _logger.LogInformation("MongoDB connection pool opened: {ServerId}", e.ServerId);
                });
                
                builder.Subscribe<MongoDB.Driver.Core.Events.ServerHeartbeatFailedEvent>(e =>
                {
                    _logger.LogWarning("MongoDB server heartbeat failed: {ServerId}, {Exception}", 
                        e.ServerId, e.Exception.Message);
                });
            };
            
            // Create client
            return new MongoClient(settings);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating MongoDB client");
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
            "(?<=mongodb://[^:]+:)[^@]+(?=@)",
            "********"
        );
    }
}