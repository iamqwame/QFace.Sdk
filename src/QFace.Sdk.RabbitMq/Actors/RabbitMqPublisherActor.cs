namespace QFace.Sdk.RabbitMq.Actors
{
    internal class RabbitMqPublisherActor : BaseActor
    {
        private readonly ILogger<RabbitMqPublisherActor> _logger;
        private readonly RabbitMqOptions _options;
        private IConnection _connection;
        private IModel _channel;

        public RabbitMqPublisherActor(
            ILogger<RabbitMqPublisherActor> logger,
            IOptions<RabbitMqOptions> options, 
            IConnection connection, IModel channel)
        {
            _logger = logger;
            _connection = connection;
            _channel = channel;
            _options = options.Value;
            
            Initialize();
            
            ReceiveAsync<PublishMessage>(HandlePublishMessage);
        }

        private void Initialize()
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    Uri = new Uri(_options.ConnectionString),
                    AutomaticRecoveryEnabled = _options.AutomaticRecoveryEnabled
                };

                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[RabbitMQ] Failed to initialize publisher actor: {ex.Message}");
                throw;
            }
        }

        private async Task HandlePublishMessage(PublishMessage message)
        {
            await PublishWithRetryAsync(message.Message, message.RoutingKey,message.ExchangeName);
        }

        private async Task<bool> PublishWithRetryAsync(object message, string routingKey,string exchangeName, int currentRetry = 0)
        {
            try
            {
                if (_channel == null || _channel.IsClosed)
                {
                    _logger.LogWarning("[RabbitMQ] Channel is closed or null. Reinitializing...");
                    Initialize();
                }
                
                var payload = JsonConvert.SerializeObject(
                    message,
                    Formatting.None,
                    new JsonSerializerSettings { ReferenceLoopHandling = ReferenceLoopHandling.Ignore }
                );

                var body = Encoding.UTF8.GetBytes(payload);
                var properties = _channel.CreateBasicProperties();
                properties.Persistent = true;
                properties.MessageId = Guid.NewGuid().ToString();
                properties.Timestamp = new AmqpTimestamp(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

                _channel.ConfirmSelect();

                _channel.BasicPublish(
                    exchange: exchangeName,
                    routingKey: routingKey,
                    basicProperties: properties,
                    body: body
                );

                bool confirmed = _channel.WaitForConfirms(TimeSpan.FromSeconds(5));

                if (confirmed)
                {
                    _logger.LogInformation($"[RabbitMQ] ✅ Published message {properties.MessageId} to exchange '{exchangeName}' with routing key '{routingKey}'");
                    return true;
                }
                else
                {
                    _logger.LogWarning($"[RabbitMQ] ⚠ Message {properties.MessageId} not confirmed");
                    
                    if (currentRetry < _options.RetryCount)
                    {
                        _logger.LogInformation($"[RabbitMQ] Retrying publish ({currentRetry + 1}/{_options.RetryCount})...");
                        await Task.Delay(_options.RetryIntervalMs);
                        return await PublishWithRetryAsync(message, routingKey, exchangeName,currentRetry + 1);
                    }
                    
                    return false;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[RabbitMQ] ❌ Failed to publish message. Error: {ex.Message}");
                
                if (currentRetry < _options.RetryCount)
                {
                    _logger.LogInformation($"[RabbitMQ] Retrying publish after error ({currentRetry + 1}/{_options.RetryCount})...");
                    await Task.Delay(_options.RetryIntervalMs);
                    return await PublishWithRetryAsync(message, routingKey, exchangeName,currentRetry + 1);
                }
                
                return false;
            }
        }

        protected override void PostStop()
        {
            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();
            
            base.PostStop();
        }
    }
}