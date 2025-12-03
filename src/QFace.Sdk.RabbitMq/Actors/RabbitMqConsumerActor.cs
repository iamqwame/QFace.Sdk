namespace QFace.Sdk.RabbitMq.Actors
{
    internal class RabbitMqConsumerActor : BaseActor
    {
        private readonly ILogger<RabbitMqConsumerActor> _logger;
        private readonly RabbitMqOptions _options;
        /// <summary>
        /// Root service provider (not scoped). Actors are long-lived and create scopes manually when needed.
        /// </summary>
        private readonly IServiceProvider _serviceProvider;
        private readonly ConsumerMetadata _consumerMetadata;
        private IConnection _connection;
        private IModel _channel;
        private string _consumerTag;

        /// <summary>
        /// Initializes a new instance of RabbitMqConsumerActor.
        /// Note: Uses IServiceProvider (root, not scoped) to create scopes manually when processing messages.
        /// </summary>
        public RabbitMqConsumerActor(
            ILogger<RabbitMqConsumerActor> logger,
            IOptions<RabbitMqOptions> options,
            IServiceProvider serviceProvider, // Root service provider - actors are long-lived
            ConsumerMetadata consumerMetadata)
        {
            _logger = logger;
            _options = options.Value;
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
            _consumerMetadata = consumerMetadata;

            Initialize();
            StartConsuming();

            // Use Receive from BaseActor for message handling
            ReceiveAsync<ConsumeMessage>(HandleConsumeMessage);
        }

        private void Initialize()
        {
            var exchangeName = _consumerMetadata.TopicAttribute.ExchangeName;
            _logger.LogInformation(
                $"[RabbitMQ] Initializing consumer actor for {_consumerMetadata.ConsumerType.Name}.{_consumerMetadata.HandlerMethod.Name}");

            // Retry initialization with exponential backoff
            int attempt = 0;
            Exception lastException = null;
            
            while (attempt <= _options.RetryCount)
            {
                try
                {
                    var factory = Services.ConnectionFactoryHelper.CreateConnectionFactory(_options);
                    
                    _logger.LogInformation($"[RabbitMQ] Creating connection to {factory.Uri} (attempt {attempt + 1}/{_options.RetryCount + 1})");
                    _connection = factory.CreateConnection();
                    _channel = _connection.CreateModel();
                    _logger.LogInformation($"[RabbitMQ] Connection created successfully");

                    try
                    {
                        if (_options.PassiveExchange)
                        {
                            // Check if exchange exists with passive declare
                            try
                            {
                                _logger.LogInformation(
                                    $"[RabbitMQ] Checking if exchange '{exchangeName}' exists (passive mode)");
                                _channel.ExchangeDeclarePassive(exchangeName);
                                _logger.LogInformation($"[RabbitMQ] Exchange '{exchangeName}' exists");
                            }
                            catch (Exception)
                            {
                                _logger.LogInformation(
                                    $"[RabbitMQ] Exchange '{exchangeName}' doesn't exist, will create it");

                                // Need to recreate channel since it's closed after a passive declare failure
                                _channel?.Dispose();
                                _channel = _connection.CreateModel();

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
                            // Just try to create the exchange directly (will verify if it exists with correct settings)
                            _logger.LogInformation($"[RabbitMQ] Creating exchange '{exchangeName}' if needed");
                            _channel.ExchangeDeclare(
                                exchange: exchangeName,
                                type: _options.ExchangeType,
                                durable: true,
                                autoDelete: false
                            );
                            _logger.LogInformation($"[RabbitMQ] Exchange '{exchangeName}' is ready");
                        }

                        // Rest of the initialization code...
                        var queueName = string.IsNullOrEmpty(_consumerMetadata.TopicAttribute.QueueName)
                            ? $"{_consumerMetadata.ConsumerType.Name}_{_consumerMetadata.HandlerMethod.Name}_queue"
                            : _consumerMetadata.TopicAttribute.QueueName;

                        _logger.LogInformation($"[RabbitMQ] Using queue name: '{queueName}'");

                        _channel.QueueDeclare(
                            queue: queueName,
                            durable: _consumerMetadata.TopicAttribute.Durable,
                            exclusive: false,
                            autoDelete: _consumerMetadata.TopicAttribute.AutoDelete
                        );

                        var routingKey = _options.ExchangeType == ExchangeType.Fanout
                            ? ""
                            : _consumerMetadata.TopicAttribute.RoutingKey;

                        _channel.QueueBind(
                            queue: queueName,
                            exchange: exchangeName,
                            routingKey: routingKey
                        );

                        _channel.BasicQos(
                            prefetchSize: 0,
                            prefetchCount: (ushort)_consumerMetadata.TopicAttribute.PrefetchCount,
                            global: false
                        );

                        _consumerMetadata.TopicAttribute.QueueName = queueName;

                        _logger.LogInformation(
                            $"[RabbitMQ] Consumer actor initialized for queue '{queueName}' with routing key '{routingKey}' on exchange '{exchangeName}'");
                        
                        // Success - break out of retry loop
                        return;
                    }
                    catch (Exception exchangeEx)
                    {
                        _logger.LogError(exchangeEx, $"[RabbitMQ] Failed to declare exchange: {exchangeEx.Message}");
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    attempt++;
                    
                    if (attempt <= _options.RetryCount)
                    {
                        var delay = Services.ConnectionFactoryHelper.CalculateExponentialBackoff(
                            _options.RetryIntervalMs, 
                            attempt - 1
                        );
                        
                        _logger.LogWarning(
                            $"[RabbitMQ] Initialization attempt {attempt} failed: {ex.Message}. " +
                            $"Retrying in {delay}ms (attempt {attempt + 1}/{_options.RetryCount + 1})...");
                        
                        // Clean up failed connection
                        try
                        {
                            _channel?.Dispose();
                            _connection?.Dispose();
                        }
                        catch { }
                        
                        Thread.Sleep(delay);
                    }
                    else
                    {
                        _logger.LogError(ex, 
                            $"[RabbitMQ] Failed to initialize consumer actor after {_options.RetryCount + 1} attempts: {ex.Message}");
                    }
                }
            }
            
            // If we get here, all retries failed
            throw new InvalidOperationException(
                $"Failed to initialize RabbitMQ consumer actor after {_options.RetryCount + 1} attempts", 
                lastException);
        }

        private void StartConsuming()
        {
            _logger.LogInformation(
                $"[RabbitMQ] Starting to consume from exchange '{_consumerMetadata.TopicAttribute.ExchangeName}' in queue '{_consumerMetadata.TopicAttribute.QueueName}'");

            try
            {
                var consumer = new EventingBasicConsumer(_channel);
                var self = Self;
                consumer.Received += (model, ea) =>
                {
                    _logger.LogDebug(
                        $"[RabbitMQ] Message received on consumer tag '{string.Join(",", consumer.ConsumerTags)}' from queue '{_consumerMetadata.TopicAttribute.QueueName}', routing key: '{ea.RoutingKey}'");
            
                    // Use the stored reference instead of Self
                    var message = new ConsumeMessage(consumer.ConsumerTags, ea,
                        _consumerMetadata.TopicAttribute.QueueName);
            
                    // Tell the message to the actor instead of Self
                    self.Tell(message);
                };

                consumer.Shutdown += (sender, ea) =>
                {
                    _logger.LogWarning($"[RabbitMQ] Consumer shutdown: {ea.ReplyText}");
                };

                consumer.ConsumerCancelled += (sender, ea) =>
                {
                    _logger.LogWarning($"[RabbitMQ] Consumer cancelled: {ea.ConsumerTags}");
                };

                _consumerTag = _channel.BasicConsume(
                    queue: _consumerMetadata.TopicAttribute.QueueName,
                    autoAck: false,
                    consumer: consumer
                );

                _logger.LogInformation(
                    $"[RabbitMQ] Successfully started consuming from queue '{_consumerMetadata.TopicAttribute.QueueName}' with consumer tag '{_consumerTag}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"[RabbitMQ] Failed to start consuming from queue '{_consumerMetadata.TopicAttribute.QueueName}'");
                throw;
            }
        }

        private async Task HandleConsumeMessage(ConsumeMessage message)
        {
            string messageBody = null;

            try
            {
                messageBody = Encoding.UTF8.GetString(message.DeliveryArgs.Body.ToArray());
                _logger.LogInformation(
                    $"[RabbitMQ] Received message on queue '{message.QueueName}', routing key: '{message.DeliveryArgs.RoutingKey}', delivery tag: {message.DeliveryArgs.DeliveryTag}");
                _logger.LogDebug($"[RabbitMQ] Message body: {messageBody}");

                using (var scope = _serviceProvider.CreateScope())
                {
                    try
                    {
                        _logger.LogDebug(
                            $"[RabbitMQ] Creating instance of consumer type '{_consumerMetadata.ConsumerType.FullName}'");
                        var handlerInstance = scope.ServiceProvider.GetRequiredService(_consumerMetadata.ConsumerType);
                        _logger.LogDebug($"[RabbitMQ] Successfully created consumer instance");

                        var parameterType = _consumerMetadata.HandlerMethod.GetParameters().FirstOrDefault()
                            ?.ParameterType;
                        if (parameterType != null)
                        {
                            _logger.LogDebug($"[RabbitMQ] Deserializing message to type '{parameterType.Name}'");

                            var typedMessage = JsonConvert.DeserializeObject(messageBody, parameterType);
                            _logger.LogDebug($"[RabbitMQ] Successfully deserialized message");

                            _logger.LogDebug(
                                $"[RabbitMQ] Invoking handler method '{_consumerMetadata.HandlerMethod.Name}'");
                            var task = (Task)_consumerMetadata.HandlerMethod.Invoke(handlerInstance,
                                new[] { typedMessage });
                            if (task != null)
                            {
                                _logger.LogDebug($"[RabbitMQ] Awaiting handler task completion");
                                await task;
                                _logger.LogDebug($"[RabbitMQ] Handler task completed successfully");
                            }
                        }
                        else
                        {
                            _logger.LogWarning(
                                $"[RabbitMQ] Handler method '{_consumerMetadata.HandlerMethod.Name}' has no parameters");
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex,
                            $"[RabbitMQ] Error creating or invoking consumer '{_consumerMetadata.ConsumerType.Name}'");
                        throw;
                    }
                }

                _logger.LogDebug(
                    $"[RabbitMQ] Acknowledging message with delivery tag {message.DeliveryArgs.DeliveryTag}");
                _channel.BasicAck(message.DeliveryArgs.DeliveryTag, false);
                _logger.LogInformation($"[RabbitMQ] Successfully processed message from queue '{message.QueueName}'");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    $"[RabbitMQ] Error processing message from queue '{message.QueueName}': {messageBody}");

                _logger.LogDebug(
                    $"[RabbitMQ] Nacking message with delivery tag {message.DeliveryArgs.DeliveryTag}, requeue=true");
                _channel.BasicNack(message.DeliveryArgs.DeliveryTag, false, true);
            }
        }

        protected override void PostStop()
        {
            if (_channel != null && _channel.IsOpen && _consumerTag != null)
            {
                _channel.BasicCancel(_consumerTag);
            }

            _channel?.Close();
            _channel?.Dispose();
            _connection?.Close();
            _connection?.Dispose();

            base.PostStop();
        }
    }
}