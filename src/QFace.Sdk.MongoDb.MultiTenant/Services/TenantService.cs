using QFace.Sdk.Extensions.Services;
using QFace.Sdk.MongoDb.MultiTenant.Dtos;

namespace QFace.Sdk.MongoDb.MultiTenant.Services;

/// <summary>
/// Implementation of tenant service
/// </summary>
public class TenantService : ITenantService
{
    private readonly ITenantRepository _tenantRepository;
        private readonly ITenantUserRepository _tenantUserRepository;
        private readonly ITenantDatabaseManager _tenantDatabaseManager;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly ITokenService _tokenService;
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITenantAccessor _tenantAccessor;
        private readonly ILogger<TenantService> _logger;
        

        public TenantService(
            ITenantRepository tenantRepository,
            ITenantUserRepository tenantUserRepository,
            ITenantDatabaseManager tenantDatabaseManager,
            IUserRepository userRepository,
            IRefreshTokenRepository refreshTokenRepository,
            ITokenService tokenService,
            IPasswordHasher passwordHasher,
            ITenantAccessor tenantAccessor,
            ILogger<TenantService> logger)
        {
            _tenantRepository = tenantRepository ?? throw new ArgumentNullException(nameof(tenantRepository));
            _tenantUserRepository = tenantUserRepository ?? throw new ArgumentNullException(nameof(tenantUserRepository));
            _tenantDatabaseManager = tenantDatabaseManager ?? throw new ArgumentNullException(nameof(tenantDatabaseManager));
            _userRepository = userRepository ?? throw new ArgumentNullException(nameof(userRepository));
            _refreshTokenRepository = refreshTokenRepository ?? throw new ArgumentNullException(nameof(refreshTokenRepository));
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
            _passwordHasher = passwordHasher ?? throw new ArgumentNullException(nameof(passwordHasher));
            _tenantAccessor = tenantAccessor ?? throw new ArgumentNullException(nameof(tenantAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        
    /// <summary>
    /// Gets a tenant by ID
    /// </summary>
    public async Task<Tenant?> GetByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(id))
            return null;
                
        return await _tenantRepository.GetByIdAsync(id, cancellationToken);
    }
        
    /// <summary>
    /// Gets a tenant by code
    /// </summary>
    public async Task<Tenant?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(code))
            return null;
                
        return await _tenantRepository.GetByCodeAsync(code, cancellationToken);
    }
        
    /// <summary>
    /// Gets all tenants
    /// </summary>
    public async Task<IEnumerable<Tenant>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default)
    {
        return await _tenantRepository.GetAllAsync(includeInactive, cancellationToken);
    }
        
    /// <summary>
    /// Creates a new tenant
    /// </summary>
    public async Task<string> CreateAsync(Tenant tenant, CancellationToken cancellationToken = default)
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
    /// Creates a new tenant with admin user and optional provisioning
    /// </summary>
    /// <param name="request">The tenant creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Result of the creation operation</returns>
    public async Task<TenantCreationResult> CreateTenantAsync(
        TenantCreationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));
            
        if (string.IsNullOrEmpty(request.TenantInfo.Name))
            return TenantCreationResult.Failed("Tenant name is required");
            
        if (string.IsNullOrEmpty(request.AdminInfo.Email))
            return TenantCreationResult.Failed("Admin email is required");
            
        if (string.IsNullOrEmpty(request.AdminInfo.Password))
            return TenantCreationResult.Failed("Admin password is required");
            
        try
        {
            // Begin transaction (if MongoDB transactions are supported)
            // For MongoDB 4.0+, you can use transactions for atomicity
            // For simplicity, we'll handle errors manually without transactions
            
            // 1. Check if tenant code already exists
            var tenantCode = request.TenantInfo.Code ?? GenerateCode(request.TenantInfo.Name);
            
            if (await _tenantRepository.ExistsByCodeAsync(tenantCode, cancellationToken))
            {
                return TenantCreationResult.Failed($"Tenant with code '{tenantCode}' already exists");
            }
            
            // 2. Check if admin email already exists
            if (await _userRepository.EmailExistsAsync(request.AdminInfo.Email, cancellationToken: cancellationToken))
            {
                return TenantCreationResult.Failed($"User with email '{request.AdminInfo.Email}' already exists");
            }
            
            // 3. Create tenant object
            var tenant = new Tenant
            {
                Code = tenantCode,
                Name = request.TenantInfo.Name,
                Description = request.TenantInfo.Description,
                TenantType = request.TenantInfo.TenantType,
                IsProvisioned = false,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            
            // Set contact info if provided
            if (request.TenantInfo.Contact != null)
            {
                tenant.Contact = request.TenantInfo.Contact;
            }
            else
            {
                // Create default contact info from admin
                tenant.Contact = new ContactInfo
                {
                    AdminName = request.AdminInfo.FullName,
                    AdminEmail = request.AdminInfo.Email
                };
            }
            
            // Set subscription info if provided
            if (request.TenantInfo.Subscription != null)
            {
                tenant.Subscription = request.TenantInfo.Subscription;
            }
            
            // Set configuration if provided
            if (request.Configuration != null)
            {
                // Set settings
                foreach (var setting in request.Configuration.Settings)
                {
                    tenant.Settings[setting.Key] = setting.Value;
                }
                
                // Set feature flags
                foreach (var flag in request.Configuration.FeatureFlags)
                {
                    tenant.FeatureFlags[flag.Key] = flag.Value;
                }
                
                // Set custom database info if provided
                if (!string.IsNullOrEmpty(request.Configuration.CustomDatabaseName))
                {
                    tenant.DatabaseName = request.Configuration.CustomDatabaseName;
                }
                else
                {
                    tenant.DatabaseName = $"tenant_{tenantCode.ToLowerInvariant()}";
                }
                
                if (!string.IsNullOrEmpty(request.Configuration.CustomConnectionString))
                {
                    tenant.ConnectionString = request.Configuration.CustomConnectionString;
                }
            }
            else
            {
                // Set default database name
                tenant.DatabaseName = $"tenant_{tenantCode.ToLowerInvariant()}";
            }
            
            // 4. Create the tenant in the database
            await _tenantRepository.InsertOneAsync(tenant, cancellationToken);
            _logger.LogInformation("Created tenant: {TenantId}, Code: {TenantCode}, Name: {TenantName}", 
                tenant.Id, tenant.Code, tenant.Name);
                
            // 5. Create admin user
            var username = request.AdminInfo.Username ?? request.AdminInfo.Email;
            var (passwordHash, passwordSalt) = _passwordHasher.HashPassword(request.AdminInfo.Password);
            
            var user = new User
            {
                Email = request.AdminInfo.Email.ToLowerInvariant(),
                Username = username.ToLowerInvariant(),
                FullName = request.AdminInfo.FullName,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsEmailVerified = true, // Auto-verify admin email
                IsActive = true,
                IsSystemAdmin = tenant.TenantType == TenantType.System, // System admins for system tenants
                LastPasswordChangeDate = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow
            };
            
            await _userRepository.InsertOneAsync(user, cancellationToken);
            _logger.LogInformation("Created admin user: {UserId}, Email: {Email} for tenant: {TenantId}", 
                user.Id, user.Email, tenant.Id);
                
            // 6. Associate user with tenant as admin
            var tenantUser = new TenantUser
            {
                UserId = user.Id,
                TenantId = tenant.Id,
                Role = "TenantAdmin",
                Permissions = GetDefaultAdminPermissions(),
                AddedDate = DateTime.UtcNow,
                AddedBy = "System",
                IsPrimaryTenant = true,
                IsActive = true,
                CreatedDate = DateTime.UtcNow
            };
            
            await _tenantUserRepository.InsertOneAsync(tenantUser, cancellationToken);
            _logger.LogInformation("Associated user {UserId} with tenant {TenantId} as admin", user.Id, tenant.Id);
            
            // 7. Provision the tenant database if requested
            bool isProvisioned = false;
            if (request.Configuration?.ProvisionImmediately == true)
            {
                try
                {
                    await _tenantDatabaseManager.ProvisionTenantDatabaseAsync(tenant, cancellationToken);
                    await _tenantRepository.UpdateProvisioningStatusAsync(tenant.Id, true, cancellationToken);
                    isProvisioned = true;
                    _logger.LogInformation("Provisioned database for tenant: {TenantId}", tenant.Id);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to provision database for tenant: {TenantId}", tenant.Id);
                    // Continue execution - we've created the tenant and admin user,
                    // but provisioning failed. This can be retried later.
                }
            }
            
            // 8. Send welcome email if requested
            if (request.AdminInfo.SendWelcomeEmail)
            {
                try
                {
                    await SendWelcomeEmailAsync(tenant, user, request.AdminInfo.Password, cancellationToken);
                    _logger.LogInformation("Sent welcome email to admin: {Email}", user.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to send welcome email to admin: {Email}", user.Email);
                    // Continue execution - we've created everything, but email sending failed
                }
            }
            
            // 9. Return success result
            return new TenantCreationResult
            {
                Success = true,
                TenantId = tenant.Id,
                TenantCode = tenant.Code,
                TenantName = tenant.Name,
                AdminUserId = user.Id,
                AdminEmail = user.Email,
                IsProvisioned = isProvisioned
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating tenant: {TenantName}", request.TenantInfo.Name);
            return TenantCreationResult.Failed($"An error occurred while creating the tenant: {ex.Message}");
        }
    }
    
    
    /// <summary>
    /// Updates a tenant
    /// </summary>
    public async Task<bool> UpdateAsync(Tenant tenant, CancellationToken cancellationToken = default)
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
    public async Task<IEnumerable<Tenant>> GetAccessibleTenantsAsync(
        string userId, 
        bool includeInactive = false, 
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(userId))
            return Enumerable.Empty<Tenant>();
                
        // Get tenant-user associations
        var tenantUsers = await _tenantUserRepository.GetTenantsByUserIdAsync(
            userId, includeInactive, cancellationToken);
                
        if (!tenantUsers.Any())
            return Enumerable.Empty<Tenant>();
                
        // Get tenant IDs
        var tenantIds = tenantUsers.Select(tu => tu.TenantId).ToList();
            
        // Get tenants
        var tenants = new List<Tenant>();
            
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
        if (tenant is not { IsActive: true } || !tenant.IsProvisioned)
            return false;
                
        // Check if user has access to tenant
        return await _tenantUserRepository.HasTenantAccessAsync(userId, tenantId, cancellationToken);
    }
    
    
        /// <summary>
        /// Authenticates a user for a tenant and issues tokens
        /// </summary>
        /// <param name="tenantCode">The tenant code</param>
        /// <param name="usernameOrEmail">Username or email</param>
        /// <param name="password">User password</param>
        /// <param name="clientInfo">Optional client information</param>
        /// <param name="ipAddress">Optional IP address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication result with tokens if successful</returns>
        public async Task<AuthResult> AuthenticateAsync(
            string tenantCode,
            string usernameOrEmail,
            string password,
            string clientInfo = "",
            string ipAddress = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate parameters
                if (string.IsNullOrEmpty(tenantCode) || string.IsNullOrEmpty(usernameOrEmail) || string.IsNullOrEmpty(password))
                {
                    return AuthResult.Failed("Missing required authentication parameters");
                }
                
                // Get tenant by code
                var tenant = await _tenantRepository.GetByCodeAsync(tenantCode, cancellationToken);
                if (tenant is not { IsActive: true })
                {
                    _logger.LogWarning("Authentication failed: Tenant {TenantCode} not found or inactive", tenantCode);
                    return AuthResult.Failed("Invalid tenant");
                }
                
                if (!tenant.IsProvisioned)
                {
                    _logger.LogWarning("Authentication failed: Tenant {TenantCode} not provisioned", tenantCode);
                    return AuthResult.Failed("Tenant is not provisioned");
                }
                
                // Find user by username or email
                User? user;
                
                // Check if input is an email
                if (usernameOrEmail.Contains('@'))
                {
                    user = await _userRepository.GetByEmailAsync(usernameOrEmail, cancellationToken);
                }
                else
                {
                    user = await _userRepository.GetByUsernameAsync(usernameOrEmail, cancellationToken);
                }
                
                if (user is not { IsActive: true })
                {
                    _logger.LogWarning("Authentication failed: User {Username} not found or inactive", usernameOrEmail);
                    return AuthResult.Failed("Invalid username or password");
                }
                
                // Check password
                if (!_passwordHasher.VerifyPassword(password, user.PasswordHash, user.PasswordSalt))
                {
                    _logger.LogWarning("Authentication failed: Invalid password for user {UserId}", user.Id);
                    return AuthResult.Failed("Invalid username or password");
                }
                
                // Check tenant access
                var tenantUser = await _tenantUserRepository.GetTenantUserAsync(user.Id, tenant.Id, cancellationToken);
                if (tenantUser is not { IsActive: true })
                {
                    _logger.LogWarning("Authentication failed: User {UserId} does not have access to tenant {TenantId}", user.Id, tenant.Id);
                    return AuthResult.Failed("User does not have access to this tenant");
                }
                
                // Check if tenant user account is locked
                if (tenantUser.IsCurrentlyLocked())
                {
                    _logger.LogWarning("Authentication failed: User {UserId} account is locked for tenant {TenantId}", user.Id, tenant.Id);
                    return AuthResult.Failed("Account is temporarily locked. Please try again later");
                }
                
                // Check if access has expired
                if (tenantUser.HasAccessExpired())
                {
                    _logger.LogWarning("Authentication failed: User {UserId} access to tenant {TenantId} has expired", user.Id, tenant.Id);
                    return AuthResult.Failed("Your access to this tenant has expired");
                }
                
                // Record successful login
                tenantUser.RecordSuccessfulLogin();
                await _tenantUserRepository.UpdateAsync(tenantUser, cancellationToken);
                
                // Generate tokens
                var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(
                    user.Id,
                    tenant.Id,
                    tenant.Code,
                    user.Email,
                    tenantUser.Role,
                    tenantUser.Permissions);
                    
                var (refreshToken, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();
                
                // Store refresh token
                await StoreRefreshToken(
                    tenant.Id,
                    user.Id,
                    refreshToken,
                    refreshTokenExpiresAt,
                    clientInfo,
                    ipAddress,
                    cancellationToken);
                
                // Get available tenants for the user
                var availableTenants = await GetAvailableTenantsForUserAsync(user.Id, cancellationToken);
                
                _logger.LogInformation("User {UserId} authenticated successfully for tenant {TenantId}", user.Id, tenant.Id);
                
                return new AuthResult
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    TenantId = tenant.Id,
                    TenantCode = tenant.Code,
                    TenantName = tenant.Name,
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshToken = refreshToken,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaVerified = !tenantUser.RequiresMfa,
                    AvailableTenants = availableTenants
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during authentication for user {Username} in tenant {TenantCode}", usernameOrEmail, tenantCode);
                return AuthResult.Failed("An unexpected error occurred during authentication");
            }
        }
        
        /// <summary>
        /// Refreshes an access token using a refresh token
        /// </summary>
        /// <param name="refreshToken">The refresh token</param>
        /// <param name="clientInfo">Optional client information</param>
        /// <param name="ipAddress">Optional IP address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>New authentication result if successful</returns>
        public async Task<AuthResult> RefreshTokenAsync(
            string refreshToken,
            string clientInfo = "",
            string ipAddress = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Validate token
                if (string.IsNullOrEmpty(refreshToken))
                {
                    return AuthResult.Failed("Refresh token is required");
                }
                
                // Get refresh token from repository
                var token = await _refreshTokenRepository.GetByTokenAsync(refreshToken, cancellationToken);
                if (token == null)
                {
                    _logger.LogWarning("Token refresh failed: Token not found");
                    return AuthResult.Failed("Invalid refresh token");
                }
                
                // Check if token is revoked or expired
                if (token.IsRevoked)
                {
                    _logger.LogWarning("Token refresh failed: Token has been revoked for user {UserId}", token.UserId);
                    return AuthResult.Failed("Refresh token has been revoked");
                }
                
                if (token.ExpiresAt <= DateTime.UtcNow)
                {
                    _logger.LogWarning("Token refresh failed: Token expired for user {UserId}", token.UserId);
                    await _refreshTokenRepository.RevokeTokenAsync(refreshToken, "Token expired", null, cancellationToken);
                    return AuthResult.Failed("Refresh token has expired");
                }
                
                // Get tenant and user
                var tenant = await _tenantRepository.GetByIdAsync(token.TenantId, cancellationToken);
                if (tenant is not { IsActive: true } || !tenant.IsProvisioned)
                {
                    _logger.LogWarning("Token refresh failed: Tenant {TenantId} not found, inactive, or not provisioned", token.TenantId);
                    return AuthResult.Failed("Tenant is not available");
                }
                
                var user = await _userRepository.GetByIdAsync(token.UserId, cancellationToken);
                if (user is not { IsActive: true })
                {
                    _logger.LogWarning("Token refresh failed: User {UserId} not found or inactive", token.UserId);
                    return AuthResult.Failed("User account is not available");
                }
                
                // Check tenant access
                var tenantUser = await _tenantUserRepository.GetTenantUserAsync(user.Id, tenant.Id, cancellationToken);
                if (tenantUser is not { IsActive: true })
                {
                    _logger.LogWarning("Token refresh failed: User {UserId} no longer has access to tenant {TenantId}", user.Id, tenant.Id);
                    return AuthResult.Failed("User no longer has access to this tenant");
                }
                
                if (tenantUser.IsCurrentlyLocked() || tenantUser.HasAccessExpired())
                {
                    _logger.LogWarning("Token refresh failed: User {UserId} account is locked or expired for tenant {TenantId}", user.Id, tenant.Id);
                    return AuthResult.Failed("User access is locked or expired");
                }
                
                // Generate new tokens
                var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(
                    user.Id,
                    tenant.Id,
                    tenant.Code,
                    user.Email,
                    tenantUser.Role,
                    tenantUser.Permissions);
                    
                var (newRefreshToken, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();
                
                // Revoke the old token and store new one
                await _refreshTokenRepository.RevokeTokenAsync(refreshToken, "Replaced with new token", newRefreshToken, cancellationToken);
                
                // Store new refresh token
                await StoreRefreshToken(
                    tenant.Id,
                    user.Id,
                    newRefreshToken,
                    refreshTokenExpiresAt,
                    clientInfo,
                    ipAddress,
                    cancellationToken);
                
                // Get available tenants for the user
                var availableTenants = await GetAvailableTenantsForUserAsync(user.Id, cancellationToken);
                
                _logger.LogInformation("Access token refreshed successfully for user {UserId} in tenant {TenantId}", user.Id, tenant.Id);
                
                return new AuthResult
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    TenantId = tenant.Id,
                    TenantCode = tenant.Code,
                    TenantName = tenant.Name,
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshToken = newRefreshToken,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaVerified = !tenantUser.RequiresMfa,
                    AvailableTenants = availableTenants
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return AuthResult.Failed("An unexpected error occurred during token refresh");
            }
        }
        
        /// <summary>
        /// Validates an access token
        /// </summary>
        /// <param name="token">The token to validate</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Validation result with claims if valid</returns>
        public async Task<(bool IsValid, IDictionary<string, string> Claims, string? ErrorMessage)> ValidateTokenAsync(
            string token,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Basic validation
                var (isValid, claims, errorMessage) = _tokenService.ValidateAccessToken(token);
                if (!isValid)
                {
                    return (false, claims, errorMessage);
                }
                
                // Extract claims
                if (!claims.TryGetValue("sub", out var userId) || 
                    !claims.TryGetValue("tenant_id", out var tenantId))
                {
                    return (false, claims, "Token missing required claims");
                }
                
                // Additional validation - check if user and tenant exist and are active
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user is not { IsActive: true })
                {
                    _logger.LogWarning("Token validation failed: User {UserId} not found or inactive", userId);
                    return (false, claims, "User account is not valid");
                }
                
                var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
                if (tenant is not { IsActive: true } || !tenant.IsProvisioned)
                {
                    _logger.LogWarning("Token validation failed: Tenant {TenantId} not found, inactive, or not provisioned", tenantId);
                    return (false, claims, "Tenant is not valid");
                }
                
                // Check user-tenant relationship
                var tenantUser = await _tenantUserRepository.GetTenantUserAsync(userId, tenantId, cancellationToken);
                if (tenantUser is not { IsActive: true })
                {
                    _logger.LogWarning("Token validation failed: User {UserId} does not have access to tenant {TenantId}", userId, tenantId);
                    return (false, claims, "User does not have access to this tenant");
                }
                
                if (tenantUser.IsCurrentlyLocked())
                {
                    _logger.LogWarning("Token validation failed: User {UserId} account is locked for tenant {TenantId}", userId, tenantId);
                    return (false, claims, "User account is locked");
                }
                
                if (tenantUser.HasAccessExpired())
                {
                    _logger.LogWarning("Token validation failed: User {UserId} access to tenant {TenantId} has expired", userId, tenantId);
                    return (false, claims, "User access has expired");
                }
                
                return (true, claims, null);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return (false, new Dictionary<string, string>(), "An error occurred during token validation");
            }
        }
        
        /// <summary>
        /// Revokes a refresh token
        /// </summary>
        /// <param name="token">The token to revoke</param>
        /// <param name="reason">Reason for revocation</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RevokeTokenAsync(
            string token,
            string reason = "Explicitly revoked",
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _refreshTokenRepository.RevokeTokenAsync(token, reason, null, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking token");
                return false;
            }
        }
        
        /// <summary>
        /// Revokes all active tokens for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="reason">Reason for revocation</param>
        /// <param name="tenantId">Optional tenant ID to limit scope</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Number of tokens revoked</returns>
        public async Task<int> RevokeAllUserTokensAsync(
            string userId,
            string reason = "User logout",
            string? tenantId = null,
            CancellationToken cancellationToken = default)
        {
            try
            {
                return await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, reason, tenantId, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking all tokens for user {UserId}", userId);
                return 0;
            }
        }
        
        /// <summary>
        /// Switches to a different tenant context for a user
        /// </summary>
        /// <param name="userId">The user ID</param>
        /// <param name="newTenantId">The new tenant ID</param>
        /// <param name="clientInfo">Optional client information</param>
        /// <param name="ipAddress">Optional IP address</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Authentication result for the new tenant</returns>
        public async Task<AuthResult> SwitchTenantContextAsync(
            string userId,
            string newTenantId,
            string clientInfo = "",
            string ipAddress = "",
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get user
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user is not { IsActive: true })
                {
                    _logger.LogWarning("Tenant switch failed: User {UserId} not found or inactive", userId);
                    return AuthResult.Failed("User not found or inactive");
                }
                
                // Get new tenant
                var tenant = await _tenantRepository.GetByIdAsync(newTenantId, cancellationToken);
                if (tenant is not { IsActive: true } || !tenant.IsProvisioned)
                {
                    _logger.LogWarning("Tenant switch failed: Tenant {TenantId} not found, inactive, or not provisioned", newTenantId);
                    return AuthResult.Failed("Target tenant is not available");
                }
                
                // Check tenant access
                var tenantUser = await _tenantUserRepository.GetTenantUserAsync(userId, newTenantId, cancellationToken);
                if (tenantUser is not { IsActive: true })
                {
                    _logger.LogWarning("Tenant switch failed: User {UserId} does not have access to tenant {TenantId}", userId, newTenantId);
                    return AuthResult.Failed("User does not have access to the target tenant");
                }
                
                if (tenantUser.IsCurrentlyLocked())
                {
                    _logger.LogWarning("Tenant switch failed: User {UserId} account is locked for tenant {TenantId}", userId, newTenantId);
                    return AuthResult.Failed("User account is locked for the target tenant");
                }
                
                if (tenantUser.HasAccessExpired())
                {
                    _logger.LogWarning("Tenant switch failed: User {UserId} access to tenant {TenantId} has expired", userId, newTenantId);
                    return AuthResult.Failed("User access has expired for the target tenant");
                }
                
                // Generate tokens for the new tenant
                var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(
                    userId,
                    tenant.Id,
                    tenant.Code,
                    user.Email,
                    tenantUser.Role,
                    tenantUser.Permissions);
                    
                var (refreshToken, refreshTokenExpiresAt) = _tokenService.GenerateRefreshToken();
                
                // Store refresh token
                await StoreRefreshToken(
                    tenant.Id,
                    userId,
                    refreshToken,
                    refreshTokenExpiresAt,
                    clientInfo,
                    ipAddress,
                    cancellationToken);
                
                // Get available tenants for the user
                var availableTenants = await GetAvailableTenantsForUserAsync(userId, cancellationToken);
                
                _logger.LogInformation("User {UserId} switched to tenant {TenantId}", userId, newTenantId);
                
                return new AuthResult
                {
                    IsSuccess = true,
                    UserId = user.Id,
                    FullName = user.FullName,
                    Email = user.Email,
                    TenantId = tenant.Id,
                    TenantCode = tenant.Code,
                    TenantName = tenant.Name,
                    AccessToken = accessToken,
                    AccessTokenExpiresAt = accessTokenExpiresAt,
                    RefreshToken = refreshToken,
                    Role = tenantUser.Role,
                    Permissions = tenantUser.Permissions,
                    RequiresMfa = tenantUser.RequiresMfa,
                    MfaVerified = !tenantUser.RequiresMfa,
                    AvailableTenants = availableTenants
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during tenant switch for user {UserId} to tenant {TenantId}", userId, newTenantId);
                return AuthResult.Failed("An unexpected error occurred during tenant switch");
            }
        }
        
        /// <summary>
        /// Adds a user to a tenant
        /// </summary>
        /// <param name="tenantId">The tenant ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="role">The user's role in the tenant</param>
        /// <param name="permissions">The user's permissions in the tenant</param>
        /// <param name="addedBy">Who added the user</param>
        /// <param name="isPrimaryTenant">Whether this is the user's primary tenant</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>TenantUser object if successful, null otherwise</returns>
        public async Task<TenantUser?> AddUserToTenantAsync(
            string tenantId,
            string userId,
            string role = "User",
            List<string>? permissions = null,
            string addedBy = "",
            bool isPrimaryTenant = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Check if tenant exists
                var tenant = await _tenantRepository.GetByIdAsync(tenantId, cancellationToken);
                if (tenant is not { IsActive: true })
                {
                    _logger.LogWarning("Add user to tenant failed: Tenant {TenantId} not found or inactive", tenantId);
                    return null;
                }
                
                // Check if user exists
                var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (user is not { IsActive: true })
                {
                    _logger.LogWarning("Add user to tenant failed: User {UserId} not found or inactive", userId);
                    return null;
                }
                
                // Check if mapping already exists
                var existingMapping = await _tenantUserRepository.GetTenantUserAsync(userId, tenantId, cancellationToken);
                if (existingMapping != null)
                {
                    // Update existing mapping if needed
                    if (!existingMapping.IsActive || existingMapping.Role != role || 
                        (permissions != null && !permissions.SequenceEqual(existingMapping.Permissions)))
                    {
                        existingMapping.IsActive = true;
                        existingMapping.Role = role;
                        if (permissions != null)
                        {
                            existingMapping.Permissions = permissions;
                        }
                        existingMapping.LastModifiedDate = DateTime.UtcNow;
                        
                        await _tenantUserRepository.UpdateAsync(existingMapping, cancellationToken);
                        _logger.LogInformation("Updated user {UserId} in tenant {TenantId}", userId, tenantId);
                    }
                    
                    return existingMapping;
                }
                
                // Create new mapping
                var tenantUser = new TenantUser
                {
                    UserId = userId,
                    TenantId = tenantId,
                    Role = role,
                    Permissions = permissions ?? new List<string>(),
                    AddedDate = DateTime.UtcNow,
                    AddedBy = addedBy,
                    IsPrimaryTenant = isPrimaryTenant,
                    IsActive = true
                };
                
                await _tenantUserRepository.InsertOneAsync(tenantUser, cancellationToken);
                _logger.LogInformation("Added user {UserId} to tenant {TenantId} with role {Role}", userId, tenantId, role);
                
                return tenantUser;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding user {UserId} to tenant {TenantId}", userId, tenantId);
                return null;
            }
        }
        
        /// <summary>
        /// Removes a user from a tenant
        /// </summary>
        /// <param name="tenantId">The tenant ID</param>
        /// <param name="userId">The user ID</param>
        /// <param name="permanently">Whether to permanently delete the mapping</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>True if successful, false otherwise</returns>
        public async Task<bool> RemoveUserFromTenantAsync(
            string tenantId,
            string userId,
            bool permanently = false,
            CancellationToken cancellationToken = default)
        {
            try
            {
                // Get mapping
                var tenantUser = await _tenantUserRepository.GetTenantUserAsync(userId, tenantId, cancellationToken);
                if (tenantUser == null)
                {
                    _logger.LogWarning("Remove user from tenant failed: Mapping not found for user {UserId} in tenant {TenantId}", userId, tenantId);
                    return false;
                }
                
                bool result;
                if (permanently)
                {
                    // Hard delete
                    result = await _tenantUserRepository.DeleteByIdAsync(tenantUser.Id, cancellationToken);
                }
                else
                {
                    // Soft delete
                    result = await _tenantUserRepository.SoftDeleteByIdAsync(tenantUser.Id, cancellationToken);
                }
                
                if (result)
                {
                    // Revoke all tokens for this user-tenant combination
                    await _refreshTokenRepository.RevokeAllUserTokensAsync(userId, "User removed from tenant", tenantId, cancellationToken);
                    
                    _logger.LogInformation("Removed user {UserId} from tenant {TenantId}", userId, tenantId);
                }
                else
                {
                    _logger.LogWarning("Failed to remove user {UserId} from tenant {TenantId}", userId, tenantId);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing user {UserId} from tenant {TenantId}", userId, tenantId);
                return false;
            }
        }
        
        // Private helper methods
        
        /// <summary>
        /// Stores a refresh token
        /// </summary>
        private async Task<RefreshToken> StoreRefreshToken(
            string tenantId,
            string userId,
            string tokenValue,
            DateTime expiresAt,
            string clientInfo,
            string ipAddress,
            CancellationToken cancellationToken)
        {
            var refreshToken = new RefreshToken
            {
                TenantId = tenantId,
                UserId = userId,
                Token = tokenValue,
                ExpiresAt = expiresAt,
                ClientId = clientInfo,
                DeviceInfo = clientInfo,
                IpAddress = ipAddress,
                CreatedDate = DateTime.UtcNow
            };
            
            await _refreshTokenRepository.InsertOneAsync(refreshToken, cancellationToken);
            return refreshToken;
        }
        
        /// <summary>
        /// Gets available tenants for a user with summary information
        /// </summary>
        private async Task<List<TenantSummary>> GetAvailableTenantsForUserAsync(
            string userId,
            CancellationToken cancellationToken)
        {
            var tenantUsers = await _tenantUserRepository.GetTenantsByUserIdAsync(userId, false, cancellationToken);
            if (!tenantUsers.Any())
            {
                return new List<TenantSummary>();
            }
            
            var result = new List<TenantSummary>();
            
            foreach (var tenantUser in tenantUsers)
            {
                if (!tenantUser.IsActive)
                    continue;
                    
                var tenant = await _tenantRepository.GetByIdAsync(tenantUser.TenantId, cancellationToken);
                if (tenant != null && tenant.IsActive && tenant.IsProvisioned)
                {
                    result.Add(new TenantSummary
                    {
                        Id = tenant.Id,
                        Code = tenant.Code,
                        Name = tenant.Name,
                        Role = tenantUser.Role,
                        IsPrimary = tenantUser.IsPrimaryTenant
                    });
                }
            }
            
            return result;
        }
    
        
    /// <summary>
    /// Validates and prepares a tenant
    /// </summary>
    private void ValidateAndPrepareTenant(Tenant tenant)
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
    
    /// <summary>
    /// Gets default permissions for tenant administrators
    /// </summary>
    private List<string> GetDefaultAdminPermissions()
    {
        return new List<string>
        {
            "manage_users",
            "manage_roles",
            "view_reports",
            "access_settings",
            "manage_data",
            "invite_users",
            "export_data",
            "view_audit_logs"
        };
    }
    
    /// <summary>
    /// Sends a welcome email to the tenant admin
    /// </summary>
    private async Task SendWelcomeEmailAsync(Tenant tenant, User admin, string password, CancellationToken cancellationToken)
    {
        // This is a placeholder - you would implement actual email sending here
        // For example, using an IEmailService interface
        
        // In a real implementation, you might:
        // 1. Use a template for the email
        // 2. Include login details and initial password
        // 3. Include tenant URL and instructions
        
        await Task.CompletedTask; // Placeholder
    }

}