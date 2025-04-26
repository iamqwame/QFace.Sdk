namespace QFace.Sdk.MongoDb.MultiTenant.Services;

/// <summary>
/// Implementation of tenant database manager
/// </summary>
public class TenantDatabaseManager : ITenantDatabaseManager
{
    private readonly IMongoDbClientFactory _mongoDbClientFactory;
    private readonly ILogger<TenantDatabaseManager> _logger;
        
    /// <summary>
    /// Creates a new tenant database manager
    /// </summary>
    /// <param name="mongoDbClientFactory">The MongoDB client factory</param>
    /// <param name="logger">The logger</param>
    public TenantDatabaseManager(
        IMongoDbClientFactory mongoDbClientFactory,
        ILogger<TenantDatabaseManager> logger)
    {
        _mongoDbClientFactory = mongoDbClientFactory ?? throw new ArgumentNullException(nameof(mongoDbClientFactory));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
        
    /// <summary>
    /// Provisions a tenant database
    /// </summary>
    public async Task ProvisionTenantDatabaseAsync(TenantDocument tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));
                
        if (tenant.IsProvisioned)
        {
            _logger.LogInformation("Tenant already provisioned: {TenantId}", tenant.Id);
            return;
        }
            
        try
        {
            // Get MongoDB client
            var client = GetTenantMongoClient(tenant);
                
            // Create database
            var database = client.GetDatabase(tenant.DatabaseName);
                
            // Create basic collections
            await CreateRequiredCollections(database, cancellationToken);
                
            _logger.LogInformation("Provisioned tenant database: {TenantId}, DatabaseName: {DatabaseName}", 
                tenant.Id, tenant.DatabaseName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error provisioning tenant database: {TenantId}, DatabaseName: {DatabaseName}", 
                tenant.Id, tenant.DatabaseName);
            throw;
        }
    }
        
    /// <summary>
    /// Deprovisions a tenant database
    /// </summary>
    public async Task DeprovisionTenantDatabaseAsync(TenantDocument tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));
                
        if (!tenant.IsProvisioned)
        {
            _logger.LogInformation("Tenant not provisioned: {TenantId}", tenant.Id);
            return;
        }
            
        try
        {
            // Get MongoDB client
            var client = GetTenantMongoClient(tenant);
                
            // Drop database (only in development or with explicit confirmation)
            if (IsDropDatabaseAllowed(tenant))
            {
                await client.DropDatabaseAsync(tenant.DatabaseName, cancellationToken);
                    
                _logger.LogInformation("Dropped tenant database: {TenantId}, DatabaseName: {DatabaseName}", 
                    tenant.Id, tenant.DatabaseName);
            }
            else
            {
                _logger.LogWarning("Database drop not allowed for tenant: {TenantId}, DatabaseName: {DatabaseName}", 
                    tenant.Id, tenant.DatabaseName);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deprovisioning tenant database: {TenantId}, DatabaseName: {DatabaseName}", 
                tenant.Id, tenant.DatabaseName);
            throw;
        }
    }
        
    /// <summary>
    /// Creates the required collections for a tenant database
    /// </summary>
    private async Task CreateRequiredCollections(IMongoDatabase database, CancellationToken cancellationToken)
    {
        // Create settings collection
        await database.CreateCollectionAsync("settings", null, cancellationToken);
            
        // Create system logs collection
        await database.CreateCollectionAsync("system_logs", null, cancellationToken);
            
        // Create diagnostics collection
        await database.CreateCollectionAsync("diagnostics", null, cancellationToken);
            
        // Create a default document in settings collection
        var settingsCollection = database.GetCollection<object>("settings");
        await settingsCollection.InsertOneAsync(new
        {
            _id = "system_settings",
            createdDate = DateTime.UtcNow,
            initialSetup = true,
            version = "1.0.0"
        }, cancellationToken: cancellationToken);
    }
        
    /// <summary>
    /// Gets a MongoDB client for a tenant
    /// </summary>
    private IMongoClient GetTenantMongoClient(TenantDocument tenant)
    {
        // Use tenant-specific connection string if provided, otherwise use default
        if (!string.IsNullOrEmpty(tenant.ConnectionString))
        {
            var clientSettings = MongoClientSettings.FromConnectionString(tenant.ConnectionString);
            return new MongoClient(clientSettings);
        }
            
        // Use default client
        return _mongoDbClientFactory.GetClient();
    }
        
    /// <summary>
    /// Checks if dropping a database is allowed
    /// </summary>
    private bool IsDropDatabaseAllowed(TenantDocument tenant)
    {
        // In a real system, you might want to check environment, have explicit confirmation,
        // or have a specific flag in tenant settings that allows database dropping
            
        // For this implementation, we'll check for a setting in tenant.Settings
        if (tenant.Settings != null && 
            tenant.Settings.TryGetValue("AllowDatabaseDrop", out var allowDropValue) &&
            bool.TryParse(allowDropValue, out var allowDrop) && 
            allowDrop)
        {
            return true;
        }
            
        // Default to false for safety
        return false;
    }
}