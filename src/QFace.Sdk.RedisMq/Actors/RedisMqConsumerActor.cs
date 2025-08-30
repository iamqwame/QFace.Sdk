namespace QFace.Sdk.RedisMq.Actors
{
    internal class RedisMqConsumerActor : BaseActor
    {
        private readonly ILogger<RedisMqConsumerActor> _logger;
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsumerMetadata _consumerMetadata;
        private readonly ISubscriber _subscriber;
        private readonly RedisChannel _channel;

        public RedisMqConsumerActor(
            ILogger<RedisMqConsumerActor> logger,
            IServiceProvider serviceProvider,
            ConsumerMetadata consumerMetadata,
            ISubscriber subscriber)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
            _consumerMetadata = consumerMetadata;
            _subscriber = subscriber;
            _channel = new RedisChannel(consumerMetadata.ChannelAttribute.ChannelName, RedisChannel.PatternMode.Literal);

            ReceiveAsync<ConsumeMessage>(HandleConsumeMessage);
        }

        private async Task HandleConsumeMessage(ConsumeMessage message)
        {
            try
            {
                _logger.LogDebug($"[Redis] Processing message from channel '{message.ChannelName}'");

                // Create a scoped service provider for this message
                using var scope = _serviceProvider.CreateScope();
                var consumerInstance = scope.ServiceProvider.GetRequiredService(_consumerMetadata.ConsumerType);

                // Get the method parameters
                var parameters = _consumerMetadata.HandlerMethod.GetParameters();
                var methodArgs = new object[parameters.Length];

                // For Redis pub/sub, we typically have one parameter which is the message content
                if (parameters.Length > 0)
                {
                    var messageType = parameters[0].ParameterType;
                    
                    if (messageType == typeof(string))
                    {
                        methodArgs[0] = message.Message;
                    }
                    else
                    {
                        // Try to deserialize the message to the expected type
                        methodArgs[0] = JsonConvert.DeserializeObject(message.Message, messageType);
                    }
                }

                // Invoke the consumer method
                var result = _consumerMetadata.HandlerMethod.Invoke(consumerInstance, methodArgs);

                // Handle async methods
                if (result is Task task)
                {
                    await task;
                }

                _logger.LogDebug($"[Redis] ✅ Successfully processed message from channel '{message.ChannelName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Redis] ❌ Failed to process message from channel '{message.ChannelName}': {ex.Message}");
            }
        }

        protected override void PreStart()
        {
            _logger.LogInformation($"[Redis] Consumer actor started for channel '{_consumerMetadata.ChannelAttribute.ChannelName}'");
            base.PreStart();
        }

        protected override void PostStop()
        {
            _logger.LogInformation($"[Redis] Consumer actor stopped for channel '{_consumerMetadata.ChannelAttribute.ChannelName}'");
            base.PostStop();
        }
    }
}
