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

    protected CoordinatorActor(ILogger logger)
    {
        _logger = logger;

        Receive<CoordinationMessages.Initialize>(OnInitialize);
        Receive<CoordinationMessages.DistributeWork>(OnDistributeWork);
        Receive<CoordinationMessages.WorkCompleted>(OnWorkCompleted);
        Receive<CoordinationMessages.CheckHealth>(OnCheckHealth);
            
        ConfigureHandlers();
    }

    protected virtual void ConfigureHandlers() { }

    protected virtual void OnInitialize(CoordinationMessages.Initialize message)
    {
        _logger.LogInformation("[{CoordinatorName}] Initializing coordinator", GetType().Name);
            
        CreateWorkerActors();
            
        ScheduleHealthChecks();
    }

    protected abstract void CreateWorkerActors();

    protected virtual void ScheduleHealthChecks()
    {
        Context.System.Scheduler.ScheduleTellRepeatedly(
            TimeSpan.FromSeconds(30),
            TimeSpan.FromMinutes(2),
            Self,
            new CoordinationMessages.CheckHealth(),
            Self);
    }

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
            
        var sender = Sender;
        var respondTo = message.RespondTo ?? sender;
            
        var workId = Guid.NewGuid();
        var context = new WorkContext { 
            WorkId = workId, 
            RespondTo = respondTo 
        };
            
        StoreWorkContext(workId, context);
            
        worker.Tell(message.WorkItem, Self);
            
        _logger.LogDebug("[{CoordinatorName}] Distributed work {WorkId} to {WorkerKey}", 
            GetType().Name, workId, workerKey);
            
        _nextWorkerIndex++;
    }

    protected virtual void StoreWorkContext(Guid workId, WorkContext context)
    {
        // In a real implementation, this would store the context
        // Override this in implementations that need to track work
    }

    protected virtual void OnWorkCompleted(CoordinationMessages.WorkCompleted message)
    {
        _logger.LogInformation("[{CoordinatorName}] Work {WorkId} completed", 
            GetType().Name, message.WorkId);
            
        // In a base implementation, we might just forward the result
        // Implementations can override this for more complex behavior
    }

    protected virtual void OnCheckHealth(CoordinationMessages.CheckHealth message)
    {
        _logger.LogDebug("[{CoordinatorName}] Performing health check on worker actors", GetType().Name);
            
        foreach (var worker in _workerActors.Values)
        {
            worker.Tell(new CoordinationMessages.Ping(), Self);
        }
    }
        
    protected class WorkContext
    {
        public Guid WorkId { get; set; }
        public IActorRef RespondTo { get; set; } = null!;
        public DateTime Created { get; set; } = DateTime.UtcNow;
    }
}