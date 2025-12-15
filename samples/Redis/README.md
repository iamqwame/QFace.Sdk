# Redis SDK Demos

This directory contains demo applications showing how to use the QFace Redis SDKs:
- **QFace.Sdk.RedisMq** - Redis Pub/Sub messaging
- **QFace.Sdk.RedisCache** - Redis caching with multi-provider support

---

## Redis Cache Demo (QFace.Sdk.RedisCache)

### Quick Start

1. **Start Redis Server** (for StackExchange provider)
   ```bash
   redis-server
   ```

2. **Run the Cache API**
   ```bash
   cd Demo.RedisCache.Api
   dotnet run
   ```

3. **Test the endpoints**
   ```bash
   # Set a value
   curl -X POST http://localhost:5000/cache/mykey \
     -H "Content-Type: application/json" \
     -d '{"value": "Hello Cache!", "expirationMinutes": 60}'
   
   # Get a value
   curl http://localhost:5000/cache/mykey
   ```

### Available Endpoints

#### Basic Operations
- `GET /cache/{key}` - Get value from cache
- `POST /cache/{key}` - Set value in cache
- `DELETE /cache/{key}` - Remove key from cache
- `GET /cache/{key}/exists` - Check if key exists and get TTL

#### User Cache (GetOrSet Pattern)
- `GET /users/{id}` - Get user (caches automatically)

#### Hash Operations
- `GET /cache/{key}/hash/{field}` - Get hash field value
- `POST /cache/{key}/hash/{field}` - Set hash field value
- `GET /cache/{key}/hash` - Get all hash fields

#### List Operations
- `POST /cache/{key}/list` - Add value to list
- `GET /cache/{key}/list?start=0&stop=-1` - Get list range

#### Set Operations
- `POST /cache/{key}/set` - Add value to set
- `GET /cache/{key}/set` - Get all set members

#### Batch Operations
- `POST /cache/batch` - Set multiple values
- `GET /cache/batch?keys=key1&keys=key2` - Get multiple values

### Configuration

#### StackExchange Provider (Default)
```json
{
  "RedisCache": {
    "Provider": "StackExchange",
    "KeyPrefix": "demo:cache:",
    "DefaultExpiration": "00:30:00",
    "StackExchange": {
      "ConnectionString": "localhost:6379",
      "Database": 0
    }
  }
}
```

#### Upstash Provider
```json
{
  "RedisCache": {
    "Provider": "Upstash",
    "KeyPrefix": "demo:cache:",
    "DefaultExpiration": "00:30:00",
    "Upstash": {
      "Url": "https://your-redis.upstash.io",
      "Token": "your-upstash-token"
    }
  }
}
```

### Features Demonstrated

- ✅ **Multi-Provider Support**: Switch between Upstash and StackExchange.Redis
- ✅ **Basic Caching**: Get, Set, Delete operations
- ✅ **GetOrSet Pattern**: Automatic caching with factory functions
- ✅ **Hash Operations**: Store and retrieve hash data structures
- ✅ **List Operations**: Push, pop, and range operations
- ✅ **Set Operations**: Add, check membership, get all members
- ✅ **Batch Operations**: Efficient bulk operations
- ✅ **TTL Management**: Expiration and time-to-live queries

### Usage Example

```csharp
// GetOrSet pattern (recommended)
var user = await _cache.GetOrSetAsync(
    $"user:{userId}",
    async () => await _dbContext.Users.FindAsync(userId),
    TimeSpan.FromHours(1)
);

// Basic operations
await _cache.SetAsync("key", "value", TimeSpan.FromMinutes(30));
var value = await _cache.GetAsync<string>("key");
await _cache.RemoveAsync("key");
```

---

## Redis Pub/Sub Demo (QFace.Sdk.RedisMq)

This directory contains demo applications showing how to use the QFace.Sdk.RedisMq library.

## Quick Start

1. **Start Redis Server**
   ```bash
   redis-server
   ```

2. **Run the Consumer**
   ```bash
   cd Demo.RedisMq.Consumer
   dotnet run
   ```

3. **Run the API (in another terminal)**
   ```bash
   cd Demo.RedisMq.Api
   dotnet run
   ```

4. **Send a message**
   ```bash
   curl -X POST http://localhost:5000/publish \
     -H "Content-Type: application/json" \
     -d '{"content": "Hello Redis!"}'
   ```

## Available Endpoints

- `POST /publish` - Send message to `demo_channel`
- `POST /publish-important` - Send message to `important_messages` channel
- `POST /publish-normal` - Send message to `normal_messages` channel

## Features Demonstrated

- **Publisher**: API endpoint that publishes messages to Redis channels
- **Consumer**: Background service that subscribes to Redis channels and processes messages
- **Actor-based Architecture**: Uses Akka.NET actors for reliable message processing
- **Automatic Discovery**: Consumer methods are automatically discovered using attributes
- **Multiple Channels**: Support for different message types on different channels

## Configuration

Both applications use the same Redis configuration:

```json
{
  "RedisMq": {
    "ConnectionString": "localhost:6379",
    "Title": "Redis Demo Consumer",
    "RetryCount": 3,
    "RetryIntervalMs": 1000,
    "Database": 0,
    "ConnectTimeout": 5000,
    "SyncTimeout": 5000,
    "AbortOnConnectFail": false
  }
}
```

## Message Flow

1. API receives HTTP request
2. API publishes message to Redis channel using `IRedisMqPublisher`
3. Consumer subscribes to Redis channel
4. Consumer processes message using actor-based architecture
5. Message is logged to console

## Consumer Example

```csharp
[Consumer("DemoMessageConsumer")]
public class MessageConsumer
{
    [Channel("important_messages")]
    public async Task HandleImportantMessage(MessageDto message)
    {
        _logger.LogInformation("=== IMPORTANT MESSAGE RECEIVED ===");
        _logger.LogInformation($"ID: {message.Id}");
        _logger.LogInformation($"Content: {message.Content}");
        _logger.LogInformation($"Timestamp: {message.Timestamp}");
        _logger.LogInformation("=================================");
        
        await Task.Delay(500);
    }

    [Channel("normal_messages")]
    public async Task HandleNormalMessage(MessageDto message)
    {
        _logger.LogInformation("--- NORMAL MESSAGE RECEIVED ---");
        _logger.LogInformation($"ID: {message.Id}");
        _logger.LogInformation($"Content: {message.Content}");
        _logger.LogInformation($"Timestamp: {message.Timestamp}");
        _logger.LogInformation("-----------------------------");
        
        await Task.Delay(300);
    }

    [Channel("demo_channel")]
    public async Task HandleDemoMessage(MessageDto message)
    {
        _logger.LogInformation(">>> DEMO CHANNEL MESSAGE <<<");
        _logger.LogInformation($"ID: {message.Id}");
        _logger.LogInformation($"Content: {message.Content}");
        _logger.LogInformation($"Timestamp: {message.Timestamp}");
        _logger.LogInformation(">>>>>>>>>>>>>>><<<<<<<<<<<<<<<");
        
        await Task.Delay(100);
    }
}
```

## Publisher Example

```csharp
// Minimal API endpoint
app.MapPost("/publish", async (
    [FromBody] MessageDto message,
    [FromServices] IRedisMqPublisher publisher) =>
{
    try
    {
        bool success = await publisher.PublishAsync(message, "demo_channel");
        return Results.Ok(new { success, message = "Message published successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});
```

## Testing with HTTP Files

Use the provided `Demo.RedisMq.Api.http` file to test the endpoints with your IDE's HTTP client support.

## Architecture

The demo shows:
- **Redis Pub/Sub**: Using StackExchange.Redis for Redis connectivity
- **Actor System**: Akka.NET actors for reliable message processing
- **Dependency Injection**: Full .NET Core DI integration
- **Automatic Discovery**: Consumer methods discovered via attributes
- **Error Handling**: Built-in retry logic and error handling
