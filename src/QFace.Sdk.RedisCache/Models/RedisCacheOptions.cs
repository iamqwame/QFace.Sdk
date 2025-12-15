namespace QFace.Sdk.RedisCache.Models;

/// <summary>
/// Configuration options for Redis Cache
/// </summary>
public class RedisCacheOptions
{
    /// <summary>
    /// Redis provider type: "Upstash" or "StackExchange"
    /// </summary>
    public RedisProvider Provider { get; set; } = RedisProvider.StackExchange;
    
    /// <summary>
    /// Optional prefix for all cache keys
    /// </summary>
    public string KeyPrefix { get; set; } = "";
    
    /// <summary>
    /// Default expiration time for cached items
    /// </summary>
    public TimeSpan DefaultExpiration { get; set; } = TimeSpan.FromMinutes(30);
    
    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectionTimeout { get; set; } = 30000;
    
    /// <summary>
    /// Upstash-specific configuration options
    /// </summary>
    public UpstashOptions Upstash { get; set; } = new();
    
    /// <summary>
    /// StackExchange.Redis-specific configuration options
    /// </summary>
    public StackExchangeOptions StackExchange { get; set; } = new();
}

/// <summary>
/// Configuration options for Upstash Redis provider
/// </summary>
public class UpstashOptions
{
    /// <summary>
    /// Upstash Redis REST API URL
    /// </summary>
    public string Url { get; set; }
    
    /// <summary>
    /// Upstash API token
    /// </summary>
    public string Token { get; set; }
    
    /// <summary>
    /// Maximum number of retry attempts
    /// </summary>
    public int RetryCount { get; set; } = 3;
    
    /// <summary>
    /// Base delay in milliseconds for exponential backoff retries
    /// </summary>
    public int RetryBaseDelayMs { get; set; } = 1000;
}

/// <summary>
/// Configuration options for StackExchange.Redis provider
/// </summary>
public class StackExchangeOptions
{
    /// <summary>
    /// Redis connection string (e.g., "localhost:6379" or "host:port,password=xxx")
    /// </summary>
    public string ConnectionString { get; set; } = "localhost:6379";
    
    /// <summary>
    /// Redis database number (0-15)
    /// </summary>
    public int Database { get; set; } = 0;
    
    /// <summary>
    /// Connection timeout in milliseconds
    /// </summary>
    public int ConnectTimeout { get; set; } = 5000;
    
    /// <summary>
    /// Sync operation timeout in milliseconds
    /// </summary>
    public int SyncTimeout { get; set; } = 5000;
    
    /// <summary>
    /// Whether to abort connection attempts if connection fails
    /// </summary>
    public bool AbortOnConnectFail { get; set; } = false;
}

/// <summary>
/// Redis provider types
/// </summary>
public enum RedisProvider
{
    /// <summary>
    /// Upstash Redis (HTTP/REST API)
    /// </summary>
    Upstash,
    
    /// <summary>
    /// Standard Redis using StackExchange.Redis (TCP)
    /// </summary>
    StackExchange
}

