# QFace.Sdk.RedisMq

A robust, actor-based integration with Redis pub/sub for .NET applications that provides a simple, declarative approach to working with message queues.

## Features

- **Actor-based Architecture**: Built on top of Akka.NET for reliable message processing
- **Redis Pub/Sub**: Full support for Redis publish/subscribe messaging
- **Declarative Consumers**: Simple attribute-based consumer registration
- **Automatic Discovery**: Automatic discovery of consumer classes and methods
- **Retry Logic**: Built-in retry mechanisms for failed message publishing
- **Dependency Injection**: Full integration with .NET Core DI container
- **Logging**: Comprehensive logging throughout the message processing pipeline

## Quick Start

### 1. Install the Package

```bash
dotnet add package QFace.Sdk.RedisMq
```

### 2. Configure Redis Connection

Add to your `appsettings.json`:

```json
{
  "RedisMq": {
    "ConnectionString": "localhost:6379",
    "Title": "My Redis Consumer",
    "RetryCount": 5,
    "RetryIntervalMs": 1000,
    "Database": 0
  }
}
```

### 3. Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
// For API applications
builder.Services.AddRedisMq(builder.Configuration, new[] { Assembly.GetExecutingAssembly() });

// For consumer applications
builder.Services.AddRedisMq(builder.Configuration, new[] { Assembly.GetExecutingAssembly() });
```

### 4. Initialize in API Applications

```csharp
app.UseRedisMqInApi();
```

### 5. Initialize in Consumer Applications

```csharp
var serviceProvider = builder.Build();
serviceProvider.UseRedisMqInConsumer();
```

## Usage

### Publishing Messages

```csharp
public class MyController : ControllerBase
{
    private readonly IRedisMqPublisher _publisher;

    public MyController(IRedisMqPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MyMessage message)
    {
        var success = await _publisher.PublishAsync(message, "my-channel");
        return success ? Ok() : BadRequest();
    }
}
```

### Consuming Messages

```csharp
[Consumer("MyConsumer")]
public class MyMessageConsumer
{
    [Channel("my-channel")]
    public async Task HandleMessage(MyMessage message)
    {
        // Process the message
        Console.WriteLine($"Received: {message.Content}");
    }
}
```

## Configuration Options

| Option | Description | Default |
|--------|-------------|---------|
| `ConnectionString` | Redis connection string | `localhost:6379` |
| `Title` | Consumer application title | - |
| `RetryCount` | Number of retry attempts | `5` |
| `RetryIntervalMs` | Delay between retries in milliseconds | `1000` |
| `Database` | Redis database number | `0` |
| `ConnectTimeout` | Connection timeout in milliseconds | `5000` |
| `SyncTimeout` | Sync timeout in milliseconds | `5000` |
| `AbortOnConnectFail` | Whether to abort on connection failure | `false` |

## Architecture

The library uses an actor-based architecture with the following components:

- **RedisMqPublisherActor**: Handles message publishing with retry logic
- **RedisMqConsumerActor**: Processes individual messages
- **RedisMqConsumerSupervisorActor**: Manages consumer actors and Redis subscriptions
- **RedisMqConnectionProvider**: Manages Redis connections

## License

MIT License - see LICENSE file for details.
