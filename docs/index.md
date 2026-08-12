# QFace SDK Documentation

Welcome to the QFace SDK documentation. QFace offers a collection of .NET libraries designed to simplify complex development tasks and provide robust, scalable solutions for modern applications.

## Available SDKs

### [Actor System SDK](actor-system.md)

A lightweight wrapper around Akka.NET that simplifies creating and managing actor-based systems in .NET applications. Ideal for building highly concurrent, distributed, and resilient applications.

### [Messaging SDK](rabbitmq-messaging.md)

Provides a unified API for working with various message queues and event brokers. Simplifies publishing, subscribing, and processing messages across different messaging technologies.

### Data Access SDK

A flexible data access layer supporting multiple databases, a clean repository pattern with built-in caching and performance optimizations. **Coming soon** — no package or dedicated doc exists yet.

### [AI/ML SDK](ai-sdk.md)

A generic AI/ML SDK library supporting multiple LLM providers (OpenAI, Anthropic, Google Gemini), forecasting algorithms (Trend, Regression, ML, Manual), and skills analysis. Designed to be extensible and provider-agnostic.

## Other libraries

| Library       | Doc                                                                                                | Status    |
| ------------- | -------------------------------------------------------------------------------------------------- | --------- |
| Blob Storage  | [blob-storage.md](blob-storage.md)                                                                 | Published |
| ElasticSearch | [elasticsearch.md](elasticsearch.md)                                                               | Published |
| Extensions    | [extensions.md](extensions.md)                                                                     | Published |
| Kafka         | [kafka-streaming.md](kafka-streaming.md)                                                           | Published |
| Logging       | [logging.md](logging.md)                                                                           | Published |
| MongoDB       | [mongodb.md](mongodb.md)                                                                           | Published |
| Redis Cache   | [redis-cache-remove-by-pattern-implementation.md](redis-cache-remove-by-pattern-implementation.md) | Published |
| SendMessage   | [send-message.md](send-message.md)                                                                 | Published |

> Note: `QimErp.Shared.Common` and `QimErp.Shared.DemoData` also build out of this repo's `src/` but are QimERP platform libraries, not QFace SDK — see the platform docs instead.

## Getting Started

For all SDKs, see the [installation](shared/installation.md) guide to get started with adding QFace packages to your project.

Each SDK has its own dedicated documentation page with detailed information on configuration, usage examples, and API references.

## Common Patterns

All QFace SDKs follow consistent patterns for:

- Integration with dependency injection
- Configuration and options
- Logging and telemetry
- Testing

Learn more about these patterns in the [configuration](shared/configuration.md) guide.

## Samples and Examples

Explore the `/samples` directory in the GitHub repository for complete working examples of each SDK in action.

## Feedback and Contributions

We welcome feedback and contributions! Please open an issue or pull request on our [GitHub repository](https://github.com/qface/sdk).

## License

All QFace SDKs are available under the MIT License. See the LICENSE file for more information.
