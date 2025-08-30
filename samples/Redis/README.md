# Redis Pub/Sub Demo

This directory contains demo applications showing how to use the QFace.Sdk.RedisMq library.

## Quick Start

1. **Start Redis Server**
   ```bash
   redis-server
   ```

2. **Run the Consumer**
   ```bash
   cd Demo.Redis.Consumer
   dotnet run
   ```

3. **Run the API (in another terminal)**
   ```bash
   cd Demo.Redis.Api
   dotnet run
   ```

4. **Send a message**
   ```bash
   curl -X POST http://localhost:5000/api/messages \
     -H "Content-Type: application/json" \
     -d '{"content": "Hello Redis!"}'
   ```

## Features Demonstrated

- **Publisher**: API endpoint that publishes messages to Redis channels
- **Consumer**: Background service that subscribes to Redis channels and processes messages
- **Actor-based Architecture**: Uses Akka.NET actors for reliable message processing
- **Automatic Discovery**: Consumer methods are automatically discovered using attributes

## Configuration

Both applications use the same Redis configuration:

```json
{
  "RedisMq": {
    "ConnectionString": "localhost:6379",
    "Title": "Redis Demo Consumer",
    "RetryCount": 3,
    "RetryIntervalMs": 1000,
    "Database": 0
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
[Consumer("MessageConsumer")]
public class MessageConsumer
{
    [Channel("messages")]
    public async Task HandleMessage(MessageDto message)
    {
        Console.WriteLine($"Received: {message.Content}");
    }
}
```

## Publisher Example

```csharp
[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IRedisMqPublisher _publisher;

    public MessagesController(IRedisMqPublisher publisher)
    {
        _publisher = publisher;
    }

    [HttpPost]
    public async Task<IActionResult> SendMessage([FromBody] MessageDto message)
    {
        var success = await _publisher.PublishAsync(message, "messages");
        return success ? Ok() : BadRequest();
    }
}
```
