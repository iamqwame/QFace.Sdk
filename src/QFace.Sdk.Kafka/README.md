# QFace.Sdk.Kafka

Kafka **producer** helpers for .NET (Akka actor pool).

> **Migration note:** Message-consumer hosts (`UseKafkaInConsumer`, `KafkaConsumerBase`, consumer samples)
> were removed. QimERP cross-module sync uses **Temporal workers hosted in WebApi** processes.
> Do not add new Kafka consumer projects.

## Install

```bash
dotnet add package QFace.Sdk.Kafka
```

## Producer usage

```csharp
builder.Services.AddKafkaProducer(builder.Configuration);
// ...
app.UseKafkaInApi();
```

Config section: `KafkaProducerConfig` (`BootstrapServers`, etc.).
