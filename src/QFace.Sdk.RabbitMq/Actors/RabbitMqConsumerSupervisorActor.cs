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

            // Get shared connection for all consumers (registered as singleton)
            // This reduces connection count from N (one per consumer) to 1 (shared)
            var connectionProvider = _serviceProvider.GetRequiredService<RabbitMqConnectionProvider>();
            var sharedConnection = connectionProvider.Connection;
            
            _logger.LogInformation($"[RabbitMQ] Using shared connection for all {_consumers.Count} consumers");

            foreach (var consumer in _consumers)
            {
                try
                {
                    _logger.LogInformation(
                        $"[RabbitMQ] Starting consumer for {consumer.ConsumerType.Name}." +
                        $"{consumer.HandlerMethod.Name} with routing key '{consumer.TopicAttribute.RoutingKey}'"
                    );
        
                    // Use manual Props.Create to avoid DI resolution issues
                    // We manually resolve services from the root service provider (not scoped)
                    // This ensures IServiceProvider (not IServiceScope) is injected
                    // Pass shared connection to avoid creating one connection per consumer
                    var props = Props.Create(
                        () => new RabbitMqConsumerActor(
                            _serviceProvider.GetRequiredService<ILogger<RabbitMqConsumerActor>>(),
                            _serviceProvider.GetRequiredService<IOptions<RabbitMqOptions>>(),
                            _serviceProvider, // Root service provider - actors are long-lived
                            consumer,
                            sharedConnection // Shared connection - all consumers use the same connection
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