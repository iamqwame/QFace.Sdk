namespace QFace.Sdk.RedisMq.Actors
{
    internal class RedisMqConsumerSupervisorActor : BaseActor
    {
        private readonly ILogger<RedisMqConsumerSupervisorActor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly List<ConsumerMetadata> _consumers;
        private readonly ISubscriber _subscriber;
        private readonly Dictionary<string, IActorRef> _consumerActors = new();
        private readonly Dictionary<string, TaskCompletionSource<bool>> _subscriptionTasks = new();

        public RedisMqConsumerSupervisorActor(
            ILogger<RedisMqConsumerSupervisorActor> logger,
            IServiceProvider serviceProvider,
            List<ConsumerMetadata> consumers)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _consumers = consumers;
            _subscriber = _serviceProvider.GetRequiredService<RedisMqConnectionProvider>().Subscriber;

            ReceiveAsync<StartConsumers>(HandleStartConsumers);
            ReceiveAsync<StopConsumers>(HandleStopConsumers);
        }

        protected override void PreStart()
        {
            _logger.LogInformation($"[Redis] Consumer supervisor actor started with {_consumers.Count} consumers");
            
            // Start all consumers
            Self.Tell(new StartConsumers());
            
            base.PreStart();
        }

        private async Task HandleStartConsumers(StartConsumers message)
        {
            try
            {
                foreach (var consumer in _consumers)
                {
                    await StartConsumer(consumer);
                }
                
                _logger.LogInformation($"[Redis] ✅ All {_consumers.Count} consumers started successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Redis] ❌ Failed to start consumers");
            }
        }

        private async Task HandleStopConsumers(StopConsumers message)
        {
            try
            {
                foreach (var consumerActor in _consumerActors.Values)
                {
                    await consumerActor.GracefulStop(TimeSpan.FromSeconds(5));
                }
                
                _consumerActors.Clear();
                _logger.LogInformation("[Redis] ✅ All consumers stopped successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[Redis] ❌ Failed to stop consumers");
            }
        }

        private async Task StartConsumer(ConsumerMetadata consumerMetadata)
        {
            try
            {
                var channelName = consumerMetadata.ChannelAttribute.ChannelName;
                
                _logger.LogInformation($"[Redis] Starting consumer for channel '{channelName}'");

                // Create consumer actor
                var consumerActor = Context.ActorOf(
                    Props.Create(() => new RedisMqConsumerActor(
                        _serviceProvider.GetRequiredService<ILogger<RedisMqConsumerActor>>(),
                        _serviceProvider,
                        consumerMetadata,
                        _subscriber
                    )),
                    $"redis-consumer-{channelName.Replace(":", "-").Replace("*", "wildcard")}"
                );

                _consumerActors[channelName] = consumerActor;

                // Subscribe to the Redis channel
                var subscriptionTask = new TaskCompletionSource<bool>();
                _subscriptionTasks[channelName] = subscriptionTask;

                await _subscriber.SubscribeAsync(channelName, async (channel, message) =>
                {
                    try
                    {
                        var consumeMessage = new ConsumeMessage(channel, message);
                        consumerActor.Tell(consumeMessage);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"[Redis] Error processing message from channel '{channel}': {ex.Message}");
                    }
                });

                subscriptionTask.SetResult(true);
                _logger.LogInformation($"[Redis] ✅ Successfully subscribed to channel '{channelName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Redis] ❌ Failed to start consumer for channel '{consumerMetadata.ChannelAttribute.ChannelName}': {ex.Message}");
            }
        }

        protected override void PostStop()
        {
            _logger.LogInformation("[Redis] Consumer supervisor actor stopped");
            base.PostStop();
        }
    }

    internal class StartConsumers { }
    internal class StopConsumers { }
}
