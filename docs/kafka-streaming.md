# Kafka Streaming (producer only)

> **Consumers removed.** Standalone Kafka consumer hosts are retired.
> See [consumer-pipeline-removal.md](./consumer-pipeline-removal.md).
> QimERP cross-module sync uses Temporal workers in WebApi processes.

## Producer quick start

```csharp
builder.Services.AddKafkaProducer(builder.Configuration);
app.UseKafkaInApi();
```

```json
{
  "KafkaProducerConfig": {
    "BootstrapServers": "localhost:9092",
    "Acks": "all",
    "Retries": 3,
    "ProducerInstances": 10,
    "ProducerUpperBound": 100
  }
}
```

Publish via `IKafkaProducer`.
