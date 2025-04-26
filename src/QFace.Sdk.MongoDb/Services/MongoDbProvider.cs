namespace QFace.Sdk.MongoDb.Services;

/// <summary>
/// Provider for accessing multiple MongoDB databases
/// Useful for multi-tenant scenarios where each tenant has its own database
/// </summary>
public class MongoDbProvider : IMongoDbProvider
{
    private readonly IMongoDbClientFactory _clientFactory;
    private readonly ILogger<MongoDbProvider> _logger;
    private readonly ConcurrentDictionary<string, IMongoDatabase> _databases = new();
    private readonly MongoDbOptions _options;

    /// <summary>
    /// Creates a new MongoDB provider
    /// </summary>
    /// <param name="clientFactory">The MongoDB client factory</param>
    /// <param name="options">The MongoDB options</param>
    /// <param name="logger">The logger</param>
    public MongoDbProvider(
        IMongoDbClientFactory clientFactory,
        IOptions<MongoDbOptions> options,
        ILogger<MongoDbProvider> logger)
    {
        _clientFactory = clientFactory;
        _logger = logger;
        _options = options.Value;
    }

    /// <summary>
    /// Gets a MongoDB database
    /// </summary>
    /// <param name="databaseName">The database name (if null, uses the default database)</param>
    /// <returns>The MongoDB database</returns>
    public IMongoDatabase GetDatabase(string databaseName = null)
    {
        var dbName = databaseName ?? _options.DatabaseName;
        
        if (string.IsNullOrEmpty(dbName))
        {
            throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));
        }
        
        return _databases.GetOrAdd(dbName, name => 
        {
            _logger.LogInformation("Creating database instance for {DatabaseName}", name);
            return _clientFactory.GetDatabase(name);
        });
    }
    
    /// <summary>
    /// Gets a MongoDB database for a tenant
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="databaseNameFormat">The database name format (default: {0}_db)</param>
    /// <returns>The MongoDB database</returns>
    public IMongoDatabase GetTenantDatabase(string tenantId, string databaseNameFormat = "{0}_db")
    {
        if (string.IsNullOrEmpty(tenantId))
        {
            throw new ArgumentException("Tenant ID cannot be null or empty", nameof(tenantId));
        }
        
        var sanitizedTenantId = SanitizeTenantId(tenantId);
        var dbName = string.Format(databaseNameFormat, sanitizedTenantId);
        
        return GetDatabase(dbName);
    }
    
    /// <summary>
    /// Gets a MongoDB collection for a specific database
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="collectionName">The collection name</param>
    /// <param name="databaseName">The database name (optional)</param>
    /// <returns>The MongoDB collection</returns>
    public IMongoCollection<TDocument> GetCollection<TDocument>(string collectionName, string databaseName = null)
    {
        var database = GetDatabase(databaseName);
        return database.GetCollection<TDocument>(collectionName);
    }
    
    /// <summary>
    /// Gets a MongoDB collection for a tenant
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="tenantId">The tenant ID</param>
    /// <param name="collectionName">The collection name</param>
    /// <param name="databaseNameFormat">The database name format (default: {0}_db)</param>
    /// <returns>The MongoDB collection</returns>
    public IMongoCollection<TDocument> GetTenantCollection<TDocument>(
        string tenantId, 
        string collectionName, 
        string databaseNameFormat = "{0}_db")
    {
        var database = GetTenantDatabase(tenantId, databaseNameFormat);
        return database.GetCollection<TDocument>(collectionName);
    }
    
    /// <summary>
    /// Sanitizes a tenant ID for use in database names
    /// </summary>
    /// <param name="tenantId">The tenant ID</param>
    /// <returns>The sanitized tenant ID</returns>
    private string SanitizeTenantId(string tenantId)
    {
        // Remove invalid characters for MongoDB database names
        // Only alphanumeric, underscore, and hyphen are allowed
        var sanitized = new string(tenantId
            .Where(c => char.IsLetterOrDigit(c) || c == '_' || c == '-')
            .ToArray());
            
        // Ensure it doesn't start with a number or special character
        if (sanitized.Length > 0 && !char.IsLetter(sanitized[0]))
        {
            sanitized = "db_" + sanitized;
        }
        
        return sanitized;
    }
}