using Microsoft.Extensions.Logging;

namespace QFace.Sdk.ActorSystems.Coordinator;

/// <summary>
/// Base background service that uses a coordinator actor
/// </summary>
public abstract class CoordinatorBackgroundService<TCoordinator> : BackgroundService 
    where TCoordinator : CoordinatorActor
{
    protected readonly ILogger _logger;
    protected readonly ActorSystem _actorSystem;
    protected IActorRef _coordinatorActor;

    /// <summary>
    /// Constructor with dependencies
    /// </summary>
    protected CoordinatorBackgroundService(
        ILogger logger,
        ActorSystem actorSystem)
    {
        _logger = logger;
        _actorSystem = actorSystem;
    }

    /// <summary>
    /// Executes the background service
    /// </summary>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("[{ServiceName}] Starting coordinator background service", GetType().Name);

        try
        {
            // Create the coordinator actor
            _coordinatorActor = CreateCoordinatorActor();

            // Initialize the coordinator
            _coordinatorActor.Tell(new CoordinationMessages.Initialize());

            // Start the service processing
            await StartProcessingAsync(stoppingToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{ServiceName}] Error in background service", GetType().Name);
            throw;
        }
    }

    /// <summary>
    /// Creates the coordinator actor
    /// </summary>
    protected virtual IActorRef CreateCoordinatorActor()
    {
        // Fix the Props creation
        return _actorSystem.ActorOf(
            DependencyResolver
                .For(_actorSystem)
                .Props<TCoordinator>(),
            $"coordinator-{typeof(TCoordinator).Name.ToLower()}-{Guid.NewGuid():N}");
    }

    /// <summary>
    /// Starts the processing logic for the service
    /// </summary>
    protected abstract Task StartProcessingAsync(CancellationToken stoppingToken);

    /// <summary>
    /// Called when the service is stopping
    /// </summary>
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[{ServiceName}] Stopping coordinator background service", GetType().Name);
            
        try
        {
            // Allow clean shutdown
            await OnStoppingAsync(cancellationToken);
                
            // Allow some time for pending work to complete
            await Task.Delay(TimeSpan.FromSeconds(5), cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // Expected during cancellation
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[{ServiceName}] Error during service shutdown", GetType().Name);
        }
            
        await base.StopAsync(cancellationToken);
    }

    /// <summary>
    /// Called when the service is stopping to allow cleanup
    /// </summary>
    protected virtual Task OnStoppingAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
        
    /// <summary>
    /// Helper to send work to the coordinator
    /// </summary>
    protected void SendWork(object workItem, IActorRef respondTo = null)
    {
        if (_coordinatorActor == null)
        {
            throw new InvalidOperationException("Coordinator actor not initialized");
        }
            
        _coordinatorActor.Tell(new CoordinationMessages.DistributeWork(workItem)
        {
            RespondTo = respondTo
        });
    }
}