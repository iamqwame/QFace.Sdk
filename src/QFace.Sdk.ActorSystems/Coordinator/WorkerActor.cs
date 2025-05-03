using Microsoft.Extensions.Logging;

namespace QFace.Sdk.ActorSystems.Coordinator;

/// <summary>
/// Base worker actor that processes work distributed by coordinators
/// </summary>
public abstract class WorkerActor : BaseActor
{
    protected readonly ILogger _logger;

    /// <summary>
    /// Constructor with logger dependency
    /// </summary>
    protected WorkerActor(ILogger logger)
    {
        _logger = logger;
            
        // Add health check handling
        Receive<CoordinationMessages.Ping>(ping => 
        {
            Sender.Tell(new CoordinationMessages.Pong(), Self);
        });
            
        // Configure additional handlers
        ConfigureHandlers();
    }
        
    /// <summary>
    /// Configure message handlers for specific work types
    /// </summary>
    protected abstract void ConfigureHandlers();
        
    /// <summary>
    /// Helper method to report work completion
    /// </summary>
    protected void CompleteWork(object result, Guid workId)
    {
        Sender.Tell(new CoordinationMessages.WorkCompleted(result, workId), Self);
    }
}