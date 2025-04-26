namespace QFace.Sdk.MongoDb.MultiTenant.Services;

/// <summary>
/// Implementation of tenant service
/// </summary>
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantUserRepository _tenantUserRepository;
    private readonly ITenantDatabaseManager _tenantDatabaseManager;
    private readonly ILogger<TenantService> _logger;
        
    /// <summary>
    /// Creates a new tenant service
    /// </summary>
    /// <param name="tenantRepository">The tenant repository</param>
    /// <param name="tenantUserRepository">The tenant user repository</param>
    /// <param name="tenantDatabaseManager">The tenant database manager</param>
    /// <param name="logger">The logger</param>
    public TenantService(
        ITenantRepository tenantRepository,
        ITenantUserRepository tenantUserRepository,
        ITenantDatabaseManager tenantDatabaseManager,
        ILogger<TenantService> logger)
    {
        _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
        _tenantUserRepository = tenantUserRepository ?? throw new ArgumentNullException(nameof(tenantUserRepository));
        _tenantDatabaseManager = tenantDatabaseManager ?? throw new ArgumentNullException(nameof(tenantDatabaseManager));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }
        
    /// <summary>
    /// Gets a tenant by ID
    /// </summary>
    public async Task<TenantDocument?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            return null;
                
        return await _tenantRepository.GetByIdAsync(id, cancellationToken);
    }
        
    /// <summary>
    /// Gets a tenant by code
    /// </summary>
    public async Task<TenantDocument?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
            return null;
                
        return await _tenantRepository.GetByCodeAsync(code, cancellationToken);
    }
        
    /// <summary>
    /// Gets all tenants
    /// </summary>
    public async Task<IEnumerable<TenantDocument>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.GetAllAsync(includeInactive, cancellationToken);
    }
        
    /// <summary>
    /// Creates a new tenant
    /// </summary>
    public async Task<string> CreateAsync(TenantDocument tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));
                
        // Validate and prepare tenant
        ValidateAndPrepareTenant(tenant);
            
        // Check if code is unique
        if (!string.IsNullOrEmpty(tenant.Code) && await _tenantRepository.ExistsByCodeAsync(tenant.Code, cancellationToken))
        {
            throw new InvalidOperationException($"Tenant with code '{tenant.Code}' already exists");
        }
            
        // Insert tenant
        await _tenantRepository.InsertOneAsync(tenant, cancellationToken);
            
        _logger.LogInformation("Created tenant: {TenantId}, Code: {TenantCode}, Name: {TenantName}", 
            tenant.Id, tenant.Code, tenant.Name);
                
        return tenant.Id;
    }
        
    /// <summary>
    /// Updates a tenant
    /// </summary>
    public async Task<bool> UpdateAsync(TenantDocument tenant, CancellationToken cancellationToken = default)
    {
        if (tenant == null)
            throw new ArgumentNullException(nameof(tenant));
                
        // Get existing tenant
        var existing = await _tenantRepository.GetByIdAsync(tenant.Id, cancellationToken);
        if (existing == null)
        {
            _logger.LogWarning("Cannot update non-existent tenant: {TenantId}", tenant.Id);
            return false;
        }
            
        // Check if code is being changed and is unique
        if (!string.IsNullOrEmpty(tenant.Code) && tenant.Code != existing.Code)
        {
            if (await _tenantRepository.ExistsByCodeAsync(tenant.Code, cancellationToken))
            {
                throw new InvalidOperationException($"Tenant with code '{tenant.Code}' already exists");
            }
        }
            
        // Preserve some fields that should not be directly updated
        tenant.IsProvisioned = existing.IsProvisioned;
        tenant.ProvisionedDate = existing.ProvisionedDate;
            
        // Update tenant
        var result = await _tenantRepository.UpdateAsync(tenant, cancellationToken);
            
        if (result)
        {
            _logger.LogInformation("Updated tenant: {TenantId}, Code: {TenantCode}, Name: {TenantName}", 
                tenant.Id, tenant.Code, tenant.Name);
        }
        else
        {
            _logger.LogWarning("Failed to update tenant: {TenantId}", tenant.Id);
        }
            
        return result;
    }
        
    /// <summary>
    /// Deletes a tenant
    /// </summary>
    public async Task<bool> DeleteAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            return false;
                
        // Soft delete the tenant
        var result = await _tenantRepository.SoftDeleteByIdAsync(id, cancellationToken);
            
        if (result)
        {
            _logger.LogInformation("Deleted tenant: {TenantId}", id);
        }
        else
        {
            _logger.LogWarning("Failed to delete tenant: {TenantId}", id);
        }
            
        return result;
    }
        
    /// <summary>
    /// Provisions a tenant database
    /// </summary>
    public async Task<bool> ProvisionAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            return false;
                
        // Get tenant
        var tenant = await _tenantRepository.GetByIdAsync(id, cancellationToken);
        if (tenant == null)
        {
            _logger.LogWarning("Cannot provision non-existent tenant: {TenantId}", id);
            return false;
        }
            
        // Check if already provisioned
        if (tenant.IsProvisioned)
        {
            _logger.LogInformation("Tenant already provisioned: {TenantId}", id);
            return true;
        }
            
        try
        {
            // Provision tenant database
            await _tenantDatabaseManager.ProvisionTenantDatabaseAsync(tenant, cancellationToken);
                
            // Update tenant provisioning status
            await _tenantRepository.UpdateProvisioningStatusAsync(id, true, cancellationToken);
                
            _logger.LogInformation("Provisioned tenant: {TenantId}, Code: {TenantCode}, Name: {TenantName}", 
                tenant.Id, tenant.Code, tenant.Name);
                    
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to provision tenant: {TenantId}", id);
            return false;
        }
    }
        
    /// <summary>
    /// Gets tenants accessible by a user
    /// </summary>
    public async Task<IEnumerable<TenantDocument>> GetAccessibleTenantsAsync(
        string userId, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return Enumerable.Empty<TenantDocument>();
                
        // Get tenant-user associations
        var tenantUsers = await _tenantUserRepository.GetTenantsByUserIdAsync(
            userId, includeInactive, cancellationToken);
                
        if (!tenantUsers.Any())
            return Enumerable.Empty<TenantDocument>();
                
        // Get tenant IDs
        var tenantIds = tenantUsers.Select(tu => tu.TenantId).ToList();
            
        // Get tenants
        var tenants = new List<TenantDocument>();
            
        foreach (var tenantId in tenantIds)
        {
            var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
            if (tenant != null && (includeInactive || tenant.IsActive))
            {
                tenants.Add(tenant);
            }
        }
            
        return tenants;
    }
        
    /// <summary>
    /// Validates tenant access for a user
    /// </summary>
    public async Task<bool> ValidateTenantAccessAsync(
        string userId, 
        string tenantId, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(tenantId))
            return false;
                
        // Check if tenant exists and is active
        var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
        if (tenant == null || !tenant.IsActive || !tenant.IsProvisioned)
            return false;
                
        // Check if user has access to tenant
        return await _tenantUserRepository.HasTenantAccessAsync(userId, tenantId, cancellationToken);
    }
        
    /// <summary>
    /// Validates and prepares a tenant
    /// </summary>
    private void ValidateAndPrepareTenant(TenantDocument tenant)
    {
        // Validate required fields
        if (string.IsNullOrEmpty(tenant.Name))
            throw new ArgumentException("Tenant name is required");
                
        // Generate code if not provided
        if (string.IsNullOrEmpty(tenant.Code))
        {
            tenant.Code = GenerateCode(tenant.Name);
        }
            
        // Set default values
        tenant.IsProvisioned = false;
        tenant.ProvisionedDate = null;
            
        // Generate database name if not provided
        if (string.IsNullOrEmpty(tenant.DatabaseName))
        {
            tenant.DatabaseName = $"tenant_{tenant.Code.ToLowerInvariant()}";
        }
    }
        
    /// <summary>
    /// Generates a code from a name
    /// </summary>
    private string GenerateCode(string name)
    {
        // Create a code from the tenant name (lowercase, no spaces)
        var code = name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
                
        // Remove special characters
        code = new string(code.Where(c => char.IsLetterOrDigit(c) || c == '-').ToArray());
            
        // Ensure it's not too long
        if (code.Length > 50)
        {
            code = code.Substring(0, 50);
        }
            
        // Add a random suffix to ensure uniqueness
        var randomSuffix = Guid.NewGuid().ToString().Substring(0, 8);
        code = $"{code}-{randomSuffix}";
            
        return code;
    }
}