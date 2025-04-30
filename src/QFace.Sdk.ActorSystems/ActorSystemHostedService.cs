using Microsoft.Extensions.Logging;

namespace QFace.Sdk.ActorSystems;

/// <summary>
/// Hosted service for managing the actor system lifecycle
/// </summary>
public class ActorSystemHostedService : IHostedService
{
    private readonly ILogger<ActorSystemHostedService> _logger;
    private readonly Akka.Actor.ActorSystem _actorSystem;
    private readonly IHostApplicationLifetime _appLifetime;

    /// <summary>
    /// Creates a new instance of the actor system hosted service
    /// </summary>
    public ActorSystemHostedService(
        ILogger<ActorSystemHostedService> logger,
        Akka.Actor.ActorSystem actorSystem,
        IHostApplicationLifetime appLifetime)
    {
        _logger = logger;
        _actorSystem = actorSystem;
        _appLifetime = appLifetime;
    }

    /// <summary>
    /// Starts the actor system
    /// </summary>
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ActorSystem] Starting actor system '{ActorSystemName}'", _actorSystem.Name);
        
        _appLifetime.ApplicationStopping.Register(() =>
        {
            _logger.LogInformation("[ActorSystem] Application stopping, initiating actor system shutdown");
        });
        
        return Task.CompletedTask;
    }

    /// <summary>
    /// Stops the actor system
    /// </summary>
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("[ActorSystem] Stopping actor system '{ActorSystemName}'", _actorSystem.Name);
        
        try
        {
            // Allow some time for the actor system to gracefully shutdown
            return Task.WhenAny(
                _actorSystem.Terminate(),
                Task.Delay(TimeSpan.FromSeconds(5), cancellationToken)
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[ActorSystem] Error stopping actor system");
            return Task.CompletedTask;
        }
    }
}