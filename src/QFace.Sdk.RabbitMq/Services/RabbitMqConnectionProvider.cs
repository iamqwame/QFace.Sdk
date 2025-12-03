namespace QFace.Sdk.RabbitMq.Services;

public class RabbitMqConnectionProvider : IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqConnectionProvider> _logger;
    private readonly RabbitMqOptions _options;

    public RabbitMqConnectionProvider(
        IOptions<RabbitMqOptions> options,
        ILogger<RabbitMqConnectionProvider> logger)
    {
        _logger = logger;
        _options = options.Value;

        try
        {
            var factory = ConnectionFactoryHelper.CreateConnectionFactory(_options);

            _connection = factory.CreateConnection();
            _channel = _connection.CreateModel();

            _logger.LogInformation("[RabbitMQ] Connection provider initialized successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RabbitMQ] Failed to initialize connection provider");
            throw;
        }
    }

    public IConnection Connection => _connection;
    public IModel Channel => _channel;

    public void Dispose()
    {
        try
        {
            // Add null checks and try-catch for each disposal
            if (_channel != null && !_channel.IsClosed)
            {
                try
                {
                    _channel.Close();
                }
                catch (ObjectDisposedException)
                {
                    // Channel already disposed, which is fine
                    _logger.LogDebug("[RabbitMQ] Channel was already disposed by another component");
                }
            }

            if (_channel != null)
            {
                try
                {
                    _channel.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }
            }

            if (_connection != null && _connection.IsOpen)
            {
                try
                {
                    _connection.Close();
                }
                catch (ObjectDisposedException)
                {
                    // Connection already disposed, which is fine
                    _logger.LogDebug("[RabbitMQ] Connection was already disposed by another component");
                }
            }

            if (_connection != null)
            {
                try
                {
                    _connection.Dispose();
                }
                catch (ObjectDisposedException)
                {
                    // Already disposed, ignore
                }
            }

            _logger.LogInformation("[RabbitMQ] Connection provider disposed");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "[RabbitMQ] Error disposing connection provider");
        }
    }
}