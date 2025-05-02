namespace QFace.Sdk.RabbitMq.Services;

public class RabbitMqPublisher : IRabbitMqPublisher
{
    private readonly ActorSystem _actorSystem;
    private readonly ILogger<RabbitMqPublisher> _logger;
    private IActorRef _publisherActorRef;

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
            if (_publisherActorRef == null)
            {
                try
                {
                    // Try to find existing actor
                    _publisherActorRef = await _actorSystem.ActorSelection("/user/rabbitmq-publisher")
                        .ResolveOne(TimeSpan.FromSeconds(1));
                }
                catch
                {
                    _logger.LogWarning("[RabbitMQ] Publisher actor not found. Make sure UseRabbitMqInApi/UseRabbitMqInConsumer has been called.");
                    return false;
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
