using QFace.Sdk.Extensions.Services;

namespace QFace.Sdk.MongoDb.MultiTenant;

/// <summary>
/// Extension methods for registering multi-tenant services
/// </summary>
public static class MultiTenantExtensions
{
    // Keep track of document types that should be excluded from tenant-awareness
    private static readonly HashSet<Type> ExcludedDocumentTypes = new();
    
    // Keep track of document types that should be excluded from collection creation
    private static readonly HashSet<Type> ExcludedCollectionTypes = new();

    /// <summary>
    /// Adds core multi-tenant services (without database connections)
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMultiTenancyCore(this IServiceCollection services)
    {
        // Core infrastructure
        services.AddSingleton<ITenantAccessor, TenantAccessor>();
        services.AddSingleton<ICollectionNamingService, CollectionNamingService>();
        
        // Core services
        services.AddScoped<ITenantDatabaseManager, TenantDatabaseManager>();
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddScoped<ITenantService, TenantService>();
        
        return services;
    }

    /// <summary>
    /// Adds tenant management database and related repositories
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="tenantManagementSectionName">Section name for tenant management database</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTenantManagementDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string tenantManagementSectionName = "MongoDbManagement")
    {
        // Tenant management repositories
        services.AddMongoRepositoryWithConnection<Tenant, TenantRepository>(
            configuration,
            tenantManagementSectionName,
            "tenants");
            
        services.AddMongoRepositoryWithConnection<TenantUser, TenantUserRepository>(
            configuration,
            tenantManagementSectionName,
            "tenant_users");
            
        // Authentication repositories (same database as tenant management)
        services.AddMongoRepositoryWithConnection<User, UserRepository>(
            configuration,
            tenantManagementSectionName,
            "users");
            
        services.AddMongoRepositoryWithConnection<RefreshToken, RefreshTokenRepository>(
            configuration,
            tenantManagementSectionName,
            "refresh_tokens");
        
        // Repository interface bindings
        services.AddScoped<ITenantRepository>(sp => sp.GetRequiredService<TenantRepository>());
        services.AddScoped<ITenantUserRepository>(sp => sp.GetRequiredService<TenantUserRepository>());
        services.AddScoped<IUserRepository>(sp => sp.GetRequiredService<UserRepository>());
        services.AddScoped<IRefreshTokenRepository>(sp => sp.GetRequiredService<RefreshTokenRepository>());

        return services;
    }

    /// <summary>
    /// Adds tenant data database and registers tenant-aware repositories
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="tenantDataSectionName">Section name for tenant data database</param>
    /// <param name="assembliesToScan">Assemblies to scan for document types</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTenantDataDb(
        this IServiceCollection services,
        IConfiguration configuration,
        string tenantDataSectionName = "MongoDbData",
        Assembly[]? assembliesToScan = null)
    {
        // Tenant data database connection
        services.AddMongoDb(
            configuration, 
            tenantDataSectionName,
            assembliesToScan: assembliesToScan
        );

        // Auto-registration of tenant-aware repositories
        if (assembliesToScan is { Length: > 0 })
        {
            RegisterAllTenantAwareDocuments(services, assembliesToScan);
        }
            
        return services;
    }

    /// <summary>
    /// Adds multi-tenant services with separate tenant management and tenant data databases
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="tenantManagementSectionName">Section name for tenant management database</param>
    /// <param name="tenantDataSectionName">Section name for tenant data database</param>
    /// <param name="assembliesToScan">Assemblies to scan for document types</param>
    /// <param name="useAttributeFiltering">Whether to use TenantCollectionAttribute for filtering</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoDbMultiTenancy(
        this IServiceCollection services,
        IConfiguration configuration,
        string tenantManagementSectionName = "MongoDbManagement",
        string tenantDataSectionName = "DefaultTenantDbData",
        Assembly[]? assembliesToScan = null,
        bool useAttributeFiltering = false)
    {
        // Add core services
        services.AddMultiTenancyCore();

        // Add tenant management database
        services.AddTenantManagementDb(configuration, tenantManagementSectionName);

        // Add tenant data database
        services.AddTenantDataDb(
            configuration, 
            tenantDataSectionName, 
            assembliesToScan
        );
        
        return services;
    }

    /// <summary>
    /// Registers tenant-aware repositories for all document types with TenantCollectionAttribute
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="assemblies">Assemblies to scan</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTenantAwareRepositoriesFromAttributes(
        this IServiceCollection services,
        Assembly[]? assemblies = null)
    {
        // If no assemblies specified, scan all loaded assemblies
        if (assemblies == null || assemblies.Length == 0)
        {
            assemblies = AppDomain.CurrentDomain.GetAssemblies();
        }
        
        // Register tenant-aware repositories for all types with TenantCollectionAttribute
        RegisterAllTenantAwareDocuments(services, assemblies, true);
        
        return services;
    }
    
    /// <summary>
    /// Registers tenant-aware repositories for all document types that inherit from TenantBaseDocument
    /// </summary>
    private static void RegisterAllTenantAwareDocuments(
        IServiceCollection services, 
        Assembly[] assemblies,
        bool useAttributeFiltering = true)
    {
        var logger = services.BuildServiceProvider().GetService<ILogger<object>>();
        
        // Find all types that inherit from TenantBaseDocument
        var query = assemblies
            .SelectMany(a => 
            {
                try { return a.GetTypes(); }
                catch { return Array.Empty<Type>(); }
            })
            .Where(t => t.IsClass && 
                       !t.IsAbstract && 
                       t.IsSubclassOf(typeof(TenantBaseDocument)) &&
                       !ExcludedDocumentTypes.Contains(t));
        
        // If attribute filtering is enabled, only include types with TenantCollectionAttribute
        if (useAttributeFiltering)
        {
            query = query.Where(t => t.GetCustomAttribute<TenantCollectionAttribute>() != null);
        }

        var documentTypes = query.ToList();
        logger?.LogInformation("Found {Count} document types for tenant-aware registration", documentTypes.Count);
        
        // Register each document type using the type-safe registrar
        foreach (var docType in documentTypes)
        {
            try
            {
                // Get collection name from attribute if available
                string? collectionName = null;
                if (useAttributeFiltering)
                {
                    var collectionAttr = docType.GetCustomAttribute<TenantCollectionAttribute>();
                    collectionName = collectionAttr?.CollectionName;
                }
                
                // Use a delegate to register the specific type using the generic method
                // This avoids ambiguity by using the C# generic type system directly
                RegisterRepositoryForType(services, docType, collectionName);
                
                logger?.LogInformation("Registered tenant-aware repository for {DocumentType}", docType.Name);
            }
            catch (Exception ex)
            {
                logger?.LogError(ex, "Failed to register tenant-aware repository for {DocumentType}", docType.Name);
            }
        }
    }
    
    /// <summary>
    /// Type-safe generic registration delegate to avoid reflection ambiguity
    /// </summary>
    private static void RegisterRepositoryForType(IServiceCollection services, Type documentType, string? collectionName)
    {
        // Create an open generic method on the registrar first
        var openMethod = typeof(TenantRepositoryRegistrar)
            .GetMethod(nameof(TenantRepositoryRegistrar.RegisterRepositoryFor));
            
        if (openMethod == null)
            throw new InvalidOperationException("RegisterRepositoryFor method not found on TenantRepositoryRegistrar");
            
        // Create a closed generic method with the specific document type
        var closedMethod = openMethod.MakeGenericMethod(documentType);
            
        // Invoke the method with the services and collection name
        closedMethod.Invoke(null, new object?[] { services, collectionName });
    }

    /// <summary>
    /// Excludes a document type from automatic tenant-awareness
    /// Must be called before AddMongoDbMultiTenancy
    /// </summary>
    /// <typeparam name="TDocument">The document type to exclude</typeparam>
    /// <returns>True if the document was excluded, false if it was already excluded</returns>
    public static bool ExcludeFromTenantAwareness<TDocument>() 
        where TDocument : TenantBaseDocument
    {
        return ExcludedDocumentTypes.Add(typeof(TDocument));
    }

    /// <summary>
    /// Excludes multiple document types from automatic tenant-awareness
    /// Must be called before AddMongoDbMultiTenancy
    /// </summary>
    /// <param name="documentTypes">The document types to exclude</param>
    public static void ExcludeFromTenantAwareness(params Type[] documentTypes)
    {
        foreach (var type in documentTypes)
        {
            if (type.IsSubclassOf(typeof(TenantBaseDocument)))
            {
                ExcludedDocumentTypes.Add(type);
            }
        }
    }

    /// <summary>
    /// Excludes a document type from automatic collection creation during tenant provisioning
    /// Must be called before provisioning tenants
    /// </summary>
    /// <typeparam name="TDocument">The document type to exclude</typeparam>
    /// <returns>True if the document was excluded, false if it was already excluded</returns>
    public static bool ExcludeFromCollectionCreation<TDocument>() 
        where TDocument : TenantBaseDocument
    {
        return ExcludedCollectionTypes.Add(typeof(TDocument));
    }

    /// <summary>
    /// Excludes multiple document types from automatic collection creation during tenant provisioning
    /// Must be called before provisioning tenants
    /// </summary>
    /// <param name="documentTypes">The document types to exclude</param>
    public static void ExcludeFromCollectionCreation(params Type[] documentTypes)
    {
        foreach (var type in documentTypes)
        {
            if (type.IsSubclassOf(typeof(TenantBaseDocument)) || type == typeof(TenantBaseDocument))
            {
                ExcludedCollectionTypes.Add(type);
            }
        }
    }

    /// <summary>
    /// Checks if a document type should be excluded from collection creation
    /// </summary>
    /// <param name="documentType">The document type to check</param>
    /// <returns>True if the document type should be excluded, false otherwise</returns>
    public static bool ShouldExcludeFromCollectionCreation(Type documentType)
    {
        return ExcludedCollectionTypes.Contains(documentType);
    }

    /// <summary>
    /// Adds a tenant-aware repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTenantAwareRepository<TDocument>(
        this IServiceCollection services, 
        string? collectionName = null)
        where TDocument : TenantBaseDocument
    {
        // Register standard repository
        services.AddMongoRepository<TDocument>(collectionName);
            
        // Decorate with tenant-aware repository
        services.AddScoped<IMongoRepository<TDocument>>(sp =>
        {
            // Get the base repository
            var baseRepository = sp.GetRequiredService<MongoRepository<TDocument>>();
                
            // Get tenant accessor
            var tenantAccessor = sp.GetRequiredService<ITenantAccessor>();
                
            // Get logger
            var logger = sp.GetRequiredService<ILogger<TenantAwareRepository<TDocument>>>();
                
            // Create tenant-aware repository
            return new TenantAwareRepository<TDocument>(baseRepository, tenantAccessor, logger);
        });
            
        return services;
    }
        
    /// <summary>
    /// Adds a tenant-aware repository with custom implementation
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <typeparam name="TRepository">The repository type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddTenantAwareRepository<TDocument, TRepository>(
        this IServiceCollection services, 
        string? collectionName = null)
        where TDocument : TenantBaseDocument
        where TRepository : class, IMongoRepository<TDocument>
    {
        // Register custom repository
        services.AddMongoRepository<TDocument, TRepository>(collectionName);
            
        // Decorate with tenant-aware repository
        services.AddScoped<IMongoRepository<TDocument>>(sp =>
        {
            // Get the base repository
            var baseRepository = sp.GetRequiredService<TRepository>();
                
            // Get tenant accessor
            var tenantAccessor = sp.GetRequiredService<ITenantAccessor>();
                
            // Get logger
            var logger = sp.GetRequiredService<ILogger<TenantAwareRepository<TDocument>>>();
                
            // Create tenant-aware repository
            return new TenantAwareRepository<TDocument>(baseRepository, tenantAccessor, logger);
        });
            
        // Register direct access to the repository type (unwrapped)
        services.AddScoped<TRepository>();
            
        return services;
    }

    /// <summary>
    /// Adds a non-tenant-aware repository for a document type
    /// </summary>
    /// <typeparam name="TDocument">The document type</typeparam>
    /// <param name="services">The service collection</param>
    /// <param name="collectionName">Optional explicit collection name</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddNonTenantAwareRepository<TDocument>(
        this IServiceCollection services,
        string? collectionName = null)
        where TDocument : TenantBaseDocument
    {
        // Register standard repository without tenant-aware decorator
        services.AddMongoRepository<TDocument>(collectionName);
        return services;
    }
        
    /// <summary>
    /// Adds default tenant resolution middleware
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseTenantResolution(this IApplicationBuilder app)
    {
        return app.UseMiddleware<TenantResolutionMiddleware>(new TenantResolutionOptions());
    }
        
    /// <summary>
    /// Adds tenant resolution middleware with custom options
    /// </summary>
    /// <param name="app">The application builder</param>
    /// <param name="configure">Action to configure options</param>
    /// <returns>The application builder</returns>
    public static IApplicationBuilder UseTenantResolution(
        this IApplicationBuilder app, 
        Action<TenantResolutionOptions> configure)
    {
        var options = new TenantResolutionOptions();
        configure(options);
            
        return app.UseMiddleware<TenantResolutionMiddleware>(options);
    }
}