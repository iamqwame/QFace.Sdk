namespace QFace.Sdk.RedisMq.Actors
{
    internal class RedisMqPublisherActor : BaseActor
    {
        private readonly ILogger<RedisMqPublisherActor> _logger;
        private readonly RedisMqOptions _options;
        private readonly IConnectionMultiplexer _connection;
        private readonly ISubscriber _subscriber;

        public RedisMqPublisherActor(
            ILogger<RedisMqPublisherActor> logger,
            IOptions<RedisMqOptions> options, 
            IConnectionMultiplexer connection)
        {
            _logger = logger;
            _connection = connection;
            _subscriber = _connection.GetSubscriber();
            _options = options.Value;
            
            ReceiveAsync<PublishMessage>(HandlePublishMessage);
        }

        private async Task HandlePublishMessage(PublishMessage message)
        {
            await PublishWithRetryAsync(message.Message, message.ChannelName);
        }

        private async Task<bool> PublishWithRetryAsync(object message, string channelName, int currentRetry = 0)
        {
            try
            {
                var payload = JsonConvert.SerializeObject(
                    message,
                    Formatting.None,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
                );

                var subscriberCount = await _subscriber.PublishAsync(channelName, payload);

                if (subscriberCount > 0)
                {
                    _logger.LogInformation($"[Redis] ✅ Published message to channel '{channelName}' with {subscriberCount} subscribers");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"[Redis] ⚠ Published message to channel '{channelName}' but no subscribers");
                    
                    if (currentRetry < _options.RetryCount)
                    {
                        _logger.LogInformation($"[Redis] Retrying publish ({currentRetry + 1}/{_options.RetryCount})...");
                        await Task.Delay(_options.RetryIntervalMs);
                        return await PublishWithRetryAsync(message, channelName, currentRetry + 1);
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[Redis] ❌ Failed to publish message to channel '{channelName}'. Error: {ex.Message}");
                
                if (currentRetry < _options.RetryCount)
                {
                    _logger.LogInformation($"[Redis] Retrying publish after error ({currentRetry + 1}/{_options.RetryCount})...");
                    
                    await Task.Delay(_options.RetryIntervalMs);
                    return await PublishWithRetryAsync(message, channelName, currentRetry + 1);
                }
                
                return false;
            }
        }

        protected override void PostStop()
        {
            _logger.LogInformation("[Redis] Publisher actor stopped");
            base.PostStop();
        }
    }
}
