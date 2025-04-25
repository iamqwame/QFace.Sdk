# QFace RabbitMQ SDK

## Overview

QFace RabbitMQ SDK is a robust, actor-based integration with RabbitMQ for .NET applications. It provides a simple, declarative approach to working with RabbitMQ while leveraging the power of the Actor model for scalability and resilience.

The SDK offers both publishing and consuming capabilities, with a strong focus on configurability, error handling, and performance. It integrates seamlessly with ASP.NET Core and other .NET application types.

## Quick Start

### Installation

```bash
dotnet add package QFace.Sdk.RabbitMq
```

### Configuration

Add RabbitMQ configuration to your `appsettings.json`:

```json
{
  "RabbitMq": {
    "ConnectionString": "amqp://guest:guest@localhost:5672",
    "ExchangeType": "fanout",
    "PassiveExchange": true,
    "AutomaticRecoveryEnabled": true,
    "RetryCount": 5,
    "RetryIntervalMs": 1000
  }
}
```

### Register Services

In your `Program.cs` or `Startup.cs`:

```csharp
// Register both publishing and consuming capabilities
builder.Services.AddRabbitMq(
    builder.Configuration,
    new[] { typeof(Program).Assembly }  // Assemblies containing consumers
);

// Initialize publisher in a web application
app.UseRabbitMqInApi();

// Or in a console application
// app.Services.UseRabbitMqInConsumer();
```

### Publishing Messages

```csharp
public class OrderService
{
    private readonly IRabbitMqPublisher _publisher;
    
    public OrderService(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }
    
    public async Task CreateOrderAsync(Order order)
    {
        // Process the order...
        
        // Publish an event
        await _publisher.PublishAsync(
            new OrderCreatedEvent { OrderId = order.Id },
            exchangeName: "orders",
            routingKey: "order.created"
        );
    }
}
```

### Consuming Messages

```csharp
[Consumer("OrderConsumer")]
public class OrderConsumer
{
    private readonly ILogger<OrderConsumer> _logger;
    
    public OrderConsumer(ILogger<OrderConsumer> logger)
    {
        _logger = logger;
    }
    
    [Topic("orders", "order_created_queue", "order.created")]
    public async Task HandleOrderCreatedAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation($"Processing order: {@event.OrderId}");
        // Process the event...
        await Task.CompletedTask;
    }
}
```

## Core Components

### Publisher

The publisher components are responsible for sending messages to RabbitMQ:

- `IRabbitMqPublisher`: Interface for publishing messages
- `RabbitMqPublisher`: Implementation that forwards messages to the publisher actor
- `RabbitMqPublisherActor`: Actor that handles the actual publishing, with retry logic

### Consumer

The consumer components handle receiving and processing messages:

- `RabbitMqConsumerService`: Hosted service that manages consumer actors
- `RabbitMqConsumerSupervisorActor`: Supervisor actor that creates and manages consumer actors
- `RabbitMqConsumerActor`: Actor that handles message consumption for a specific topic

### Attributes

- `ConsumerAttribute`: Marks a class as a message consumer
- `TopicAttribute`: Specifies exchange, queue, and routing key for a handler method

### Configuration

- `RabbitMqOptions`: Configuration options for the RabbitMQ SDK
- `ConsumerMetadata`: Metadata about discovered consumer handlers

## Message Handling

### Publishing Messages

When you call `PublishAsync<T>` on the `IRabbitMqPublisher`, the following happens:

1. The publisher creates a `PublishMessage`
2. The message is sent to the `RabbitMqPublisherActor`
3. The actor serializes the message and publishes it to RabbitMQ
4. If publishing fails, the actor retries based on configuration

### Consuming Messages

Consumer registration and processing follows these steps:

1. During startup, the SDK scans assemblies for classes with `[Consumer]` attribute
2. Methods with `[Topic]` attribute are registered as message handlers
3. `RabbitMqConsumerSupervisorActor` creates a `RabbitMqConsumerActor` for each handler
4. Each consumer actor connects to its exchange and queue, and starts consuming
5. When a message is received, it's deserialized and passed to the appropriate handler method

## Advanced Features

### Actor-Based Architecture

The SDK uses the Actor model (via QFace.Sdk.ActorSystems) to provide:

- Concurrency control
- Isolation between handlers
- Automatic retry for failed operations
- Graceful shutdown
- Supervision hierarchies

### Retry Logic

Both publishers and consumers include built-in retry logic:

```csharp
// Configure retries in appsettings.json
{
  "RabbitMq": {
    "RetryCount": 5,
    "RetryIntervalMs": 1000
  }
}
```

### Exchange and Queue Options

Configure exchanges and queues with the `TopicAttribute`:

```csharp
[Topic(
    exchangeName: "orders",
    queueName: "order_processing_queue", 
    routingKey: "order.created")]
[Topic(
    exchangeName: "orders", 
    queueName: "order_logging_queue",
    routingKey: "order.#")]
```

### Configuration Options

```csharp
public class RabbitMqOptions
{
    // Connection options
    public string ConnectionString { get; set; }
    public bool AutomaticRecoveryEnabled { get; set; } = true;
    
    // Exchange options
    public string ExchangeType { get; set; } = ExchangeType.Fanout;
    public bool PassiveExchange { get; set; } = true;
    
    // Retry options
    public int RetryCount { get; set; } = 5;
    public int RetryIntervalMs { get; set; } = 1000;
    
    // Actor pool sizing
    public int PublisherInstances { get; set; } = 10;
    public int PublisherUpperBound { get; set; } = 100;
    public int ConsumerInstances { get; set; } = 10;
    public int ConsumerUpperBound { get; set; } = 100;
    
    // Application naming (for console apps)
    public string Title { get; set; }
}
```

## Integration Examples

### ASP.NET Core Web API

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Register services
builder.Services.AddRabbitMq(
    builder.Configuration, 
    new[] { typeof(Program).Assembly }
);

var app = builder.Build();

// Initialize RabbitMQ publisher
app.UseRabbitMqInApi();

app.Run();

// OrderController.cs
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IRabbitMqPublisher _publisher;
    
    public OrderController(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }
    
    [HttpPost]
    public async Task<IActionResult> CreateOrder(CreateOrderRequest request)
    {
        var order = new Order { /* ... */ };
        
        // Process order...
        
        // Publish event
        await _publisher.PublishAsync(
            new OrderCreatedEvent { OrderId = order.Id },
            exchangeName: "orders",
            routingKey: "order.created"
        );
        
        return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
    }
}
```

### Console Application

```csharp
// Program.cs
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((context, services) =>
    {
        // Register RabbitMQ
        services.AddRabbitMq(
            context.Configuration, 
            new[] { typeof(Program).Assembly }
        );
        
        // Add your application services
        services.AddHostedService<WorkerService>();
    })
    .Build();

// Initialize RabbitMQ for consumers
host.Services.UseRabbitMqInConsumer();

await host.RunAsync();

// OrderConsumer.cs
[Consumer("OrderConsumer")]
public class OrderConsumer
{
    private readonly ILogger<OrderConsumer> _logger;
    
    public OrderConsumer(ILogger<OrderConsumer> logger)
    {
        _logger = logger;
    }
    
    [Topic("orders", "order_processing_queue", "order.created")]
    public async Task ProcessOrderAsync(OrderCreatedEvent @event)
    {
        _logger.LogInformation($"Processing order: {@event.OrderId}");
        // Process the order...
        await Task.CompletedTask;
    }
}
```

## Best Practices

### Message Design

1. **Immutable Messages**: Design messages as immutable DTOs
2. **Versioning**: Consider adding version information to message contracts
3. **Serialization-Friendly**: Keep messages serialization-friendly (avoid circular references)
4. **Small Payloads**: Prefer small message payloads with identifiers over large objects

### Consumer Design

1. **Keep Handlers Focused**: Each handler method should have a single responsibility
2. **Quick Processing**: Keep handlers fast; offload long-running work to other systems
3. **Proper Error Handling**: Handle exceptions appropriately to avoid poison messages
4. **Idempotent Handlers**: Design handlers to be idempotent (can process same message multiple times)

### Exchange and Queue Strategy

1. **Naming Conventions**: Use consistent naming for exchanges, queues, and routing keys
2. **Durable Queues**: Use durable queues for important messages
3. **Topic Exchanges**: Prefer topic exchanges for flexibility
4. **Routing Key Structure**: Use hierarchical routing keys (e.g., `order.created`, `order.updated`)

## Troubleshooting

### Common Issues

1. **Connection Issues**:
    - Check RabbitMQ server is running
    - Verify connection string is correct
    - Ensure network allows connections

2. **Message Not Received**:
    - Verify exchange and routing key match
    - Check queue is bound correctly
    - Ensure consumer is running

3. **Serialization Errors**:
    - Check message structure matches expected type
    - Look for circular references
    - Verify property types match

### Enabling Detailed Logging

```csharp
builder.Services.AddLogging(logging => {
    logging.AddFilter("QFace.Sdk.RabbitMq", LogLevel.Debug);
});
```

## API Reference

### Extension Methods

- `AddRabbitMq(IConfiguration, Assembly[], Action<ActorConfig>)`: Adds RabbitMQ with both producer and consumer support
- `AddRabbitMqProducer(IServiceCollection)`: Adds only producer support
- `AddRabbitMqConsumers(IServiceCollection, Assembly[])`: Adds only consumer support
- `UseRabbitMqInApi(IApplicationBuilder)`: Initializes RabbitMQ in an API
- `UseRabbitMqInConsumer(IServiceProvider)`: Initializes RabbitMQ in a console application

### Core Interfaces

- `IRabbitMqPublisher`: Interface for publishing messages
    - `Task<bool> PublishAsync<T>(T message, string exchangeName, string routingKey = "")`

### Attributes

- `[Consumer(string name = null)]`: Marks a class as a message consumer
- `[Topic(string exchangeName, string queueName, string routingKey = "")]`: Configures a message handler
    - `ExchangeName`: Name of the exchange to use
    - `QueueName`: Name of the queue to bind
    - `RoutingKey`: Routing key to bind to the queue
    - `Durable`: Whether the queue should be durable (default: true)
    - `AutoDelete`: Whether the queue should auto-delete (default: false)
    - `PrefetchCount`: Number of messages to prefetch (default: 10)

### Configuration

- `RabbitMqOptions`: Configuration options for RabbitMQ

## Sample Use Cases

### Event-Driven Architecture

Use the SDK to implement an event-driven architecture:

1. **Services publish domain events** when state changes
2. **Consumers subscribe to events** they're interested in
3. **Multiple consumers** can process the same event for different purposes

### Microservice Communication

Enable communication between microservices:

1. **Commands**: Send instructions from one service to another
2. **Events**: Notify other services about state changes
3. **Integration Events**: Share information across service boundaries

### Background Processing

Offload work to background processes:

1. **Web API publishes tasks** to be processed
2. **Worker services consume tasks** and process them asynchronously
3. **Scaling** by adding more consumers as needed