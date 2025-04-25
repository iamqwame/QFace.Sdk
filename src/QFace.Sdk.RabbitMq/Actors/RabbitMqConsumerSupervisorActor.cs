using QFace.Sdk.RabbitMq.Consumer;
using QFace.Sdk.RabbitMq.Models;

namespace QFace.Sdk.RabbitMq.Actors
{
    public class RabbitMqConsumerSupervisorActor : ReceiveActor
    {
        private readonly ILogger<RabbitMqConsumerSupervisorActor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ConsumerMetadata> _consumers;

        public RabbitMqConsumerSupervisorActor(
            ILogger<RabbitMqConsumerSupervisorActor> logger,
            IServiceProvider serviceProvider,
            List<ConsumerMetadata> consumers)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _consumers = consumers;

            Receive<string>(message =>
            {
                if (message == "start")
                {
                    StartConsumers();
                }
            });

            Self.Tell("start");
        }

        private void StartConsumers()
        {
            _logger.LogInformation($"[RabbitMQ] Starting {_consumers.Count} consumers");

            foreach (var consumer in _consumers)
            {
                try
                {
                    _logger.LogInformation(
                        $"[RabbitMQ] Starting consumer for {consumer.ConsumerType.Name}." +
                        $"{consumer.HandlerMethod.Name} with routing key '{consumer.TopicAttribute.RoutingKey}'"
                    );
        
                    var props = Props.Create(
                        () => new RabbitMqConsumerActor(
                            _serviceProvider.GetRequiredService<ILogger<RabbitMqConsumerActor>>(),
                            _serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>(),
                            _serviceProvider,
                            consumer
                        )
                    );

                    var actorName = $"rabbitmq-consumer-{consumer.ConsumerType.Name}-" +
                                    $"{consumer.TopicAttribute.RoutingKey}-" +
                                    $"{Guid.NewGuid().ToString("N")[..8]}";
                    
                    var actorRef = Context.ActorOf(props, actorName);
                    
                    _logger.LogInformation(
                        $"[RabbitMQ] Started consumer actor '{actorName}' " +
                        $"for routing key '{consumer.TopicAttribute.RoutingKey}' " +
                        $"with reference {actorRef.Path}"
                    );
                }
                catch (Exception ex)
                {
                    _logger.LogError(
                        ex, 
                        $"[RabbitMQ] Failed to start consumer actor for " +
                        $"'{consumer.ConsumerType.Name}' with routing key '{consumer.TopicAttribute.RoutingKey}'"
                    );
                }
            }
        }
    }
}