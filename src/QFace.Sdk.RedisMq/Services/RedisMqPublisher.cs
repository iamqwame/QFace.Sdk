namespace QFace.Sdk.RedisMq.Services;

public class RedisMqPublisher : IRedisMqPublisher
{
    private readonly ActorSystem _actorSystem;
    private readonly ILogger<RedisMqPublisher> _logger;
    private IActorRef _publisherActorRef;
    private readonly object _actorLock = new object();

    public RedisMqPublisher(
        ActorSystem actorSystem,
        ILogger<RedisMqPublisher> logger)
    {
        _actorSystem = actorSystem;
        _logger = logger;
    }

    public async Task<bool> PublishAsync<T>(T message, string channelName)
    {
        try
        {
            // Ensure we have a reference to the publisher actor
            if (_publisherActorRef == null || _publisherActorRef.IsNobody())
            {
                lock (_actorLock)
                {
                    if (_publisherActorRef == null || _publisherActorRef.IsNobody())
                    {
                        try
                        {
                            // Try to find existing actor
                            _publisherActorRef = _actorSystem.ActorSelection("/user/redis-publisher")
                                .ResolveOne(TimeSpan.FromSeconds(1))
                                .GetAwaiter()
                                .GetResult();
                            
                            _logger.LogDebug("[Redis] Found existing publisher actor");
                        }
                        catch
                        {
                            // Actor doesn't exist - this should not happen as it should be created during initialization
                            _logger.LogError("[Redis] Publisher actor not found. Make sure UseRedisMqInApi/UseRedisMqInConsumer has been called.");
                            return false;
                        }
                    }
                }
            }

            var publishMessage = new PublishMessage(message, channelName);
            
            // Send message directly to the actor
            _publisherActorRef.Tell(publishMessage);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[Redis] Failed to publish message to channel '{channelName}'");
            return false;
        }
    }
}
