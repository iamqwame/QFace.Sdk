namespace QFace.Sdk.RedisMq.Consumer
{
    internal class RedisMqConsumerService : IHostedService
    {
        private readonly ILogger<RedisMqConsumerService> _logger;
        private readonly ActorSystem _actorSystem;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ConsumerMetadata> _consumers;
        private IActorRef _supervisorActor;

        public RedisMqConsumerService(
            ILogger<RedisMqConsumerService> logger,
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
                _logger.LogInformation(
                    "Starting consumer service with {ConsumerCount} consumers",
                    _consumers.Count);

                // Log details about each discovered consumer (aggregated to avoid log-in-loop)
                var consumerDetails = _consumers
                    .Select(c => $"{c.ConsumerType.Name}/{c.HandlerMethod.Name}@{c.ChannelAttribute.ChannelName}")
                    .ToArray();
                _logger.LogInformation(
                    "Found {ConsumerCount} consumers: {ConsumerDetails}",
                    consumerDetails.Length, consumerDetails);

                // Create props for the supervisor actor
                var props = Props.Create(
                    () => new RedisMqConsumerSupervisorActor(
                        _serviceProvider.GetRequiredService<ILogger<RedisMqConsumerSupervisorActor>>(),
                        _serviceProvider,
                        new List<ConsumerMetadata>(_consumers)
                    )
                );

                // Actually create the actor with more logging
                try
                {
                    _supervisorActor = _actorSystem.ActorOf(props, "redis-consumer-supervisor");
                    _logger.LogInformation("Successfully created consumer supervisor actor");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to create consumer supervisor actor");
                    throw;
                }
            }
            else
            {
                _logger.LogWarning("No consumers registered, consumer service will not start");
            }
            
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Stopping consumer service");
            
            // Gracefully stop the supervisor actor if it exists
            _supervisorActor?.GracefulStop(TimeSpan.FromSeconds(5));
            
            return Task.CompletedTask;
        }
    }
}
