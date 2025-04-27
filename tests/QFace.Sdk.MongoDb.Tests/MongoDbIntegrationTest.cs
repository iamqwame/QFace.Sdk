using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using QFace.Sdk.MongoDb.Models;
using QFace.Sdk.MongoDb.Repositories;
using Xunit;

namespace QFace.Sdk.MongoDb.Tests;

/// <summary>
/// Base class for MongoDB integration tests
/// </summary>
public abstract class MongoDbIntegrationTest : IAsyncLifetime
{
    protected readonly IServiceProvider ServiceProvider;
    protected readonly MongoDbContainerManager ContainerManager;
    protected readonly string DatabaseName;
    
    /// <summary>
    /// Creates a new MongoDB integration test
    /// </summary>
    protected MongoDbIntegrationTest()
    {
        // Create service collection
        var services = new ServiceCollection();
        
        // Add logging
        // services.AddLogging(builder => 
        //     builder.AddConsole().SetMinimumLevel(LogLevel.Information));
        
        // Create container manager
        var loggerFactory = services.BuildServiceProvider().GetRequiredService<ILoggerFactory>();
        var containerLogger = loggerFactory.CreateLogger<MongoDbContainerManager>();
        ContainerManager = new MongoDbContainerManager(containerLogger);
        
        // Configure MongoDB options
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string>
            {
                {"MongoDb:ConnectionString", ContainerManager.GetConnectionString()},
                {"MongoDb:DatabaseName", ContainerManager.GetDatabaseName()}
            })
            .Build();
        
        DatabaseName = ContainerManager.GetDatabaseName();
        
        // Register MongoDB services
        services.AddMongoDb(configuration);
        
        // Register any additional services
        ConfigureServices(services);
        
        // Build service provider
        ServiceProvider = services.BuildServiceProvider();
    }
    
    /// <summary>
    /// Override to register additional services
    /// </summary>
    /// <param name="services">Service collection</param>
    protected virtual void ConfigureServices(IServiceCollection services)
    {
    }
    
    /// <summary>
    /// Gets a service from the service provider
    /// </summary>
    /// <typeparam name="T">Service type</typeparam>
    /// <returns>Service instance</returns>
    protected T GetService<T>() where T : notnull
    {
        return ServiceProvider.GetRequiredService<T>();
    }
    
    /// <summary>
    /// Registers a repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">Document type</typeparam>
    /// <param name="services">Service collection</param>
    /// <param name="collectionName">Optional collection name (defaults to type name)</param>
    protected void RegisterRepository<TDocument>(IServiceCollection services, string collectionName = null) 
        where TDocument : BaseDocument
    {
        services.AddMongoRepository<TDocument>(collectionName);
    }
    
    /// <summary>
    /// Gets a repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">Document type</typeparam>
    /// <returns>Repository instance</returns>
    protected IMongoRepository<TDocument> GetRepository<TDocument>() where TDocument : BaseDocument
    {
        return ServiceProvider.GetRequiredService<IMongoRepository<TDocument>>();
    }
    
    /// <summary>
    /// Initializes the test by starting the MongoDB container
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        await ContainerManager.StartAsync();
    }
    
    /// <summary>
    /// Disposes the test by stopping the MongoDB container
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        await ContainerManager.DisposeAsync();
    }
}