namespace QFace.Sdk.MongoDb.MultiTenant;

/// <summary>
/// Extension methods for registering multi-tenant services
/// </summary>
public static class MultiTenantExtensions
{
    /// <summary>
    /// Adds multi-tenant services to the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <returns>The service collection</returns>
    public static IServiceCollection AddMongoDbMultiTenancy(this IServiceCollection services)
    {
        // Register tenant accessor
        services.AddSingleton<ITenantAccessor, TenantAccessor>();
            
        // Register tenant repositories
        services.AddMongoRepository<TenantDocument, TenantRepository>("tenants");
        services.AddMongoRepository<TenantUserDocument, TenantUserRepository>("tenant_users");
            
        // Register tenant services
        services.AddScoped<ITenantDatabaseManager, TenantDatabaseManager>();
        services.AddScoped<ITenantService, TenantService>();
            
        return services;
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