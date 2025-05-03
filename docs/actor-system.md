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




## Coordinator Pattern

The Coordinator pattern provides a structured approach to coordinating work among multiple worker actors. This pattern is built into the SDK to make it easy to implement distributed processing systems.

### Overview

The Coordinator pattern consists of three main components:

1. **Coordinator Actor**: Central manager that distributes work and monitors workers
2. **Worker Actors**: Process work items and report results back to the coordinator
3. **Background Service**: Hosts the coordinator and provides integration with your application

### Key Benefits

- **Load Distribution**: Efficiently distribute work across multiple workers
- **Health Monitoring**: Automatic health checks to ensure workers are responsive
- **Fault Tolerance**: Handle failures gracefully and redistribute work as needed
- **Scalability**: Easily scale workers up or down based on demand

### Getting Started

#### Step 1: Define Work Items

Define your work items and result types:

```csharp
// Work item to be processed
public class ProcessDocumentWork
{
    public string DocumentId { get; set; }
    public string Content { get; set; }
}

// Result from processing
public class DocumentProcessedResult
{
    public string DocumentId { get; set; }
    public int WordCount { get; set; }
    public DateTime ProcessedTime { get; set; } = DateTime.UtcNow;
}
```

#### Step 2: Create Worker Actor

Implement a worker actor by inheriting from `WorkerActor`:

```csharp
public class DocumentProcessingWorker : WorkerActor
{
    private readonly IDocumentService _documentService;
    
    public DocumentProcessingWorker(
        ILogger<DocumentProcessingWorker> logger,
        IDocumentService documentService) : base(logger)
    {
        _documentService = documentService;
    }
    
    protected override void ConfigureHandlers()
    {
        // Define how to process work items
        Receive<ProcessDocumentWork>(async work => 
        {
            _logger.LogInformation("Processing document: {DocumentId}", work.DocumentId);
            
            // Do the actual processing work
            var wordCount = await _documentService.CountWordsAsync(work.Content);
            
            // Create result
            var result = new DocumentProcessedResult
            {
                DocumentId = work.DocumentId,
                WordCount = wordCount
            };
            
            // Report completion (Guid is for tracking in coordinator)
            CompleteWork(result, Guid.Parse(work.DocumentId));
        });
    }
}
```

#### Step 3: Implement Coordinator Actor

Create a coordinator by inheriting from `CoordinatorActor`:

```csharp
public class DocumentCoordinator : CoordinatorActor
{
    private readonly IDocumentRepository _documentRepository;
    
    public DocumentCoordinator(
        ILogger<DocumentCoordinator> logger,
        IDocumentRepository documentRepository) : base(logger)
    {
        _documentRepository = documentRepository;
    }
    
    protected override void CreateWorkerActors()
    {
        // Workers are automatically created via the actor system configuration
        // but you can create additional workers or specialized workers here if needed
    }
    
    // Override work completed to store results
    protected override void OnWorkCompleted(CoordinationMessages.WorkCompleted message)
    {
        base.OnWorkCompleted(message);
        
        if (message.Result is DocumentProcessedResult result)
        {
            // Store the result
            _documentRepository.SaveProcessingResult(result);
            
            // Forward to anyone waiting for the result
            Sender.Tell(result);
        }
    }
}
```

#### Step 4: Create Background Service

Create a background service by inheriting from `CoordinatorBackgroundService<T>`:

```csharp
public class DocumentProcessingService : CoordinatorBackgroundService<DocumentCoordinator>
{
    private readonly IDocumentRepository _documentRepository;
    
    public DocumentProcessingService(
        ILogger<DocumentProcessingService> logger,
        ActorSystem actorSystem,
        IDocumentRepository documentRepository) : base(logger, actorSystem)
    {
        _documentRepository = documentRepository;
    }
    
    protected override async Task StartProcessingAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Starting document processing service");
        
        // Create a timer to periodically check for new documents
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        
        while (await timer.WaitForNextTickAsync(stoppingToken))
        {
            try
            {
                // Get documents that need processing
                var documents = await _documentRepository.GetPendingDocumentsAsync(limit: 20);
                
                foreach (var document in documents)
                {
                    // Create work item
                    var workItem = new ProcessDocumentWork
                    {
                        DocumentId = document.Id,
                        Content = document.Content
                    };
                    
                    // Send to coordinator
                    SendWork(workItem);
                    
                    // Mark as processing
                    await _documentRepository.MarkAsProcessingAsync(document.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing documents");
            }
        }
    }
}
```

#### Step 5: Register the Service

Register your coordinator service with a single line:

```csharp
// Register the coordinator service
services.AddCoordinatorService<DocumentProcessingService, DocumentCoordinator, DocumentProcessingWorker>();
```

This registers all the necessary components:

- Adds the actor system with lifecycle management
- Configures the coordinator actor
- Configures the worker actors with appropriate scaling
- Registers the background service

### Customization Options

The coordinator framework is designed to be customizable:

#### Custom Work Distribution

Override the `OnDistributeWork` method to implement custom distribution strategies:

```csharp
protected override void OnDistributeWork(CoordinationMessages.DistributeWork message)
{
    // Custom work distribution strategy
    // Example: Priority-based assignment instead of round-robin
    
    var workItem = message.WorkItem as PriorityWork;
    var workerKey = SelectWorkerByPriority(workItem.Priority);
    
    // Send to selected worker
    _workerActors[workerKey].Tell(workItem, Self);
}
```

#### Custom Worker Creation

Override the `CreateWorkerActors` method for specialized worker configurations:

```csharp
protected override void CreateWorkerActors()
{
    // Create different types of workers for different work items
    
    for (int i = 0; i < 3; i++)
    {
        var imageWorker = Context.ActorOf(
            DependencyResolver.For(Context.System).Props<ImageProcessingWorker>(),
            $"image-worker-{i}");
            
        _workerActors.Add($"image-worker-{i}", imageWorker);
    }
    
    for (int i = 0; i < 5; i++)
    {
        var textWorker = Context.ActorOf(
            DependencyResolver.For(Context.System).Props<TextProcessingWorker>(),
            $"text-worker-{i}");
            
        _workerActors.Add($"text-worker-{i}", textWorker);
    }
}
```

#### Advanced Work Tracking

Add work tracking by overriding the `StoreWorkContext` method:

```csharp
// Dictionary to store work contexts
private readonly Dictionary<Guid, WorkContext> _workContexts = new();

protected override void StoreWorkContext(Guid workId, WorkContext context)
{
    _workContexts[workId] = context;
    
    // Set up timeout for work items
    Context.System.Scheduler.ScheduleTellOnce(
        TimeSpan.FromMinutes(5),
        Self,
        new CheckWorkTimeout(workId),
        Self);
}

// Custom timeout message
public class CheckWorkTimeout
{
    public Guid WorkId { get; }
    
    public CheckWorkTimeout(Guid workId)
    {
        WorkId = workId;
    }
}

// Add timeout handler in ConfigureHandlers
protected override void ConfigureHandlers()
{
    Receive<CheckWorkTimeout>(message => {
        if (_workContexts.TryGetValue(message.WorkId, out var context))
        {
            if (DateTime.UtcNow - context.Created > TimeSpan.FromMinutes(5))
            {
                _logger.LogWarning("Work item {WorkId} timed out", message.WorkId);
                
                // Redistribute or notify of timeout
                RedistributeWork(message.WorkId);
            }
        }
    });
}
```

### Common Use Cases

The Coordinator pattern is useful for:

1. **Background Processing**: Process items from a queue (like documents, images, etc.)
2. **Scheduled Tasks**: Run periodic tasks that need to be distributed
3. **Resource-Intensive Operations**: Distribute CPU or memory-intensive tasks
4. **Parallel Processing**: Break down large tasks into smaller parallel units
5. **Event Processing**: Process events from external systems in an ordered manner

### Health Monitoring

The coordinator automatically performs health checks on workers. You can customize this by overriding the `OnCheckHealth` method:

```csharp
protected override void OnCheckHealth(CoordinationMessages.CheckHealth message)
{
    base.OnCheckHealth(message);
    
    // Add additional health metrics
    foreach (var worker in _workerActors)
    {
        worker.Value.Tell(new CheckResourceUsage(), Self);
    }
}

// Add a handler for resource usage responses
public class ResourceUsage
{
    public string WorkerId { get; set; }
    public double CpuUsage { get; set; }
    public double MemoryUsage { get; set; }
}

protected override void ConfigureHandlers()
{
    Receive<ResourceUsage>(usage => {
        _logger.LogInformation(
            "Worker {WorkerId} resource usage - CPU: {CpuUsage}%, Memory: {MemoryUsage}MB",
            usage.WorkerId, usage.CpuUsage, usage.MemoryUsage);
            
        // Restart workers with high resource usage
        if (usage.CpuUsage > 90 || usage.MemoryUsage > 500)
        {
            _logger.LogWarning("Restarting worker {WorkerId} due to high resource usage", usage.WorkerId);
            
            var worker = _workerActors[usage.WorkerId];
            worker.Tell(PoisonPill.Instance);
            
            // Create a new worker to replace it
            var newWorker = Context.ActorOf(
                DependencyResolver.For(Context.System).Props<WorkerActor>(),
                $"{usage.WorkerId}-new");
                
            _workerActors[usage.WorkerId] = newWorker;
        }
    });
}
```

### Best Practices

1. **Right-Size Workers**: Find the optimal number of workers for your workload through testing
2. **Stateless Workers**: Keep workers stateless to allow easy scaling and replacement
3. **Worker Specialization**: Consider specialized workers for different types of work
4. **Work Idempotency**: Ensure work can be safely retried in case of failures
5. **Graceful Shutdown**: Allow in-progress work to complete during shutdown
6. **Work Timeouts**: Implement timeouts for long-running work items
7. **Monitoring**: Log key metrics to track system health and performance

### Example: API Integration

Here's how to expose the coordinator service via an API:

```csharp
[ApiController]
[Route("api/[controller]")]
public class DocumentController : ControllerBase
{
    private readonly IActorService _actorService;
    
    public DocumentController(IActorService actorService)
    {
        _actorService = actorService;
    }
    
    [HttpPost]
    public IActionResult ProcessDocument([FromBody] DocumentRequest request)
    {
        // Create work item
        var workItem = new ProcessDocumentWork
        {
            DocumentId = Guid.NewGuid().ToString(),
            Content = request.Content
        };
        
        // Send to coordinator
        _actorService.Tell<DocumentCoordinator>(
            new CoordinationMessages.DistributeWork(workItem));
            
        return Accepted(new { DocumentId = workItem.DocumentId });
    }
    
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStatus(string id)
    {
        // Could check a repository or ask the coordinator for status
        var status = await _documentRepository.GetProcessingStatusAsync(id);
        return Ok(status);
    }
}
```






