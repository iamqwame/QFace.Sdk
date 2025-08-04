# QFace Kafka SDK Configuration Example

## appsettings.json Configuration

```json
{
  "KafkaConsumerConfig": {
    "BootstrapServers": "localhost:9092",
    "GroupId": "analytics-service-group",
    "TopicGroups": {
      "GovAnalytics": [
        "gov.events.topic",
        "audit.events.topic", 
        "compliance.events.topic"
      ],
      "UserAnalytics": [
        "user.created",
        "user.updated", 
        "user.deleted"
      ],
      "SystemEvents": [
        "system.health",
        "system.alerts",
        "system.metrics"
      ]
    },
    "Topics": [
      "fallback.topic"
    ],
    "ExtraProperties": {
      "auto.offset.reset": "latest",
      "enable.auto.commit": "false",
      "session.timeout.ms": "30000"
    },
    "MaxBatchSize": 200,
    "BatchTimeoutMs": 5000,
    "EnableAutoCommit": false,
    "SessionTimeoutMs": 30000,
    "MaxPollIntervalMs": 300000,
    "RetryCount": 3,
    "RetryIntervalMs": 1000
  },
  "KafkaProducerConfig": {
    "BootstrapServers": "localhost:9092",
    "ExtraProperties": {
      "acks": "1",
      "retries": "3",
      "batch.size": "16384",
      "linger.ms": "5"
    },
    "Acks": "1",
    "Retries": 3,
    "BatchSize": 16384,
    "LingerMs": 5,
    "CompressionType": "none",
    "ProducerInstances": 10,
    "ProducerUpperBound": 100
  },
  "MessageGroupConsumerLogicConfig": {
    "TimeoutInMilliseconds": 5000,
    "MaxElements": 200,
    "CommitStrategy": "AfterSuccessfulProcessing",
    "EnableManualOffsetManagement": false
  }
}
```

## Startup Configuration

### For ASP.NET Core Applications

```csharp
using QFace.Sdk.Kafka.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add Kafka with consumer assemblies
builder.Services.AddKafka(
    builder.Configuration,
    new[] { Assembly.GetExecutingAssembly() } // Assemblies containing your consumers
);

var app = builder.Build();

// Initialize Kafka in API
app.UseKafkaInApi();

app.Run();
```

### For Console Applications

```csharp
using QFace.Sdk.Kafka.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        services.AddKafka(
            context.Configuration,
            new[] { Assembly.GetExecutingAssembly() }
        );
    })
    .Build();

// Initialize Kafka for consumer applications
host.Services.UseKafkaInConsumer();

await host.RunAsync();
```

### Producer-Only Applications

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add only Kafka producer functionality
builder.Services.AddKafkaProducer();
builder.Services.Configure<KafkaProducerConfig>(
    builder.Configuration.GetSection("KafkaProducerConfig"));

var app = builder.Build();
app.UseKafkaInApi();
```

## Usage Examples

### Basic Consumer with Topic Group

```csharp
public class OrderEventsConsumer : KafkaConsumerBase
{
    private readonly ILogger<OrderEventsConsumer> _logger;

    public OrderEventsConsumer(ILogger<OrderEventsConsumer> logger, ITopLevelActors topLevelActors)
    {
        _logger = logger;
    }

    [ConsumeTopic("OrderEvents")] // References TopicGroups["OrderEvents"] from config
    public async Task HandleOrderEvents(List<OrderEvent> messages)
    {
        _logger.LogInformation($"Processing {messages.Count} order events");
        
        foreach (var orderEvent in messages)
        {
            // Process each order event
            await ProcessOrderEvent(orderEvent);
        }
    }
    
    private async Task ProcessOrderEvent(OrderEvent orderEvent)
    {
        // Your business logic
    }
}
```

### Advanced Consumer with Multiple Topic Handlers

```csharp
public class AnalyticsConsumer : KafkaConsumerBase
{
    [ConsumeTopic("UserAnalytics")]
    public async Task HandleUserEvents(List<UserEvent> events)
    {
        // Process user-related analytics
    }
    
    [ConsumeTopic("SystemEvents", MaxBatchSize = 50, BatchTimeoutMs = 2000)]
    public async Task HandleSystemEvents(List<SystemEvent> events)
    {
        // Process system events with custom batch settings
    }
    
    [ConsumeTopic("critical.alerts", "security.alerts")] // Direct topic specification
    public async Task HandleCriticalAlerts(List<AlertEvent> alerts)
    {
        // Handle critical alerts immediately
        foreach (var alert in alerts)
        {
            await SendImmediateNotification(alert);
        }
    }
}
```

### Producer Usage

```csharp
public class EventPublisher
{
    private readonly IKafkaProducer _producer;
    
    public EventPublisher(IKafkaProducer producer)
    {
        _producer = producer;
    }
    
    public async Task PublishUserEvent(UserCreatedEvent userEvent)
    {
        // Simple publish
        await _producer.ProduceAsync("user.created", userEvent);
    }
    
    public async Task PublishOrderEvent(OrderCreatedEvent orderEvent)
    {
        // Publish with key for partitioning
        await _producer.ProduceAsync("order.created", orderEvent, 
            key: orderEvent.CustomerId);
    }
    
    public async Task PublishOrderEventsWithKeySelector(List<OrderEvent> orders)
    {
        // Batch publish with key selector
        await _producer.ProduceBatchAsync("order.events", orders, 
            order => order.CustomerId);
    }
}
```

## Key Features

### Topic Groups
- **Logical organization**: Group related topics together
- **Environment flexibility**: Different topic mappings per environment  
- **Team ownership**: Different teams can own different topic groups

### Batch Processing
- **Configurable batching**: Control batch size and timeout per consumer
- **Automatic offset management**: Framework handles offset commits
- **Error handling**: Failed batches don't commit offsets

### Advanced Features
- **Partition awareness**: Access to partition and offset information
- **Manual offset control**: Override automatic offset management when needed
- **Dead letter topics**: Configure failed message routing
- **Consumer group coordination**: Automatic partition assignment and rebalancing

### Production Ready
- **Connection resilience**: Automatic connection recovery
- **Actor supervision**: Failed consumers are automatically restarted
- **Comprehensive logging**: Detailed logging for troubleshooting
- **Configuration validation**: Early validation of configuration issues
