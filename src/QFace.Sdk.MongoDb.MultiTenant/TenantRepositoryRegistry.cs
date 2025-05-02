namespace QFace.Sdk.MongoDb.MultiTenant;

/// <summary>
/// Helper class to register tenant repositories without using reflection
/// </summary>
public class TenantRepositoryRegistrar
{
    // Static register method to avoid creating instances
    public static void RegisterRepositoryFor<TDocument>(
        IServiceCollection services, 
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
    }
}