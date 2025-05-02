namespace QFace.Sdk.RabbitMq.Consumer
{
    internal class RabbitMqConsumerService : IHostedService
    {
        private readonly ILogger<RabbitMqConsumerService> _logger;
        private readonly ActorSystem _actorSystem;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ConsumerMetadata> _consumers;
        private IActorRef _supervisorActor;

        public RabbitMqConsumerService(
            ILogger<RabbitMqConsumerService> logger,
            ActorSystem actorSystem,
            IServiceProvider serviceProvider,
            List<ConsumerMetadata> consumers)
        {
            _logger = logger;
            _actorSystem = actorSystem;
            _serviceProvider = serviceProvider;
            _consumers = consumers;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            if (_consumers != null && _consumers.Any())
            {
                _logger.LogInformation($"[RabbitMQ] Starting consumer service with {_consumers.Count} consumers");
        
                // Log details about each discovered consumer
                foreach (var consumer in _consumers)
                {
                    _logger.LogInformation(
                        $"[RabbitMQ] Found consumer: {consumer.ConsumerType.Name}, " +
                        $"Method: {consumer.HandlerMethod.Name}, " +
                        $"Routing Key: {consumer.TopicAttribute.RoutingKey}, " +
                        $"Queue: {consumer.TopicAttribute.QueueName}"
                    );
                }
                
                // Create props for the supervisor actor
                var props = Props.Create(
                    () => new RabbitMqConsumerSupervisorActor(
                        _serviceProvider.GetRequiredService<ILogger<RabbitMqConsumerSupervisorActor>>(),
                        _serviceProvider,
                        new List<ConsumerMetadata>(_consumers)
                    )
                );
        
                // Actually create the actor with more logging
                try 
                {
                    _supervisorActor = _actorSystem.ActorOf(props, "rabbitmq-consumer-supervisor");
                    _logger.LogInformation("[RabbitMQ] Successfully created consumer supervisor actor");
                }
                catch (Exception ex) 
                {
                    _logger.LogError(ex, "[RabbitMQ] Failed to create consumer supervisor actor");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("[RabbitMQ] No consumers registered, consumer service will not start");
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("[RabbitMQ] Stopping consumer service");
            
            // Gracefully stop the supervisor actor if it exists
            _supervisorActor?.GracefulStop(TimeSpan.FromSeconds(5));
            
            return Task.CompletedTask;
        }
    }
}