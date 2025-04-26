namespace QFace.Sdk.MongoDb.Config;

/// <summary>
/// Configuration options for MongoDB connections
/// </summary>
public class MongoDbOptions
{
    /// <summary>
    /// Connection string to the MongoDB server
    /// </summary>
    public string ConnectionString { get; set; }
    
    /// <summary>
    /// Database name to connect to
    /// </summary>
    public string DatabaseName { get; set; }
    
    /// <summary>
    /// Optional username if not included in connection string
    /// </summary>
    public string Username { get; set; }
    
    /// <summary>
    /// Optional password if not included in connection string
    /// </summary>
    public string Password { get; set; }
    
    /// <summary>
    /// Authentication database (usually 'admin')
    /// </summary>
    public string AuthDatabase { get; set; } = "admin";
    
    /// <summary>
    /// Timeout in seconds for connection attempts
    /// </summary>
    public int ConnectionTimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Maximum number of connection attempts
    /// </summary>
    public int MaxConnectionAttempts { get; set; } = 3;
    
    /// <summary>
    /// Whether to use SSL for the connection
    /// </summary>
    public bool UseSsl { get; set; } = false;
    
    /// <summary>
    /// Collection naming conventions configuration
    /// </summary>
    public CollectionNamingOptions CollectionNaming { get; set; } = new CollectionNamingOptions();
    
    /// <summary>
    /// Connection pool settings
    /// </summary>
    public ConnectionPoolOptions ConnectionPool { get; set; } = new ConnectionPoolOptions();
    
    /// <summary>
    /// Validates that required options are set
    /// </summary>
    /// <returns>True if valid, false otherwise</returns>
    [JsonIgnore]
    public bool IsValid => !string.IsNullOrEmpty(ConnectionString) && !string.IsNullOrEmpty(DatabaseName);
}

/// <summary>
/// Configuration options for MongoDB collection naming conventions
/// </summary>
public class CollectionNamingOptions
{
    /// <summary>
    /// Collection naming strategy (Plural, CamelCase, PluralCamelCase, Raw)
    /// </summary>
    public string Strategy { get; set; } = "Plural";
    
    /// <summary>
    /// Whether to force all collections to lowercase
    /// </summary>
    public bool ForceLowerCase { get; set; } = true;
}

/// <summary>
/// Configuration options for MongoDB connection pool
/// </summary>
public class ConnectionPoolOptions
{
    /// <summary>
    /// Maximum size of the connection pool
    /// </summary>
    public int MaxSize { get; set; } = 100;
    
    /// <summary>
    /// Minimum size of the connection pool
    /// </summary>
    public int MinSize { get; set; } = 10;
    
    /// <summary>
    /// Maximum lifetime of a connection in minutes
    /// </summary>
    public int MaxConnectionLifeTimeMinutes { get; set; } = 30;
    
    /// <summary>
    /// Timeout in milliseconds for waiting for a connection
    /// </summary>
    public int WaitQueueTimeoutMilliseconds { get; set; } = 5000;
}