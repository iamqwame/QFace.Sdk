# Redis Cache API Demo

This demo showcases the **QFace.Sdk.RedisCache** library with support for both **Upstash Redis** (HTTP) and **StackExchange.Redis** (TCP) providers.

## Quick Start

### 1. Start Redis Server (for StackExchange provider)

```bash
redis-server
```

### 2. Run the API

```bash
cd Demo.RedisCache.Api
dotnet run
```

The API will be available at:
- HTTP: `http://localhost:5000`
- Swagger UI: `http://localhost:5000/swagger`

### 3. Test the Endpoints

Use the provided `Demo.RedisCache.Api.http` file or test with curl:

```bash
# Set a value
curl -X POST http://localhost:5000/cache/mykey \
  -H "Content-Type: application/json" \
  -d '{"value": "Hello Cache!", "expirationMinutes": 60}'

# Get a value
curl http://localhost:5000/cache/mykey

# Get user (demonstrates GetOrSet pattern)
curl http://localhost:5000/users/123
```

## Available Endpoints

### Basic Cache Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/cache/{key}` | Get value from cache |
| `POST` | `/cache/{key}` | Set value in cache |
| `DELETE` | `/cache/{key}` | Remove key from cache |
| `GET` | `/cache/{key}/exists` | Check if key exists and get TTL |

### User Cache (GetOrSet Pattern)

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/users/{id}` | Get user (automatically caches if not found) |

### Hash Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/cache/{key}/hash/{field}` | Get hash field value |
| `POST` | `/cache/{key}/hash/{field}` | Set hash field value |
| `GET` | `/cache/{key}/hash` | Get all hash fields |

### List Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/cache/{key}/list` | Add value to list |
| `GET` | `/cache/{key}/list?start=0&stop=-1` | Get list range |

### Set Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/cache/{key}/set` | Add value to set |
| `GET` | `/cache/{key}/set` | Get all set members |

### Batch Operations

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/cache/batch` | Set multiple values |
| `GET` | `/cache/batch?keys=key1&keys=key2` | Get multiple values |

## Configuration

### StackExchange Provider (Default)

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

### Upstash Provider

To use Upstash, update `appsettings.json`:

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

## Features Demonstrated

1. **Multi-Provider Support**: Switch between providers via configuration
2. **Basic Caching**: Get, Set, Delete operations with expiration
3. **GetOrSet Pattern**: Automatic caching with factory functions
4. **Hash Operations**: Store and retrieve hash data structures
5. **List Operations**: Push, pop, and range operations
6. **Set Operations**: Add, check membership, get all members
7. **Batch Operations**: Efficient bulk operations
8. **TTL Management**: Expiration and time-to-live queries

## Example Usage

### Basic Operations

```csharp
// Set a value
await cache.SetAsync("mykey", "myvalue", TimeSpan.FromMinutes(30));

// Get a value
var value = await cache.GetAsync<string>("mykey");

// Remove a key
await cache.RemoveAsync("mykey");
```

### GetOrSet Pattern (Recommended)

```csharp
// Automatically caches if not found
var user = await cache.GetOrSetAsync(
    $"user:{userId}",
    async () => await _dbContext.Users.FindAsync(userId),
    TimeSpan.FromHours(1)
);
```

### Hash Operations

```csharp
// Set hash field
await cache.HashSetAsync("user:123", "email", "user@example.com");

// Get hash field
var email = await cache.HashGetAsync<string>("user:123", "email");

// Get all hash fields
var profile = await cache.HashGetAllAsync<string>("user:123");
```

## Testing

Use the provided `Demo.RedisCache.Api.http` file with your IDE's HTTP client support, or use curl/Postman to test the endpoints.

## Switching Providers

You can easily switch between providers by changing the `Provider` value in `appsettings.json`:

- `"Provider": "StackExchange"` - Uses local Redis via TCP
- `"Provider": "Upstash"` - Uses Upstash Redis via HTTP

No code changes required!

