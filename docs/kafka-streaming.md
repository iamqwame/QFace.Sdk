# QFace Kafka SDK

A robust, actor-based integration with Apache Kafka for .NET applications that provides a simple, declarative approach to working with event streaming and message processing.

## Features

- **Actor-based Architecture**: Built on Akka.NET for resilient, scalable message processing
- **Topic Groups**: Logical organization of topics for better configuration management
- **Batch Processing**: Configurable batch sizes and timeouts for optimal throughput
- **Automatic Offset Management**: At-least-once delivery with proper error handling
- **Configuration-driven**: No hardcoded topic names - everything configurable
- **Production Ready**: Connection resilience, automatic recovery, comprehensive logging

## Installation

```bash
dotnet add package QFace.Sdk.Kafka
```

## Quick Start

### 1. Configuration (appsettings.json)

```json
{
  "KafkaConsumerConfig": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "my-service-group",
    "TopicGroups": {
      "Analytics": ["user.events", "system.events"],
      "Orders": ["order.created", "order.updated", "order.cancelled"]
    },
    "ExtraProperties": {
      "auto.offset.reset": "latest"
    }
  },
  "KafkaProducerConfig": {
    "BootstrapServers": "localhost:9092"
  },
  "MessageGroupConsumerLogicConfig": {
    "TimeoutInMilliseconds": 5000,
    "MaxElements": 200
  }
}
```

### 2. Create a Consumer

```csharp
using QFace.Sdk.Kafka.Models;

public class AnalyticsConsumer : KafkaConsumerBase
{
    private readonly ILogger<AnalyticsConsumer> _logger;

    public AnalyticsConsumer(ILogger<AnalyticsConsumer> logger, ITopLevelActors topLevelActors)
    {
        _logger = logger;
    }

    [ConsumeTopic("Analytics")]
    public async Task HandleBulkMessages(List<EventSourceModel> messages)
    {
        _logger.LogInformation($"Processing {messages.Count} analytics events");
        
        foreach (var message in messages)
        {
            // Your business logic here
            _logger.LogDebug($"Processing event: {message.Id} of type {message.EventType}");
            
            // Example processing
            await ProcessAnalyticsEvent(message);
        }
    }

    private async Task ProcessAnalyticsEvent(EventSourceModel eventData)
    {
        // Implement your event processing logic
        await Task.CompletedTask;
    }

    public override Task ConsumingStopped()
    {
        _logger.LogWarning("Analytics consumer stopped");
        return Task.CompletedTask;
    }
}

// Event model
public class EventSourceModel
{
    public string Id { get; set; }
    public string EventType { get; set; }
    public DateTime Timestamp { get; set; }
    public string Source { get; set; }
    public object Data { get; set; }
    public Dictionary<string, string> Metadata { get; set; } = new();
}
```

### 3. Setup in Program.cs

```csharp
using QFace.Sdk.Kafka.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Kafka with consumer discovery
builder.Services.AddKafka(
    builder.Configuration,
    new[] { Assembly.GetExecutingAssembly() }
);

var app = builder.Build();

// Initialize Kafka
app.UseKafkaInApi();

app.Run();
```

### 4. Produce Messages

```csharp
public class EventService
{
    private readonly IKafkaProducer _producer;
    
    public EventService(IKafkaProducer producer)
    {
        _producer = producer;
    }
    
    public async Task PublishAnalyticsEvent(string eventType, object data)
    {
        var analyticsEvent = new EventSourceModel
        {
            Id = Guid.NewGuid().ToString(),
            EventType = eventType,
            Timestamp = DateTime.UtcNow,
            Source = "MyApplication",
            Data = data,
            Metadata = new Dictionary<string, string>
            {
                { "Version", "1.0" },
                { "Environment", "Production" }
            }
        };
        
        // Publish to analytics topic with event type as key for partitioning
        await _producer.ProduceAsync("analytics.events", analyticsEvent, key: eventType);
    }
}
```

## Advanced Usage

### Topic Groups

Organize related topics into logical groups:

```json
{
  "KafkaConsumerConfig": {
    "TopicGroups": {
      "UserAnalytics": [
        "user.registered",
        "user.profile.updated", 
        "user.preferences.changed"
      ],
      "SystemMonitoring": [
        "system.health.check",
        "system.performance.metrics",
        "system.error.alerts"
      ]
    }
  }
}
```

### Multiple Topic Handlers

```csharp
public class AnalyticsConsumer : KafkaConsumerBase
{
    [ConsumeTopic("UserAnalytics")]
    public async Task HandleUserEvents(List<UserEvent> events)
    {
        // Process user analytics
    }
    
    [ConsumeTopic("SystemMonitoring", MaxBatchSize = 50, BatchTimeoutMs = 2000)]
    public async Task HandleSystemEvents(List<SystemEvent> events)
    {
        // Process system events with custom batch settings
    }
    
    // Direct topic specification (bypasses topic groups)
    [ConsumeTopic("critical.alerts", "security.alerts")]
    public async Task HandleCriticalAlerts(List<AlertEvent> alerts)
    {
        // Handle critical alerts immediately
    }
}
```

### Batch Producer Operations

```csharp
public async Task PublishOrderUpdates(List<OrderUpdate> updates)
{
    await _producer.ProduceBatchAsync("order.updates", updates, 
        update => update.CustomerId); // Key selector for partitioning
}
```

### Consumer Lifecycle Hooks

```csharp
public class MyConsumer : KafkaConsumerBase
{
    public override Task ConsumingStarted()
    {
        _logger.LogInformation("Consumer started - initializing resources");
        return Task.CompletedTask;
    }
    
    public override Task PartitionsAssigned(List<TopicPartition> partitions)
    {
        _logger.LogInformation($"Assigned partitions: {string.Join(", ", partitions)}");
        return Task.CompletedTask;
    }
    
    public override Task ConsumingError(Exception exception)
    {
        _logger.LogError(exception, "Consumer error occurred");
        // Send alerts, update metrics, etc.
        return Task.CompletedTask;
    }
}
```

## Configuration Options

### Consumer Configuration

| Property | Description | Default |
|----------|-------------|---------|
| `BootstrapServers` | Kafka broker addresses | `localhost:9092` |
| `GroupId` | Consumer group identifier | `default-group` |
| `TopicGroups` | Named groups of topics | `{}` |
| `Topics` | Fallback topic list | `[]` |
| `MaxBatchSize` | Maximum messages per batch | `200` |
| `BatchTimeoutMs` | Batch timeout in milliseconds | `5000` |
| `EnableAutoCommit` | Enable automatic offset commits | `false` |
| `ExtraProperties` | Additional Kafka properties | `{}` |

### Producer Configuration

| Property | Description | Default |
|----------|-------------|---------|
| `BootstrapServers` | Kafka broker addresses | `localhost:9092` |
| `Acks` | Acknowledgment level | `1` |
| `Retries` | Number of retries | `3` |
| `CompressionType` | Message compression | `none` |
| `ExtraProperties` | Additional Kafka properties | `{}` |

### Batch Processing Configuration

| Property | Description | Default |
|----------|-------------|---------|
| `TimeoutInMilliseconds` | Batch processing timeout | `5000` |
| `MaxElements` | Maximum batch size | `200` |
| `CommitStrategy` | Offset commit strategy | `AfterSuccessfulProcessing` |

## Deployment Scenarios

### ASP.NET Core Web API

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddKafka(
    builder.Configuration,
    new[] { Assembly.GetExecutingAssembly() }
);

var app = builder.Build();
app.UseKafkaInApi();
app.Run();
```

### Console Application / Worker Service

```csharp
var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddKafka(
            context.Configuration,
            new[] { Assembly.GetExecutingAssembly() }
        );
    })
    .Build();

host.Services.UseKafkaInConsumer();
await host.RunAsync();
```

### Producer-Only Applications

```csharp
builder.Services.AddKafkaProducer();
builder.Services.Configure<KafkaProducerConfig>(
    builder.Configuration.GetSection("KafkaProducerConfig"));
```

## Error Handling & Resilience

### Automatic Recovery
- **Connection failures**: Automatic reconnection to Kafka brokers
- **Consumer failures**: Actor supervision restarts failed consumers
- **Serialization errors**: Logged and skipped (configurable)

### Offset Management
- **At-least-once delivery**: Offsets committed only after successful processing
- **Failed batch handling**: No offset commit on processing failure
- **Manual offset control**: Override automatic behavior when needed

### Dead Letter Queues

```csharp
[ConsumeTopic("Orders", DeadLetterTopic = "orders.failed")]
public async Task HandleOrders(List<Order> orders)
{
    // Failed messages automatically routed to dead letter topic
}
```

## Production Considerations

### Performance Tuning

```json
{
  "KafkaConsumerConfig": {
    "MaxBatchSize": 500,
    "BatchTimeoutMs": 1000,
    "ExtraProperties": {
      "fetch.min.bytes": "50000",
      "fetch.max.wait.ms": "500"
    }
  }
}
```

### Monitoring & Observability

The SDK provides comprehensive logging at different levels:
- `Information`: Batch processing, partition assignments
- `Debug`: Detailed message flow, offset commits
- `Error`: Processing failures, connection issues
- `Warning`: Consumer lifecycle events

### Scaling Considerations

- **Partition Strategy**: More partitions = more parallel consumers
- **Consumer Groups**: Multiple app instances automatically balance load
- **Batch Sizes**: Larger batches = higher throughput, higher latency
- **Actor Pools**: Configure actor pool sizes based on load

## Examples

See the [Examples](./Examples/) directory for:
- Government Analytics Consumer
- Multi-topic processing patterns
- Configuration examples
- Error handling strategies

## Contributing

1. Fork the repository
2. Create a feature branch
3. Write tests for your changes
4. Submit a pull request

## License

MIT License - see [LICENSE](LICENSE) file for details.

## Support

For issues and questions:
- GitHub Issues: [Report bugs or request features](https://github.com/iamqwame/qface-sdk/issues)
- Documentation: [Full documentation](https://github.com/iamqwame/qface-sdk/docs)

---

**QFace Kafka SDK** - Stream processing made simple with the power of actors.
