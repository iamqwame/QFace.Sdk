using Microsoft.Extensions.Logging;
using Testcontainers.MongoDb;

namespace QFace.Sdk.MongoDb.Tests;

/// <summary>
/// Manager for MongoDB test container
/// </summary>
public class MongoDbContainerManager : IAsyncDisposable
{
    private readonly MongoDbContainer _container;
    private readonly ILogger<MongoDbContainerManager> _logger;
    private readonly string _databaseName;
    private bool _started;

    public MongoDbContainerManager(ILogger<MongoDbContainerManager> logger, string databaseName = null)
    {
        _logger = logger;
        _databaseName = databaseName ?? $"test-db-{Guid.NewGuid()}";

        _container = new MongoDbBuilder()
            .WithImage("mongo:latest")
            .Build();
    }

    public string GetConnectionString()
    {
        EnsureContainerStarted();
        return _container.GetConnectionString();
    }

    public string GetDatabaseName()
    {
        EnsureContainerStarted();
        return _databaseName;
    }

    public async Task StartAsync()
    {
        if (_started)
        {
            _logger.LogInformation("MongoDB test container already started.");
            return;
        }

        _logger.LogInformation("Starting MongoDB test container...");
        await _container.StartAsync();
        _started = true;
        _logger.LogInformation("MongoDB test container started with connection string {ConnectionString}", 
            MaskConnectionString(_container.GetConnectionString()));
    }

    public async ValueTask DisposeAsync()
    {
        if (!_started)
        {
            return;
        }

        _logger.LogInformation("Stopping MongoDB test container...");
        await _container.DisposeAsync();
        _logger.LogInformation("MongoDB test container stopped");
    }

    private void EnsureContainerStarted()
    {
        if (!_started)
        {
            throw new InvalidOperationException("MongoDB container is not started. Call StartAsync first.");
        }
    }

    private string MaskConnectionString(string connectionString)
    {
        if (string.IsNullOrEmpty(connectionString))
        {
            return connectionString;
        }

        return System.Text.RegularExpressions.Regex.Replace(
            connectionString,
            "(?<=mongodb://[^:]+:)[^@]+(?=@)",
            "********"
        );
    }
}
