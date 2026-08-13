namespace QFace.Sdk.MongoDb.MultiTenant.Services;

/// <summary>
/// Implementation of tenant database manager
/// </summary>
public class TenantDatabaseManager : ITenantDatabaseManager
{
    private readonly IMongoDbClientFactory _mongoDbClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ICollectionNamingService _collectionNamingService;
    private readonly ILogger<TenantDatabaseManager> _logger;
    
    public TenantDatabaseManager(
        IMongoDbClientFactory mongoDbClientFactory,
        IConfiguration configuration,
        ICollectionNamingService collectionNamingService,
        ILogger<TenantDatabaseManager> logger)
    {
        _mongoDbClientFactory = mongoDbClientFactory ?? throw new ArgumentNullException(nameof(mongoDbClientFactory));
        _configuration = configuration;
        _collectionNamingService = collectionNamingService ?? throw new ArgumentNullException(nameof(collectionNamingService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
        
    /// <summary>
    /// Provisions a tenant database
    /// </summary>
    public async Task ProvisionTenantDatabaseAsync(Tenant tenant, CancellationToken cancellationToken = default)
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
            if (tenant.TenantType == TenantType.Shared)
            {
                 ProvisionSharedTenantAsync(tenant, cancellationToken);
            }
            else
            {
                await ProvisionDedicatedTenantAsync(tenant, cancellationToken);
            }
                
            _logger.LogInformation("Provisioned tenant: {TenantId}, DatabaseName: {DatabaseName}, Type: {TenantType}", 
                tenant.Id, tenant.DatabaseName, tenant.TenantType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error provisioning tenant database: {TenantId}, DatabaseName: {DatabaseName}, Type: {TenantType}", 
                tenant.Id, tenant.DatabaseName, tenant.TenantType);
            throw;
        }
    }
    
    private async Task ProvisionDedicatedTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var client = GetTenantMongoClient(tenant);
        
        var database = client.GetDatabase(tenant.DatabaseName);
        
        await CreateRequiredCollections(database, tenant, cancellationToken);
    }
    
    private void ProvisionSharedTenantAsync(Tenant tenant, CancellationToken cancellationToken)
    {
        var sharedDbSection = _configuration.GetSection("DefaultTenantDbData");
        var sharedDbName = sharedDbSection["DatabaseName"] ?? "shared_erp_core";
        
        tenant.DatabaseName = sharedDbName;
        
        _logger.LogInformation("Shared tenant {TenantId} provisioned to use database: {DatabaseName}", 
            tenant.Id, sharedDbName);
    }
        
    public async Task DeprovisionTenantDatabaseAsync(Tenant tenant, CancellationToken cancellationToken = default)
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
            var client = GetTenantMongoClient(tenant);
                
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
    
    
    private async Task CreateRequiredCollections(IMongoDatabase database, Tenant tenant, CancellationToken cancellationToken)
    {
        await database.CreateCollectionAsync("settings", null, cancellationToken);
        await database.CreateCollectionAsync("system_logs", null, cancellationToken);
        await database.CreateCollectionAsync("diagnostics", null, cancellationToken);
        
        var settingsCollection = database.GetCollection<object>("settings");
        await settingsCollection.InsertOneAsync(new
        {
            _id = "system_settings",
            createdDate = DateTime.UtcNow,
            initialSetup = true,
            version = "1.0.0"
        }, cancellationToken: cancellationToken);

        if (tenant.TenantType == TenantType.Dedicated)
        {
            _logger.LogInformation("Creating entity collections for dedicated tenant database: {TenantId}", tenant.Id);
            
            // Find all types inheriting from TenantBaseDocument using the same approach as MongoDbServiceExtensions
            var documentTypes = FindTenantDocumentTypes();
            
            foreach (var docType in documentTypes)
            {
                // Use the already available collection naming service
                var collectionName = _collectionNamingService.GetCollectionName(docType.Name);
                
                try 
                {
                    _logger.LogInformation("Creating collection '{CollectionName}' for type {TypeName}", 
                        collectionName, docType.Name);
                    
                    await database.CreateCollectionAsync(collectionName, null, cancellationToken);
                }
                catch (MongoCommandException ex) when (ex.Message.Contains("already exists"))
                {
                    _logger.LogDebug("Collection '{CollectionName}' already exists", collectionName);
                }
                catch (Exception ex)
                {
                    // Log other errors but continue - we don't want one collection failure to stop everything
                    _logger.LogWarning(ex, "Error creating collection '{CollectionName}'", collectionName);
                }
            }
        }
    }
    
    private IEnumerable<Type> FindTenantDocumentTypes()
    {
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
    
        var documentTypes = assemblies
            .SelectMany(a => 
            {
                try
                {
                    return a.GetTypes();
                }
                catch
                {
                    return Array.Empty<Type>();
                }
            })
            .Where(t => t.IsClass && 
                        !t.IsAbstract && 
                        typeof(TenantBaseDocument).IsAssignableFrom(t) &&
                        t.GetCustomAttribute<TenantCollectionAttribute>() != null &&
                        !MultiTenantExtensions.ShouldExcludeFromCollectionCreation(t))
            .ToList();
    
        _logger.LogInformation("Found {Count} document types marked for tenant collection creation", documentTypes.Count);
    
        return documentTypes;
    }
    
    
    private IMongoClient GetTenantMongoClient(Tenant tenant)
    {
        if (!string.IsNullOrEmpty(tenant.ConnectionString))
        {
            var clientSettings = MongoClientSettings.FromConnectionString(tenant.ConnectionString);
            return new MongoClient(clientSettings);
        }
            
        return _mongoDbClientFactory.GetClient();
    }
        
    private bool IsDropDatabaseAllowed(Tenant tenant)
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