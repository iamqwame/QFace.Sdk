namespace QFace.Sdk.RabbitMq.Actors
{
    internal class RabbitMqPublisherActor : BaseActor
    {
        private readonly ILogger<RabbitMqPublisherActor> _logger;
        private readonly RabbitMqOptions _options;
        private IConnection _connection;
        private IModel _channel;
        private readonly object _channelLock = new object();
        private readonly HashSet<string> _declaredExchanges = new HashSet<string>();

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
                // Only initialize if we don't have a connection
                if (_connection == null || !_connection.IsOpen)
                {
                    var factory = new ConnectionFactory
                    {
                        Uri = new Uri(_options.ConnectionString),
                        AutomaticRecoveryEnabled = _options.AutomaticRecoveryEnabled
                    };

                    _connection = factory.CreateConnection();
                }

                if (_channel == null || _channel.IsClosed)
                {
                    _channel = _connection.CreateModel();
                    // Clear declared exchanges when we get a new channel
                    _declaredExchanges.Clear();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[RabbitMQ] Failed to initialize publisher actor: {ex.Message}");
                throw;
            }
        }

        private async Task HandlePublishMessage(PublishMessage message)
        {
            await PublishWithRetryAsync(message.Message, message.RoutingKey, message.ExchangeName);
        }

        private void EnsureExchangeExists(string exchangeName)
        {
            lock (_channelLock)
            {
                if (_declaredExchanges.Contains(exchangeName))
                {
                    _logger.LogDebug($"[RabbitMQ] Exchange '{exchangeName}' already declared in this session");
                    return;
                }

                try
                {
                    if (_options.PassiveExchange)
                    {
                        // Check if exchange exists with passive declare
                        try
                        {
                            _logger.LogInformation($"[RabbitMQ] Checking if exchange '{exchangeName}' exists (passive mode)");
                            _channel.ExchangeDeclarePassive(exchangeName);
                            _logger.LogInformation($"[RabbitMQ] Exchange '{exchangeName}' exists");
                        }
                        catch (Exception)
                        {
                            _logger.LogInformation($"[RabbitMQ] Exchange '{exchangeName}' doesn't exist, will create it");

                            // Need to recreate channel since it's closed after a passive declare failure
                            _channel?.Dispose();
                            _channel = _connection.CreateModel();
                            _declaredExchanges.Clear();

                            _channel.ExchangeDeclare(
                                exchange: exchangeName,
                                type: _options.ExchangeType,
                                durable: true,
                                autoDelete: false
                            );
                            _logger.LogInformation($"[RabbitMQ] Successfully created exchange '{exchangeName}'");
                        }
                    }
                    else
                    {
                        // Just try to create the exchange directly
                        _logger.LogInformation($"[RabbitMQ] Creating exchange '{exchangeName}' if needed");
                        _channel.ExchangeDeclare(
                            exchange: exchangeName,
                            type: _options.ExchangeType,
                            durable: true,
                            autoDelete: false
                        );
                        _logger.LogInformation($"[RabbitMQ] Exchange '{exchangeName}' is ready");
                    }

                    _declaredExchanges.Add(exchangeName);
                }
                catch (Exception exchangeEx)
                {
                    _logger.LogError(exchangeEx, $"[RabbitMQ] Failed to declare exchange: {exchangeEx.Message}");
                    throw;
                }
            }
        }

        private async Task<bool> PublishWithRetryAsync(object message, string routingKey, string exchangeName, int currentRetry = 0)
        {
            try
            {
                lock (_channelLock)
                {
                    if (_channel == null || _channel.IsClosed)
                    {
                        _logger.LogWarning("[RabbitMQ] Channel is closed or null. Reinitializing...");
                        Initialize();
                    }

                    // Ensure exchange exists before publishing
                    EnsureExchangeExists(exchangeName);
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
                        return await PublishWithRetryAsync(message, routingKey, exchangeName, currentRetry + 1);
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
                    
                    // If we get a channel-related error, try to reinitialize
                    if (ex is RabbitMQ.Client.Exceptions.AlreadyClosedException || 
                        ex is RabbitMQ.Client.Exceptions.OperationInterruptedException)
                    {
                        lock (_channelLock)
                        {
                            _channel?.Dispose();
                            _channel = null;
                            _declaredExchanges.Clear();
                        }
                    }
                    
                    await Task.Delay(_options.RetryIntervalMs);
                    return await PublishWithRetryAsync(message, routingKey, exchangeName, currentRetry + 1);
                }
                
                return false;
            }
        }

        protected override void PostStop()
        {
            lock (_channelLock)
            {
                _channel?.Close();
                _channel?.Dispose();
                _connection?.Close();
                _connection?.Dispose();
            }
            
            base.PostStop();
        }
    }
}