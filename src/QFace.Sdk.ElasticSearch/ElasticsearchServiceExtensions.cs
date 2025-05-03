using QFace.Sdk.Elasticsearch.Repositories;
using QFace.Sdk.Elasticsearch.Services;

namespace QFace.Sdk.Elasticsearch;

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Nest;

/// <summary>
/// Extension methods for registering Elasticsearch services
/// </summary>
public static class ElasticsearchServiceExtensions
{
    #region Base Elasticsearch Setup

    /// <summary>
    /// Adds Elasticsearch services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="sectionName">The configuration section name (default: "Elasticsearch")</param>
    /// <param name="assembliesToScan">Assemblies to scan for document types and repositories. If provided, repositories will be automatically registered.</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddElasticsearch(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = "Elasticsearch",
        Assembly[] assembliesToScan = null)
    {
        // Register options
        services.Configure<ElasticsearchOptions>(configuration.GetSection(sectionName));

        // Register Elasticsearch services
        services.AddSingleton<IElasticsearchClientFactory, ElasticsearchClientFactory>();
        services.AddSingleton<IIndexNamingService, IndexNamingService>();

        // Register Elasticsearch client
        services.AddSingleton<IElasticClient>(sp => 
            sp.GetRequiredService<IElasticsearchClientFactory>().GetClient());

        // Scan for repositories if assembliesToScan is provided
        if (assembliesToScan != null)
        {
            ScanAndRegisterRepositories(services, assembliesToScan);
        }

        return services;
    }

    /// <summary>
    /// Adds Elasticsearch services with explicit connection settings
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="nodeUrls">The Elasticsearch node URLs (comma-separated)</param>
    /// <param name="defaultIndexPrefix">The default index prefix</param>
    /// <param name="assembliesToScan">Assemblies to scan for document types and repositories. If provided, repositories will be automatically registered.</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddElasticsearch(
        this IServiceCollection services,
        string nodeUrls,
        string defaultIndexPrefix,
        Assembly[] assembliesToScan = null)
    {
        // Create options with connection settings
        var options = new ElasticsearchOptions
        {
            NodeUrls = nodeUrls,
            DefaultIndexPrefix = defaultIndexPrefix
        };

        // Register options
        services.Configure<ElasticsearchOptions>(opt =>
        {
            opt.NodeUrls = options.NodeUrls;
            opt.DefaultIndexPrefix = options.DefaultIndexPrefix;
        });

        // Register Elasticsearch services
        services.AddSingleton<IElasticsearchClientFactory, ElasticsearchClientFactory>();
        services.AddSingleton<IIndexNamingService, IndexNamingService>();

        // Register Elasticsearch client
        services.AddSingleton<IElasticClient>(sp => 
            sp.GetRequiredService<IElasticsearchClientFactory>().GetClient());

        // Scan for repositories if assembliesToScan is provided
        if (assembliesToScan != null)
        {
            ScanAndRegisterRepositories(services, assembliesToScan);
        }

        return services;
    }

    #endregion

    #region Repository Registration

    /// <summary>
    /// Adds a repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <typeparam name="TRepository">The repository type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="indexName">Optional explicit index name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddElasticsearchRepository<TDocument, TRepository>(
        this IServiceCollection services,
        string indexName = null)
        where TDocument : EsBaseDocument
        where TRepository : class, IElasticsearchRepository<TDocument>
    {
        // Register repository with either custom index name or auto-generated name
        if (string.IsNullOrEmpty(indexName))
        {
            services.AddScoped<IElasticsearchRepository<TDocument>, TRepository>(sp =>
            {
                var client = sp.GetRequiredService<IElasticClient>();
                var namingService = sp.GetRequiredService<IIndexNamingService>();
                var logger = sp.GetRequiredService<ILogger<TRepository>>();

                var resolvedName = namingService.GetIndexName<TDocument>();
                return (TRepository)Activator.CreateInstance(
                    typeof(TRepository),
                    client,
                    resolvedName,
                    logger);
            });
        }
        else
        {
            services.AddScoped<IElasticsearchRepository<TDocument>, TRepository>(sp =>
            {
                var client = sp.GetRequiredService<IElasticClient>();
                var logger = sp.GetRequiredService<ILogger<TRepository>>();

                return (TRepository)Activator.CreateInstance(
                    typeof(TRepository),
                    client,
                    indexName,
                    logger);
            });
        }

        // Register repository interface directly
        services.AddScoped<TRepository>(sp => 
            (TRepository)sp.GetRequiredService<IElasticsearchRepository<TDocument>>());

        return services;
    }

    /// <summary>
    /// Adds a default repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="indexName">Optional explicit index name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddElasticsearchRepository<TDocument>(
        this IServiceCollection services,
        string indexName = null)
        where TDocument : EsBaseDocument
    {
        return services.AddElasticsearchRepository<TDocument, ElasticsearchRepository<TDocument>>(indexName);
    }

    #endregion

    #region Time-Based Repository Registration

    /// <summary>
    /// Adds a time-based repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <typeparam name="TRepository">The repository type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="dateFormat">The date format for the index name (default: yyyy.MM)</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTimeBasedElasticsearchRepository<TDocument, TRepository>(
        this IServiceCollection services,
        string dateFormat = "yyyy.MM")
        where TDocument : EsBaseDocument
        where TRepository : class, IElasticsearchRepository<TDocument>
    {
        services.AddScoped<IElasticsearchRepository<TDocument>, TRepository>(sp =>
        {
            var client = sp.GetRequiredService<IElasticClient>();
            var namingService = sp.GetRequiredService<IIndexNamingService>();
            var logger = sp.GetRequiredService<ILogger<TRepository>>();

            var indexName = namingService.GetTimeBasedIndexName<TDocument>(DateTime.UtcNow, dateFormat);
            return (TRepository)Activator.CreateInstance(
                typeof(TRepository),
                client,
                indexName,
                logger);
        });

        // Register repository interface directly
        services.AddScoped<TRepository>(sp => 
            (TRepository)sp.GetRequiredService<IElasticsearchRepository<TDocument>>());

        return services;
    }

    /// <summary>
    /// Adds a default time-based repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="dateFormat">The date format for the index name (default: yyyy.MM)</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTimeBasedElasticsearchRepository<TDocument>(
        this IServiceCollection services,
        string dateFormat = "yyyy.MM")
        where TDocument : EsBaseDocument
    {
        return services.AddTimeBasedElasticsearchRepository<TDocument, ElasticsearchRepository<TDocument>>(dateFormat);
    }

    #endregion

    #region Repository Scanning

    /// <summary>
    /// Scans assemblies for document types and registers repositories for them
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">The assemblies to scan</param>
    private static void ScanAndRegisterRepositories(IServiceCollection services, Assembly[] assemblies)
    {
        if (assemblies == null || assemblies.Length == 0)
        {
            return;
        }

        // Find all document types (classes that inherit from BaseDocument)
        var documentTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && typeof(EsBaseDocument).IsAssignableFrom(t))
            .ToList();

        // Find all custom repository implementations
        var repositoryTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && !t.IsAbstract && IsElasticsearchRepository(t))
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
        // Get the generic AddElasticsearchRepository method
        var method = typeof(ElasticsearchServiceExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(AddElasticsearchRepository) &&
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
        // Get the generic AddElasticsearchRepository method with one type parameter
        var method = typeof(ElasticsearchServiceExtensions)
            .GetMethods(BindingFlags.Public | BindingFlags.Static)
            .First(m => m.Name == nameof(AddElasticsearchRepository) &&
                        m.GetGenericArguments().Length == 1);

        // Create the generic method with the specific document type
        var genericMethod = method.MakeGenericMethod(documentType);

        // Invoke the method to register the repository
        genericMethod.Invoke(null, new object[] { services, null });
    }

    /// <summary>
    /// Checks if a type is an Elasticsearch repository
    /// </summary>
    /// <param name="type">The type to check</param>
    /// <returns>True if the type is an Elasticsearch repository, false otherwise</returns>
    private static bool IsElasticsearchRepository(Type type)
    {
        // Check all interfaces implemented by the type
        foreach (var interfaceType in type.GetInterfaces())
        {
            // Check if the interface is IElasticsearchRepository<T>
            if (interfaceType.IsGenericType &&
                interfaceType.GetGenericTypeDefinition() == typeof(IElasticsearchRepository<>))
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
        // Find the IElasticsearchRepository<T> interface
        var esRepositoryInterface = repositoryType.GetInterfaces()
            .First(i => i.IsGenericType &&
                        i.GetGenericTypeDefinition() == typeof(IElasticsearchRepository<>));

        // Get the document type from the interface
        return esRepositoryInterface.GetGenericArguments()[0];
    }

    #endregion

    #region Validation

    /// <summary>
    /// Validates that all Elasticsearch repositories used in the application are properly registered.
    /// This should be called during service registration, before building the application.
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection for chaining</returns>
    public static IServiceCollection ValidateElasticsearchRepositories(this IServiceCollection services)
    {
        // Get all methods from the entry assembly that might be endpoints
        var entryAssembly = Assembly.GetEntryAssembly();
        var methods = new List<MethodInfo>();
        
        // Scan the assembly containing the Program class (where the app is built)
        if (entryAssembly != null)
        {
            methods.AddRange(entryAssembly.GetTypes()
                .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance))
                .Where(m => m.GetParameters().Any(p => 
                    p.ParameterType.IsGenericType && 
                    p.ParameterType.GetGenericTypeDefinition() == typeof(IElasticsearchRepository<>))));
        }
        
        // Also scan controllers for API endpoints
        methods.AddRange(AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .Where(t => t.Name.EndsWith("Controller"))
            .SelectMany(t => t.GetMethods(BindingFlags.Public | BindingFlags.Instance))
            .Where(m => m.GetParameters().Any(p => 
                p.ParameterType.IsGenericType && 
                p.ParameterType.GetGenericTypeDefinition() == typeof(IElasticsearchRepository<>))));
        
        // Check each method for IElasticsearchRepository<T> parameters
        var missingRepositories = new Dictionary<Type, Dictionary<Type, List<MethodInfo>>>();
        
        foreach (var method in methods)
        {
            var parameters = method.GetParameters();
            foreach (var param in parameters)
            {
                var paramType = param.ParameterType;
                if (paramType.IsGenericType && 
                    paramType.GetGenericTypeDefinition() == typeof(IElasticsearchRepository<>))
                {
                    // Get document type from repository type
                    var documentType = paramType.GetGenericArguments()[0];
                    
                    // Check if the repository is registered
                    var isRegistered = services.Any(sd => sd.ServiceType == paramType);
                    
                    // If not registered, add to missing repositories
                    if (!isRegistered)
                    {
                        if (!missingRepositories.ContainsKey(paramType))
                        {
                            missingRepositories[paramType] = new Dictionary<Type, List<MethodInfo>>();
                        }
                        
                        if (!missingRepositories[paramType].ContainsKey(documentType))
                        {
                            missingRepositories[paramType][documentType] = new List<MethodInfo>();
                        }
                        
                        missingRepositories[paramType][documentType].Add(method);
                    }
                }
            }
        }
        
        // If missing repositories found, throw an exception
        if (missingRepositories.Count > 0)
        {
            var errorMessage = new System.Text.StringBuilder("Missing Elasticsearch repository registrations detected:\n\n");
            
            errorMessage.AppendLine("To fix this, you have two options:\n");
            
            // Option 1: Register specific repositories
            errorMessage.AppendLine("OPTION 1: Register specific repositories:");
            foreach (var entry in missingRepositories)
            {
                foreach (var docEntry in entry.Value)
                {
                    var documentType = docEntry.Key;
                    errorMessage.AppendLine($"  services.AddElasticsearchRepository<{documentType.Name}>();");
                }
            }
            
            // Option 2: Use assembly scanning
            errorMessage.AppendLine("\nOPTION 2: Use assembly scanning (recommended):");
            errorMessage.AppendLine("  builder.Services.AddElasticsearch(");
            errorMessage.AppendLine("      builder.Configuration,");
            errorMessage.AppendLine("      assembliesToScan: new[] { Assembly.GetExecutingAssembly() }");
            errorMessage.AppendLine("  );");
            
            errorMessage.AppendLine("\nDetailed missing repository information:");
            foreach (var entry in missingRepositories)
            {
                foreach (var docEntry in entry.Value)
                {
                    var repoType = entry.Key;
                    var documentType = docEntry.Key;
                    var usedInMethods = docEntry.Value;
                    
                    errorMessage.AppendLine($"* {repoType.Name} for {documentType.Name} is not registered but is used in:");
                    foreach (var method in usedInMethods)
                    {
                        var methodName = method.Name;
                        // Clean up lambda method names for better readability
                        if (methodName.Contains('<') && methodName.Contains('>'))
                        {
                            methodName = methodName.Substring(methodName.IndexOf("<") + 1, 
                                methodName.IndexOf(">") - methodName.IndexOf("<") - 1);
                        }
                        
                        errorMessage.AppendLine($"  - {method.DeclaringType?.Name}.{methodName}");
                    }
                }
            }
            
            throw new InvalidOperationException(errorMessage.ToString());
        }
        
        return services;
    }

    #endregion
}