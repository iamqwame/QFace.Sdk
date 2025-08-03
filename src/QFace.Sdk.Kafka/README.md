# QFace.Sdk.Kafka

A robust, actor-based integration with Apache Kafka for .NET applications that provides a simple, declarative approach to working with event streaming.

## Installation

```bash
dotnet add package QFace.Sdk.Kafka
```

## Quick Example

```csharp
// Consumer
public class AnalyticsConsumer : KafkaConsumerBase
{
    public AnalyticsConsumer(ILogger<AnalyticsConsumer> logger, ITopLevelActors topLevelActors) { }

    [ConsumeTopic("Analytics")]
    public async Task HandleBulkMessages(List<EventSourceModel> messages)
    {
        // Process batch of analytics events
    }
}

// Startup
builder.Services.AddKafka(configuration, new[] { Assembly.GetExecutingAssembly() });
app.UseKafkaInApi();

// Producer
await kafkaProducer.ProduceAsync("analytics.events", eventData, key: eventType);
```

## Documentation

For comprehensive documentation, examples, and configuration options, see:
- [Kafka Streaming Guide](../docs/kafka-streaming.md)
- [API Reference](../docs/shared/)

## Features

- **Topic Groups**: Logical organization of topics for configuration management
- **Batch Processing**: Configurable batch sizes and timeouts
- **Actor-based**: Built on Akka.NET for resilience and scalability  
- **At-least-once Delivery**: Proper offset management with error handling
- **Configuration-driven**: No hardcoded topics, fully configurable

## License

MIT
