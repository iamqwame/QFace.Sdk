namespace QFace.Sdk.RabbitMq.Services;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly ActorSystem _actorSystem;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IActorRef _publisherActorRef;
    private readonly object _actorLock = new object();

    public RabbitMqPublisher(
        ActorSystem actorSystem,
        ILogger<RabbitMqPublisher> logger)
    {
        _actorSystem = actorSystem;
        _logger = logger;
    }

    public async Task<bool> PublishAsync<T>(T message, string exchangeName, string routingKey = "")
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
                            _publisherActorRef = _actorSystem.ActorSelection("/user/rabbitmq-publisher")
                                .ResolveOne(TimeSpan.FromSeconds(1))
                                .GetAwaiter()
                                .GetResult();
                            
                            _logger.LogDebug("[RabbitMQ] Found existing publisher actor");
                        }
                        catch
                        {
                            // Actor doesn't exist - this should not happen as it should be created during initialization
                            _logger.LogError("[RabbitMQ] Publisher actor not found. Make sure UseRabbitMqInApi/UseRabbitMqInConsumer has been called.");
                            return false;
                        }
                    }
                }
            }

            var publishMessage = new PublishMessage(message, routingKey, exchangeName);
            
            // Send message directly to the actor
            _publisherActorRef.Tell(publishMessage);
            
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"[RabbitMQ] Failed to publish message to exchange '{exchangeName}' with routing key '{routingKey}'");
            return false;
        }
    }
}