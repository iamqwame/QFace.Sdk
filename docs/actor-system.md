# QFace Actor System SDK

## Overview

QFace Actor System SDK is a lightweight wrapper around Akka.NET that simplifies the creation and management of actor-based systems in .NET applications. It provides an easy-to-use API for configuring, registering, and communicating with actors, as well as integrating with dependency injection.

The SDK is designed to make actor-based programming more accessible to .NET developers by abstracting away some of the complexities of Akka.NET while still providing access to its powerful features.

## Quick Start

### Installation

```bash
dotnet add package QFace.Sdk.ActorSystems
```

### Basic Setup

Add the actor system to your service collection in `Program.cs` or `Startup.cs`:

```csharp
// Register actor system with the assemblies that contain your actor classes
builder.Services.AddActorSystem(
    new[] { typeof(Program).Assembly },
    config =>
    {
        config.SystemName = "MyActorSystem";
        config.AddActorType<MyActor>(numberOfInstances: 3, useRouter: true);
    },
    addLifecycle: true);
```

Initialize the actor system in your application:

```csharp
app.UseActorSystem();
```

### Creating an Actor

Actors inherit from the `BaseActor` class:

```csharp
public class GreetingActor : BaseActor
{
    public GreetingActor()
    {
        Receive<GreetMessage>(message => {
            Console.WriteLine($"Hello, {message.Name}!");
        });
    }
}

public class GreetMessage
{
    public string Name { get; set; }
}
```

### Sending Messages

Inject `IActorService` to send messages to actors:

```csharp
public class GreetingService
{
    private readonly IActorService _actorService;

    public GreetingService(IActorService actorService)
    {
        _actorService = actorService;
    }

    public void Greet(string name)
    {
        _actorService.Tell<GreetingActor>(new GreetMessage { Name = name });
    }
}
```

## Core Concepts

### Actor Model

The actor model is a conceptual model for concurrent computation where actors are the universal primitives of computation. Actors:

1. Can maintain private state that can only be modified by that actor
2. Can send messages to other actors
3. Can create new actors
4. Can determine how to handle the next received message

### BaseActor

All actors in your system should inherit from `BaseActor`. The `BaseActor` class provides:

- Message handling via the `Receive<T>()` method
- Event publishing via the `Publish()` method
- Child actor creation via the `ReceiveAsync<TMessage, TActor>()` method
- Integration with dependency injection

Example with multiple message types:

```csharp
public class UserActor : BaseActor
{
    private readonly IUserRepository _repository;

    public UserActor(IUserRepository repository)
    {
        _repository = repository;

        Receive<CreateUserCommand>(async cmd => {
            var user = await _repository.CreateUserAsync(cmd.UserData);
            Sender.Tell(new UserCreatedEvent { UserId = user.Id });
        });

        Receive<UpdateUserCommand>(async cmd => {
            await _repository.UpdateUserAsync(cmd.UserId, cmd.UserData);
            Sender.Tell(new UserUpdatedEvent { UserId = cmd.UserId });
        });
    }
}
```

### ActorConfig

The `ActorConfig` class is used to configure your actor system and its actors:

```csharp
var config = new ActorConfig()
    .AddActorType<UserActor>(numberOfInstances: 5, upperBound: 20, useRouter: true)
    .AddActorType<OrderActor>(numberOfInstances: 3);
```

Configuration options:

- `SystemName`: Name of the actor system (default: "DefaultActorSystem")
- `AddActorType<T>()`: Configure an actor type with options:
  - `numberOfInstances`: Initial number of instances for routed actors
  - `upperBound`: Maximum number of instances for elastic scaling
  - `useRouter`: Whether to use a router for load balancing

### IActorService

The `IActorService` is the primary way to interact with your actors:

```csharp
// Send a message to an actor
actorService.Tell<NotificationActor>(new SendEmailMessage { To = "user@example.com" });

// Send a message to a specific instance of an actor
actorService.Tell<UserActor>(new UpdateUserMessage { UserId = 123 }, "user-123");
```

## Advanced Features

### Routers

Routers distribute messages among multiple instances of an actor for load balancing:

```csharp
config.AddActorType<WorkerActor>(
    numberOfInstances: 10,  // Start with 10 instances
    upperBound: 50,         // Allow scaling up to 50 instances
    useRouter: true);       // Use a router
```

The SDK uses a round-robin router with a default resizer that can dynamically adjust the number of instances based on load.

### Event Publishing

Actors can publish events to the event stream:

```csharp
public class OrderActor : BaseActor
{
    public OrderActor()
    {
        Receive<ProcessOrderCommand>(cmd => {
            // Process order...

            // Publish event for other systems to react to
            Publish(new OrderProcessedEvent { OrderId = cmd.OrderId });
        });
    }
}
```

### Child Actors

Create child actors to handle specific tasks:

```csharp
public class ParentActor : BaseActor
{
    public ParentActor()
    {
        // Forward messages of type ProcessTask to new instances of TaskActor
        ReceiveAsync<ProcessTask, TaskActor>();

        // Forward and then terminate the child actor
        ReceiveAsync<OneTimeTask, OneTimeTaskActor>(poisonPill: true);
    }
}
```

### Actor Discovery

The SDK automatically discovers actor types in the specified assemblies:

```csharp
// Register all actors in these assemblies
services.AddActorSystem(
    new[] {
        typeof(Program).Assembly,
        typeof(UserActor).Assembly
    });
```

## Lifecycle Management

When you enable lifecycle management with `addLifecycle: true`, the SDK registers an `ActorSystemHostedService` that handles the graceful startup and shutdown of your actor system.

The hosted service:

1. Logs the actor system startup
2. Registers for application stopping notifications
3. Gracefully terminates the actor system on shutdown
4. Implements timeout handling for clean shutdown

## Best Practices

### Designing Actors

1. **Single Responsibility**: Each actor should have a clear, focused responsibility
2. **Immutable Messages**: Messages should be immutable to avoid concurrency issues
3. **Fine-Grained Actors**: Prefer many small actors over a few large ones
4. **Avoid Blocking**: Don't block inside actors; use `ReceiveAsync` for I/O operations

### Deployment Considerations

1. **Monitoring**: Set up monitoring for your actor system
2. **Scaling**: Start with fewer actors and scale up as needed
3. **Error Handling**: Implement supervision strategies for error recovery
4. **Testing**: Create unit tests for your actors using TestKit

## Integration Examples

### ASP.NET Core Web API

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add actor system
builder.Services.AddActorSystemWithLifecycle(
    new[] { typeof(Program).Assembly });

// Add controllers
builder.Services.AddControllers();

var app = builder.Build();

// Initialize actor system
app.UseActorSystem();

app.MapControllers();
app.Run();

// OrderController.cs
[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly IActorService _actorService;

    public OrderController(IActorService actorService)
    {
        _actorService = actorService;
    }

    [HttpPost]
    public IActionResult CreateOrder(CreateOrderRequest request)
    {
        _actorService.Tell<OrderActor>(new ProcessOrderCommand {
            OrderId = Guid.NewGuid(),
            Items = request.Items
        });

        return Accepted();
    }
}
```

### Worker Service

```csharp
// Program.cs
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddActorSystemWithLifecycle(
            new[] { typeof(Program).Assembly },
            config => {
                config.SystemName = "WorkerSystem";
                config.AddActorType<WorkerActor>(numberOfInstances: 5, useRouter: true);
            });

        services.AddHostedService<Worker>();
    })
    .Build();

await host.RunAsync();

// Worker.cs
public class Worker : BackgroundService
{
    private readonly IActorService _actorService;

    public Worker(IActorService actorService)
    {
        _actorService = actorService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _actorService.Tell<WorkerActor>(new ProcessWorkItemCommand());
            await Task.Delay(1000, stoppingToken);
        }
    }
}
```

## Troubleshooting

### Common Issues

1. **Actors not receiving messages**:

   - Check that the actor is properly registered
   - Ensure the actor type name matches

2. **Exception in actor initialization**:

   - Check constructor dependencies
   - Review error logs for specific exceptions

3. **Actor system not shutting down cleanly**:
   - Increase shutdown timeout
   - Check for blocked actors

### Debugging

Enable detailed logging for the actor system:

```csharp
builder.Services.AddLogging(logging => {
    logging.AddFilter("QFace.Sdk.ActorSystems", LogLevel.Debug);
});
```

## API Reference

### Extension Methods

- `AddActorSystem(Assembly[], Action<ActorConfig>, bool)`: Adds an actor system to the service collection
- `AddActorSystemWithLifecycle(Assembly[], Action<ActorConfig>)`: Adds an actor system with lifecycle management
- `UseActorSystem(IHost)`: Initializes the actor system in the application

### Core Classes

- `BaseActor`: Base class for all actors
- `ActorConfig`: Configuration for the actor system
- `ActorTypeConfig`: Configuration for a specific actor type
- `IActorService`: Service for sending messages to actors
- `TopLevelActors`: Registry for top-level actors
- `ActorSystemHostedService`: Manages actor system lifecycle

## Additional Resources

- [Akka.NET Documentation](https://getakka.net/articles/intro/what-is-akka.html)
- [Actor Model Explained](https://www.brianstorti.com/the-actor-model/)
- [Reactive Programming Principles](https://www.reactivemanifesto.org/)
