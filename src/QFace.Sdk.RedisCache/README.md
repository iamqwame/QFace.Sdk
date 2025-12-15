# QFace Redis Cache SDK

A Redis caching SDK for .NET applications supporting both **Upstash Redis** (HTTP/REST API) and **standard Redis** (StackExchange.Redis TCP connection). Switch between providers seamlessly via configuration.

## Features

- ✅ **Multi-Provider Support**: Switch between Upstash and StackExchange.Redis
- ✅ **Unified API**: Same interface regardless of provider
- ✅ **Type-Safe**: Generic methods for type-safe caching
- ✅ **Async/Await**: Full async support
- ✅ **Key Prefixing**: Automatic key prefixing support
- ✅ **TTL Management**: Built-in expiration handling
- ✅ **Advanced Operations**: Hash, List, Set operations
- ✅ **GetOrSet Pattern**: Cache-aside pattern support

## Installation

```bash
dotnet add package QFace.Sdk.RedisCache
```

## Quick Start

### 1. Configuration

#### For Upstash Redis:
```json
{
  "RedisCache": {
    "Provider": "Upstash",
    "KeyPrefix": "app:cache:",
    "DefaultExpiration": "00:30:00",
    "ConnectionTimeout": 30000,
    "Upstash": {
      "Url": "https://your-redis.upstash.io",
      "Token": "your-upstash-token",
      "RetryCount": 3,
      "RetryBaseDelayMs": 1000
    }
  }
}
```

#### For Standard Redis (StackExchange):
```json
{
  "RedisCache": {
    "Provider": "StackExchange",
    "KeyPrefix": "app:cache:",
    "DefaultExpiration": "00:30:00",
    "ConnectionTimeout": 30000,
    "StackExchange": {
      "ConnectionString": "localhost:6379,password=yourpassword",
      "Database": 0,
      "ConnectTimeout": 5000,
      "SyncTimeout": 5000,
      "AbortOnConnectFail": false
    }
  }
}
```

### 2. Register Services

```csharp
// In Program.cs or Startup.cs
builder.Services.AddRedisCache(builder.Configuration);
```

### 3. Use in Your Services

```csharp
public class UserService
{
    private readonly IRedisCacheService _cache;
    
    public UserService(IRedisCacheService cache)
    {
        _cache = cache;
    }
    
    // Simple get/set
    public async Task<User> GetUserAsync(int userId)
    {
        var cacheKey = $"user:{userId}";
        var cached = await _cache.GetAsync<User>(cacheKey);
        
        if (cached != null) return cached;
        
        // Get from database
        var user = await _dbContext.Users.FindAsync(userId);
        
        // Cache for 1 hour
        await _cache.SetAsync(cacheKey, user, TimeSpan.FromHours(1));
        
        return user;
    }
    
    // GetOrSet pattern (recommended)
    public async Task<User> GetUserAsync(int userId)
    {
        return await _cache.GetOrSetAsync(
            $"user:{userId}",
            async () => await _dbContext.Users.FindAsync(userId),
            TimeSpan.FromHours(1)
        );
    }
}
```

## API Reference

### Basic Operations

```csharp
// Get value
var user = await _cache.GetAsync<User>("user:123");

// Set value
await _cache.SetAsync("user:123", user, TimeSpan.FromHours(1));

// Remove key
await _cache.RemoveAsync("user:123");

// Check existence
var exists = await _cache.ExistsAsync("user:123");
```

### Batch Operations

```csharp
// Get multiple values
var users = await _cache.GetManyAsync<User>("user:1", "user:2", "user:3");

// Set multiple values
var items = new Dictionary<string, User>
{
    { "user:1", user1 },
    { "user:2", user2 }
};
await _cache.SetManyAsync(items, TimeSpan.FromHours(1));

// Remove multiple keys
await _cache.RemoveManyAsync("user:1", "user:2", "user:3");
```

### Advanced Operations

```csharp
// Set if not exists
var wasSet = await _cache.SetIfNotExistsAsync("key", value, TimeSpan.FromMinutes(30));

// Get time to live
var ttl = await _cache.GetTimeToLiveAsync("key");

// Extend expiration
await _cache.ExtendExpirationAsync("key", TimeSpan.FromHours(2));
```

### Hash Operations

```csharp
// Get hash field
var email = await _cache.HashGetAsync<string>("user:123", "email");

// Set hash field
await _cache.HashSetAsync("user:123", "email", "user@example.com");

// Get all hash fields
var profile = await _cache.HashGetAllAsync<string>("user:123");
```

### List Operations

```csharp
// Push to list
await _cache.ListPushAsync("notifications:123", notification);

// Pop from list
var notification = await _cache.ListPopAsync<Notification>("notifications:123");

// Get range
var notifications = await _cache.ListGetRangeAsync<Notification>("notifications:123", 0, 10);
```

### Set Operations

```csharp
// Add to set
await _cache.SetAddAsync("tags:post:123", "technology");

// Check membership
var hasTag = await _cache.SetContainsAsync("tags:post:123", "technology");

// Get all members
var tags = await _cache.SetGetAllAsync<string>("tags:post:123");
```

## Configuration Options

### Common Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Provider` | `RedisProvider` | `StackExchange` | Provider type: `Upstash` or `StackExchange` |
| `KeyPrefix` | `string` | `""` | Prefix for all cache keys |
| `DefaultExpiration` | `TimeSpan` | `30 minutes` | Default TTL for cached items |
| `ConnectionTimeout` | `int` | `30000` | Connection timeout in milliseconds |

### Upstash Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `Url` | `string` | Required | Upstash Redis REST API URL |
| `Token` | `string` | Required | Upstash API token |
| `RetryCount` | `int` | `3` | Maximum retry attempts |
| `RetryBaseDelayMs` | `int` | `1000` | Base delay for exponential backoff |

### StackExchange Options

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ConnectionString` | `string` | `"localhost:6379"` | Redis connection string |
| `Database` | `int` | `0` | Redis database number (0-15) |
| `ConnectTimeout` | `int` | `5000` | Connection timeout in milliseconds |
| `SyncTimeout` | `int` | `5000` | Sync operation timeout in milliseconds |
| `AbortOnConnectFail` | `bool` | `false` | Abort on connection failure |

## Switching Providers

You can easily switch between providers by changing the configuration:

```json
// Development - use local Redis
{
  "RedisCache": {
    "Provider": "StackExchange",
    "StackExchange": {
      "ConnectionString": "localhost:6379"
    }
  }
}

// Production - use Upstash
{
  "RedisCache": {
    "Provider": "Upstash",
    "Upstash": {
      "Url": "https://your-redis.upstash.io",
      "Token": "your-token"
    }
  }
}
```

## Best Practices

1. **Use GetOrSet Pattern**: Prefer `GetOrSetAsync` for cache-aside pattern
2. **Set Appropriate TTLs**: Don't cache forever - set reasonable expiration times
3. **Use Key Prefixes**: Organize keys with prefixes (e.g., `user:`, `product:`)
4. **Handle Null Values**: Check for null after `GetAsync`
5. **Batch Operations**: Use `GetManyAsync` and `SetManyAsync` for better performance

## License

MIT

