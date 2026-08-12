# QFace SDK

A collection of .NET libraries designed to simplify complex development tasks and provide robust, scalable solutions for modern applications.

[![NuGet](https://img.shields.io/nuget/v/QFace.Sdk.ActorSystems.svg)](https://www.nuget.org/packages/QFace.Sdk.ActorSystems/)
[![Build Status](https://img.shields.io/github/workflow/status/qface/sdk/build)](https://github.com/qface/sdk/actions)
[![License](https://img.shields.io/github/license/qface/sdk)](LICENSE)

## Libraries

| Library                                          | Description                                                                              | NuGet                                                                                                                         |
| ------------------------------------------------ | ---------------------------------------------------------------------------------------- | ----------------------------------------------------------------------------------------------------------------------------- |
| [QFace.Sdk.ActorSystems](docs/actor-system.md)   | Lightweight wrapper around Akka.NET for actor-based systems                              | [![NuGet](https://img.shields.io/nuget/v/QFace.Sdk.ActorSystems.svg)](https://www.nuget.org/packages/QFace.Sdk.ActorSystems/) |
| [QFace.Sdk.AI](docs/ai-sdk.md)                   | Provider-agnostic LLM + forecasting SDK                                                  |                                                                                                                               |
| [QFace.Sdk.BlobStorage](docs/blob-storage.md)    | Blob/object storage helpers                                                              |                                                                                                                               |
| [QFace.Sdk.ElasticSearch](docs/elasticsearch.md) | Elasticsearch client helpers                                                             |                                                                                                                               |
| [QFace.Sdk.Extensions](docs/extensions.md)       | Common extension methods                                                                 |                                                                                                                               |
| [QFace.Sdk.Kafka](docs/kafka-streaming.md)       | Kafka producer/consumer helpers                                                          |                                                                                                                               |
| [QFace.Sdk.Logging](docs/logging.md)             | Structured logging helpers                                                               |                                                                                                                               |
| [QFace.Sdk.MongoDb](docs/mongodb.md)             | MongoDB data access (+ `QFace.Sdk.MongoDb.MultiTenant`)                                  |                                                                                                                               |
| QFace.Sdk.RedisCache                             | Redis caching helpers — see [docs](docs/redis-cache-remove-by-pattern-implementation.md) |                                                                                                                               |
| QFace.Sdk.RedisMq                                | Redis-backed message queue                                                               |                                                                                                                               |
| [QFace.Sdk.SendMessage](docs/send-message.md)    | Send-message abstractions                                                                |                                                                                                                               |
| QFace.Sdk.Temporal                               | Temporal helpers (see `QimErp.Shared.Common` usage)                                      |                                                                                                                               |
| QFace.WebApi.Template                            | Project template for new WebApi SDK consumers                                            |                                                                                                                               |
| QFace.Sdk.Messaging                              | Unified API for message queues / event brokers                                           | _Coming soon_                                                                                                                 |
| QFace.Sdk.DataAccess                             | Flexible data access layer supporting multiple databases                                 | _Coming soon_                                                                                                                 |

> `QimErp.Shared.Common` and `QimErp.Shared.DemoData` also build out of this repo's `src/`, but they are QimERP platform libraries, not QFace SDK — see the platform docs (`../docs/shared/MODULES.md`).

## Quick Start

### Actor System SDK

```bash
dotnet add package QFace.Sdk.ActorSystems
```

```csharp
// Register actor system
builder.Services.AddActorSystem(
    new[] { typeof(Program).Assembly },
    config =>
    {
        config.SystemName = "MySystem";
        config.AddActorType<MyActor>();
    },
    addLifecycle: true);

// Use in your service
public class MyService
{
    private readonly IActorService _actorService;

    public MyService(IActorService actorService)
    {
        _actorService = actorService;
    }

    public void DoSomething()
    {
        _actorService.Tell<MyActor>(new MyMessage());
    }
}
```

## Documentation

For detailed documentation, visit:

- [SDK Overview](docs/index.md)
- [Actor System SDK](docs/actor-system.md)
- [Installation Guide](docs/shared/installation.md)
- [Configuration Patterns](docs/shared/configuration.md)

## Samples

Check out the [samples](samples) directory for complete working examples of each SDK.

## Requirements

- .NET 6.0 or later
- For Actor System SDK: Akka.NET 1.4.46 or later

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request
