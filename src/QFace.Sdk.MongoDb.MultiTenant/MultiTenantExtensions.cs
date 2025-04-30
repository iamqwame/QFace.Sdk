using System.Reflection;

namespace QFace.Sdk.MongoDb.MultiTenant;

/// <summary>
/// Extension methods for registering multi-tenant services
/// </summary>
public static class MultiTenantExtensions
{
    // Keep track of document types that should be excluded from tenant-awareness
    private static readonly HashSet<Type> ExcludedDocumentTypes = new();

 /// <summary>
    /// Adds multi-tenant services to the service collection with separate tenant management database
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <param name="tenantManagementSectionName">Section name for tenant management database</param>
    /// <param name="tenantDataSectionName">Section name for tenant data database</param>
    /// <param name="assembliesToScan">Assemblies to scan for document types</param>
    /// <returns>The service collection</returns>
 public static IServiceCollection AddMongoDbMultiTenancy(
    this IServiceCollection services,
    IConfiguration configuration,
    string tenantManagementSectionName = "MongoDbManagement",
    string tenantDataSectionName = "MongoDbData",
    Assembly[]? assembliesToScan = null)
{
    // Core infrastructure
    services.AddSingleton<ITenantAccessor, TenantAccessor>();
    services.AddSingleton<ICollectionNamingService, CollectionNamingService>();

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

    // Tenant data database connection
    services.AddMongoDb(
        configuration, 
        tenantDataSectionName,
        assembliesToScan: assembliesToScan
    );

    // Core services
    services.AddScoped<ITenantDatabaseManager, TenantDatabaseManager>();
    services.AddSingleton<IPasswordHasher, PasswordHasher>();
    services.AddSingleton<ITokenService, JwtTokenService>();
    services.AddScoped<ITenantService, TenantService>();
    
    // Auto-registration of tenant-aware repositories
    if (assembliesToScan is { Length: > 0 })
    {
        RegisterAllTenantAwareDocuments(services, assembliesToScan);
    }
        
    return services;
}
    /// <summary>
    /// Registers tenant-aware repositories for all document types that inherit from TenantBaseDocument
    /// </summary>
    private static void RegisterAllTenantAwareDocuments(IServiceCollection services, Assembly[] assemblies)
    {
        // Find all types that inherit from TenantBaseDocument
        var documentTypes = assemblies
            .SelectMany(a => a.GetTypes())
            .Where(t => t.IsClass && 
                        !t.IsAbstract && 
                        t.IsSubclassOf(typeof(TenantBaseDocument)) &&
                        !ExcludedDocumentTypes.Contains(t))
            .ToList();

        // Register tenant-aware repository for each document type
        foreach (var documentType in documentTypes)
        {
            // Use reflection to call the generic method for each document type
            typeof(MultiTenantExtensions)
                .GetMethod(nameof(AddTenantAwareRepository), new[] { typeof(IServiceCollection), typeof(string) })
                ?.MakeGenericMethod(documentType)
                .Invoke(null, new object[] { services, null });
        }
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