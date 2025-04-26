using Microsoft.Extensions.DependencyInjection.Extensions;

namespace QFace.Sdk.MongoDb;

/// <summary>
/// Extension methods for registering MongoDB services with multi-tenant support
/// </summary>
public static class MongoDbServiceExtensions
{
    #region Base MongoDB Setup

    /// <summary>
    /// Adds MongoDB services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="sectionName">The configuration section name (default: "MongoDb")</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "MongoDb")
    {
        // Register options
        services.Configure<MongoDbOptions>(configuration.GetSection(sectionName));
        
        // Register MongoDB services
        services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
        services.AddSingleton<ICollectionNamingService, CollectionNamingService>();
        
        // Register MongoDB client and database
        services.AddSingleton<IMongoClient>(sp => sp.GetRequiredService<IMongoDbClientFactory>().GetClient());
        services.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<IMongoDbClientFactory>().GetDatabase());
        
        // Register provider for multi-tenant support
        services.TryAddSingleton<IMongoDbProvider, MongoDbProvider>();
        
        // Configure MongoDB serialization conventions
        ConfigureConventions();
        
        return services;
    }
    
    /// <summary>
    /// Adds MongoDB services with a specific database name
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The connection string</param>
    /// <param name="databaseName">The database name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        string connectionString,
        string databaseName)
    {
        // Create configuration with connection string and database name
        var options = new MongoDbOptions
        {
            ConnectionString = connectionString,
            DatabaseName = databaseName
        };
        
        // Register options
        services.Configure<MongoDbOptions>(opt =>
        {
            opt.ConnectionString = options.ConnectionString;
            opt.DatabaseName = options.DatabaseName;
        });
        
        // Register MongoDB services
        services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
        services.AddSingleton<ICollectionNamingService, CollectionNamingService>();
        
        // Register MongoDB client and database
        services.AddSingleton<IMongoClient>(sp => sp.GetRequiredService<IMongoDbClientFactory>().GetClient());
        services.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<IMongoDbClientFactory>().GetDatabase());
        
        // Register provider for multi-tenant support
        services.TryAddSingleton<IMongoDbProvider, MongoDbProvider>();
        
        // Configure MongoDB serialization conventions
        ConfigureConventions();
        
        return services;
    }

    #endregion

    #region Single-Tenant Repository Registration

    /// <summary>
    /// Adds a repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <typeparam name="TRepository">The repository type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoRepository<TDocument, TRepository>(
        this IServiceCollection services,
        string collectionName = null) 
        where TDocument : BaseDocument
        where TRepository : class, IMongoRepository<TDocument>
    {
        // Register repository with either custom collection name or auto-generated name
        if (string.IsNullOrEmpty(collectionName))
        {
            services.AddScoped<IMongoRepository<TDocument>, TRepository>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var namingService = sp.GetRequiredService<ICollectionNamingService>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TRepository>>();
                
                var resolvedName = namingService.GetCollectionName<TDocument>();
                return (TRepository)Activator.CreateInstance(
                    typeof(TRepository),
                    database,
                    resolvedName,
                    logger);
            });
        }
        else
        {
            services.AddScoped<IMongoRepository<TDocument>, TRepository>(sp =>
            {
                var database = sp.GetRequiredService<IMongoDatabase>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TRepository>>();
                
                return (TRepository)Activator.CreateInstance(
                    typeof(TRepository),
                    database,
                    collectionName,
                    logger);
            });
        }
        
        // Register repository interface directly
        services.AddScoped<TRepository>(sp => (TRepository)sp.GetRequiredService<IMongoRepository<TDocument>>());
        
        return services;
    }
    
    /// <summary>
    /// Adds a default repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoRepository<TDocument>(
        this IServiceCollection services,
        string collectionName = null) 
        where TDocument : BaseDocument
    {
        return services.AddMongoRepository<TDocument, MongoRepository<TDocument>>(collectionName);
    }

    #endregion

    #region Multi-Tenant Repository Registration
    
    /// <summary>
    /// Adds a multi-tenant repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <typeparam name="TRepository">The repository type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="tenantProvider">Function to resolve the tenant ID from the service provider</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <param name="databaseNameFormat">Optional database name format</param>
    /// <param name="lifetime">The service lifetime (default: Scoped)</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMultiTenantRepository<TDocument, TRepository>(
        this IServiceCollection services,
        Func<IServiceProvider, string> tenantProvider,
        string collectionName = null,
        string databaseNameFormat = "{0}_db",
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TDocument : BaseDocument
        where TRepository : class, IMongoRepository<TDocument>
    {
        // Determine service descriptor based on lifetime
        var serviceDescriptor = new ServiceDescriptor(
            typeof(IMongoRepository<TDocument>),
            sp =>
            {
                var dbProvider = sp.GetRequiredService<IMongoDbProvider>();
                var namingService = sp.GetRequiredService<ICollectionNamingService>();
                var logger = sp.GetRequiredService<Microsoft.Extensions.Logging.ILogger<TRepository>>();
                var tenantId = tenantProvider(sp);
                
                if (string.IsNullOrEmpty(tenantId))
                {
                    throw new InvalidOperationException("Tenant ID cannot be null or empty");
                }
                
                var resolvedCollection = collectionName ?? namingService.GetCollectionName<TDocument>();
                
                return (TRepository)Activator.CreateInstance(
                    typeof(TRepository),
                    dbProvider,
                    resolvedCollection,
                    tenantId,
                    logger,
                    databaseNameFormat);
            },
            lifetime);
        
        services.Add(serviceDescriptor);
        
        // Register repository interface directly with the same lifetime
        services.Add(new ServiceDescriptor(
            typeof(TRepository),
            sp => (TRepository)sp.GetRequiredService<IMongoRepository<TDocument>>(),
            lifetime));
        
        return services;
    }
    
    /// <summary>
    /// Adds a default multi-tenant repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="tenantProvider">Function to resolve the tenant ID from the service provider</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <param name="databaseNameFormat">Optional database name format</param>
    /// <param name="lifetime">The service lifetime (default: Scoped)</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMultiTenantRepository<TDocument>(
        this IServiceCollection services,
        Func<IServiceProvider, string> tenantProvider,
        string collectionName = null,
        string databaseNameFormat = "{0}_db",
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TDocument : BaseDocument
    {
        return services.AddMultiTenantRepository<TDocument, MultiTenantRepository<TDocument>>(
            tenantProvider, collectionName, databaseNameFormat, lifetime);
    }
    
    #endregion

    #region Helper Methods
    
    /// <summary>
    /// Configures MongoDB serialization conventions
    /// </summary>
    private static void ConfigureConventions()
    {
        try
        {
            // Register ID generator for string type
            // We need to try-catch here since MongoDB doesn't have a clean way to check
            // if a type is already registered with an ID generator
            BsonSerializer.RegisterIdGenerator(typeof(string), StringObjectIdGenerator.Instance);
        }
        catch (BsonSerializationException)
        {
            // If the type is already registered, an exception will be thrown
            // We can safely ignore this exception
        }

        // Configure conventions
        var pack = new ConventionPack
        {
            new CamelCaseElementNameConvention(),
            new EnumRepresentationConvention(BsonType.String),
            new IgnoreExtraElementsConvention(true),
            new IgnoreIfNullConvention(true)
        };

        ConventionRegistry.Register("QFace.Sdk.MongoDb Conventions", pack, t => true);
    }
    
    #endregion
}

