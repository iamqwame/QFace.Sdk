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

        // Validate configuration
        if (string.IsNullOrEmpty(_options.ConnectionString))
        {
            var errorMessage = "RabbitMQ ConnectionString is not configured. " +
                              "Please add 'RabbitMq:ConnectionString' to your appsettings.json";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        try
        {
            _logger.LogInformation("[RabbitMQ] Initializing connection to: {ConnectionString}", 
                MaskConnectionString(_options.ConnectionString));
            
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

    /// <summary>
    /// Masks sensitive information in the connection string for logging
    /// </summary>
    private static string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
            return "null";

        try
        {
            var uri = new Uri(connectionString);
            var userInfo = uri.UserInfo;
            if (!string.IsNullOrEmpty(userInfo))
            {
                // Mask password: user:password -> user:****
                var parts = userInfo.Split(':');
                if (parts.Length > 1)
                {
                    var maskedUserInfo = $"{parts[0]}:****";
                    return connectionString.Replace(userInfo, maskedUserInfo);
                }
            }
            return connectionString;
        }
        catch
        {
            // If parsing fails, just mask the entire string
            return "****";
        }
    }

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