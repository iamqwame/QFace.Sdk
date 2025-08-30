namespace QFace.Sdk.RedisMq.Services;

public class RedisMqConnectionProvider : IDisposable
{
    private readonly IConnectionMultiplexer _connection;
    private readonly IDatabase _database;
    private readonly ISubscriber _subscriber;
    private bool _disposed = false;

    public RedisMqConnectionProvider(IOptions<RedisMqOptions> options)
    {
        var configuration = ConfigurationOptions.Parse(options.Value.ConnectionString);
        configuration.ConnectTimeout = options.Value.ConnectTimeout;
        configuration.SyncTimeout = options.Value.SyncTimeout;
        configuration.AbortOnConnectFail = options.Value.AbortOnConnectFail;
        configuration.DefaultDatabase = options.Value.Database;

        _connection = ConnectionMultiplexer.Connect(configuration);
        _database = _connection.GetDatabase();
        _subscriber = _connection.GetSubscriber();
    }

    public IConnectionMultiplexer Connection => _connection;
    public IDatabase Database => _database;
    public ISubscriber Subscriber => _subscriber;

    public void Dispose()
    {
        if (!_disposed)
        {
            _connection?.Dispose();
            _disposed = true;
        }
    }
}
