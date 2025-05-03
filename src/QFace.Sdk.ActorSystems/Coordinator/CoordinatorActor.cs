using Microsoft.Extensions.Logging;

namespace QFace.Sdk.ActorSystems.Coordinator;

/// <summary>
/// Base coordinator actor that provides a framework for coordination patterns
/// </summary>
public abstract class CoordinatorActor : BaseActor
{
    protected readonly ILogger _logger;
    protected readonly Dictionary<string, IActorRef> _workerActors = new();
    protected int _nextWorkerIndex = 0;

    /// <summary>
    /// Constructor with logger dependency
    /// </summary>
    protected CoordinatorActor(ILogger logger)
    {
        _logger = logger;

        // Define base message handlers
        Receive<CoordinationMessages.Initialize>(OnInitialize);
        Receive<CoordinationMessages.DistributeWork>(OnDistributeWork);
        Receive<CoordinationMessages.WorkCompleted>(OnWorkCompleted);
        Receive<CoordinationMessages.CheckHealth>(OnCheckHealth);
            
        // Setup additional handlers
        ConfigureHandlers();
    }

    /// <summary>
    /// Configure additional message handlers
    /// </summary>
    protected virtual void ConfigureHandlers() { }

    /// <summary>
    /// Initialization handler that sets up worker actors
    /// </summary>
    protected virtual void OnInitialize(CoordinationMessages.Initialize message)
    {
        _logger.LogInformation("[{CoordinatorName}] Initializing coordinator", GetType().Name);
            
        // Create the initial worker actors
        CreateWorkerActors();
            
        // Schedule periodic health check
        ScheduleHealthChecks();
    }

    /// <summary>
    /// Creates worker actors according to implementation
    /// </summary>
    protected abstract void CreateWorkerActors();

    /// <summary>
    /// Schedules periodic health checks
    /// </summary>
    protected virtual void ScheduleHealthChecks()
    {
        Context.System.Scheduler.ScheduleTellRepeatedly(
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(2),
            Self,
            new CoordinationMessages.CheckHealth(),
            Self);
    }

    /// <summary>
    /// Distributes work to workers using the chosen strategy
    /// </summary>
    protected virtual void OnDistributeWork(CoordinationMessages.DistributeWork message)
    {
        if (_workerActors.Count == 0)
        {
            _logger.LogWarning("[{CoordinatorName}] No worker actors available to process work", GetType().Name);
            return;
        }

        // Default to round-robin distribution
        _nextWorkerIndex = _nextWorkerIndex % _workerActors.Count;
        var workerKey = _workerActors.Keys.ElementAt(_nextWorkerIndex);
        var worker = _workerActors[workerKey];
            
        // Keep track of sender for response
        var sender = Sender;
        var respondTo = message.RespondTo ?? sender;
            
        var workId = Guid.NewGuid();
        var context = new WorkContext { 
            WorkId = workId, 
            RespondTo = respondTo 
        };
            
        // Store the work context for later retrieval
        StoreWorkContext(workId, context);
            
        // Forward the work to the worker
        worker.Tell(message.WorkItem, Self);
            
        _logger.LogDebug("[{CoordinatorName}] Distributed work {WorkId} to {WorkerKey}", 
            GetType().Name, workId, workerKey);
            
        _nextWorkerIndex++;
    }

    /// <summary>
    /// Stores work context for tracking
    /// </summary>
    protected virtual void StoreWorkContext(Guid workId, WorkContext context)
    {
        // In a real implementation, this would store the context
        // Override this in implementations that need to track work
    }

    /// <summary>
    /// Work completion handler
    /// </summary>
    protected virtual void OnWorkCompleted(CoordinationMessages.WorkCompleted message)
    {
        _logger.LogInformation("[{CoordinatorName}] Work {WorkId} completed", 
            GetType().Name, message.WorkId);
            
        // In a base implementation, we might just forward the result
        // Implementations can override this for more complex behavior
    }

    /// <summary>
    /// Health check handler
    /// </summary>
    protected virtual void OnCheckHealth(CoordinationMessages.CheckHealth message)
    {
        _logger.LogDebug("[{CoordinatorName}] Performing health check on worker actors", GetType().Name);
            
        foreach (var worker in _workerActors.Values)
        {
            worker.Tell(new CoordinationMessages.Ping(), Self);
        }
    }
        
    /// <summary>
    /// Work context for tracking work items
    /// </summary>
    protected class WorkContext
    {
        public Guid WorkId { get; set; }
        public IActorRef RespondTo { get; set; }
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}