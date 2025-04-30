namespace QFace.Sdk.MongoDb;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;

/// <summary>
/// Extension methods for registering MongoDB services
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
    /// <param name="assembliesToScan">Assemblies to scan for document types and repositories. If provided, repositories will be automatically registered.</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "MongoDb",
        Assembly[]? assembliesToScan = null)
    {
        // Register options
        services.Configure<MongoDbOptions>(configuration.GetSection(sectionName));

        // Register MongoDB services
        services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
        services.AddSingleton<ICollectionNamingService, CollectionNamingService>();

        // Register MongoDB client and database
        services.AddSingleton<IMongoClient>(sp => sp.GetRequiredService<IMongoDbClientFactory>().GetClient());
        services.AddSingleton<IMongoDatabase>(sp => sp.GetRequiredService<IMongoDbClientFactory>().GetDatabase());

        // Configure MongoDB serialization conventions
        ConfigureConventions();

        // Scan for repositories if assembliesToScan is provided
        if (assembliesToScan != null)
        {
            ScanAndRegisterRepositories(services, assembliesToScan);
        }

        return services;
    }

    /// <summary>
    /// Adds MongoDB services with a specific database name
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">The connection string</param>
    /// <param name="databaseName">The database name</param>
    /// <param name="assembliesToScan">Assemblies to scan for document types and repositories. If provided, repositories will be automatically registered.</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoDb(
        this IServiceCollection services,
        string connectionString,
        string databaseName,
        Assembly[]? assembliesToScan = null)
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

        // Configure MongoDB serialization conventions
        ConfigureConventions();

        // Scan for repositories if assembliesToScan is provided
        if (assembliesToScan != null)
        {
            ScanAndRegisterRepositories(services, assembliesToScan);
        }

        return services;
    }

    #endregion

    
    #region Repository with Custom Connection String

    /// <summary>
    /// Adds a repository with a dedicated connection string
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <typeparam name="TRepository">The repository type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">MongoDB connection string for this repository</param>
    /// <param name="databaseName">The database name for this repository</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoRepositoryWithConnection<TDocument, TRepository>(
        this IServiceCollection services,
        string connectionString,
        string databaseName,
        string? collectionName = null)
        where TDocument : BaseDocument
        where TRepository : class, IMongoRepository<TDocument>
    {
        // Configure MongoDB serialization conventions (in case not already done)
        ConfigureConventions();

        // Resolve collection name if not provided
        if (string.IsNullOrEmpty(collectionName))
        {
            // Register repository with auto-generated collection name
            services.AddScoped<IMongoRepository<TDocument>, TRepository>(sp =>
            {
                var namingService = sp.GetRequiredService<ICollectionNamingService>();
                var logger = sp.GetRequiredService<ILogger<TRepository>>();

                // Create dedicated client and database for this repository
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);

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
            // Register repository with explicit collection name
            services.AddScoped<IMongoRepository<TDocument>, TRepository>(sp =>
            {
                var logger = sp.GetRequiredService<ILogger<TRepository>>();

                // Create dedicated client and database for this repository
                var client = new MongoClient(connectionString);
                var database = client.GetDatabase(databaseName);

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
    /// Adds a default repository with a dedicated connection string
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="connectionString">MongoDB connection string for this repository</param>
    /// <param name="databaseName">The database name for this repository</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoRepositoryWithConnection<TDocument>(
        this IServiceCollection services,
        string connectionString,
        string databaseName,
        string? collectionName = null)
        where TDocument : BaseDocument
    {
        return services.AddMongoRepositoryWithConnection<TDocument, MongoRepository<TDocument>>(
            connectionString,
            databaseName,
            collectionName);
    }

    /// <summary>
    /// Adds a repository with a dedicated connection string from configuration
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <typeparam name="TRepository">The repository type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="sectionName">The configuration section name</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoRepositoryWithConnection<TDocument, TRepository>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        string? collectionName = null)
        where TDocument : BaseDocument
        where TRepository : class, IMongoRepository<TDocument>
    {
        // Get connection options from configuration
        var options = new MongoDbOptions();
        configuration.GetSection(sectionName).Bind(options);

        if (string.IsNullOrEmpty(options.ConnectionString))
        {
            throw new InvalidOperationException($"MongoDB connection string not found in configuration section '{sectionName}'");
        }

        if (string.IsNullOrEmpty(options.DatabaseName))
        {
            throw new InvalidOperationException($"MongoDB database name not found in configuration section '{sectionName}'");
        }

        // Register repository with connection from configuration
        return services.AddMongoRepositoryWithConnection<TDocument, TRepository>(
            options.ConnectionString,
            options.DatabaseName,
            collectionName);
    }

    /// <summary>
    /// Adds a default repository with a dedicated connection string from configuration
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="sectionName">The configuration section name</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoRepositoryWithConnection<TDocument>(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName,
        string? collectionName = null)
        where TDocument : BaseDocument
    {
        return services.AddMongoRepositoryWithConnection<TDocument, MongoRepository<TDocument>>(
            configuration,
            sectionName,
            collectionName);
    }

    #endregion
    
    
    #region Repository Registration

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
        string? collectionName = null)
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
                var logger = sp.GetRequiredService<ILogger<TRepository>>();

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
                var logger = sp.GetRequiredService<ILogger<TRepository>>();

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
        string? collectionName = null)
        where TDocument : BaseDocument
    {
        return services.AddMongoRepository<TDocument, MongoRepository<TDocument>>(collectionName);
    }

    #endregion

    #region Repository Scanning

    /// <summary>
    /// Scans assemblies for document types and registers repositories for them
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies to scan</param>
    /// <summary>
    /// Scans assemblies for document types and registers repositories for them
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies to scan</param>
    private static void ScanAndRegisterRepositories(IServiceCollection services, Assembly[]? assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            return;
        }

        // Find all document types (classes that inherit from BaseDocument)
        var documentTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(BaseDocument).IsAssignableFrom(t))
            .ToList();

        // Find all custom repository implementations
        var repositoryTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && IsMongoRepository(t))
            .ToList();

        // Group repositories by document type (handling multiple implementations for the same document type)
        var repositoryTypesByDocumentType = new Dictionary<Type, Type>();
        foreach (var repositoryType in repositoryTypes)
        {
            var documentType = GetDocumentType(repositoryType);
            // Only add if not already in the dictionary or if it's a more specific implementation
            if (!repositoryTypesByDocumentType.TryGetValue(documentType, out var existingType) ||
                existingType.IsAssignableFrom(repositoryType))
            {
                repositoryTypesByDocumentType[documentType] = repositoryType;
            }
        }

        // Register repositories for each document type
        foreach (var documentType in documentTypes)
        {
            // Try to get custom repository implementation
            if (repositoryTypesByDocumentType.TryGetValue(documentType, out var customRepositoryType))
            {
                // Register with custom repository implementation
                RegisterRepositoryWithType(services, documentType, customRepositoryType);
            }
            else
            {
                // Register with default repository implementation
                RegisterDefaultRepository(services, documentType);
            }
        }
    }


    /// <summary>
    /// Registers a repository with a custom implementation type
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="documentType">The document type</param>
    /// <param name="repositoryType">The repository type</param>
    private static void RegisterRepositoryWithType(
        IServiceCollection services,
        Type documentType,
        Type repositoryType)
    {
        // Get the generic AddMongoRepository method
        var method = typeof(MongoDbServiceExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(AddMongoRepository) &&
                        m.GetGenericArguments().Length == 2);

        // Create the generic method with the specific document and repository types
        var genericMethod = method.MakeGenericMethod(documentType, repositoryType);

        // Invoke the method to register the repository
        genericMethod.Invoke(null, new object[] { services, null });
    }

    /// <summary>
    /// Registers a default repository for a document type
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="documentType">The document type</param>
    private static void RegisterDefaultRepository(IServiceCollection services, Type documentType)
    {
        // Get the generic AddMongoRepository method with one type parameter
        var method = typeof(MongoDbServiceExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(AddMongoRepository) &&
                        m.GetGenericArguments().Length == 1);

        // Create the generic method with the specific document type
        var genericMethod = method.MakeGenericMethod(documentType);

        // Invoke the method to register the repository
        genericMethod.Invoke(null, new object[] { services, null });
    }

    /// <summary>
    /// Checks if a type is a MongoDB repository
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is a MongoDB repository, false otherwise</returns>
    private static bool IsMongoRepository(Type type)
    {
        // Check all interfaces implemented by the type
        foreach (var interfaceType in type.GetInterfaces())
        {
            // Check if the interface is IMongoRepository<T>
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IMongoRepository<>))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Gets the document type for a repository type
    /// </summary>
    /// <param name="repositoryType">The repository type</param>
    /// <returns>The document type</returns>
    private static Type GetDocumentType(Type repositoryType)
    {
        // Find the IMongoRepository<T> interface
        var mongoRepositoryInterface = repositoryType.GetInterfaces()
            .First(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IMongoRepository<>));

        // Get the document type from the interface
        return mongoRepositoryInterface.GetGenericArguments()[0];
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