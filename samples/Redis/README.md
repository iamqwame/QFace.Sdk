# Redis Pub/Sub Demo

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
