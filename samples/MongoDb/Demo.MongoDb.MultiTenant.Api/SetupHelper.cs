using QFace.Sdk.Extensions.Services;

namespace Demo.MongoDb.MultiTenant.Api
{
    /// <summary>
    /// Helper for initializing test data
    /// </summary>
    public static class SetupHelper
    {
        /// <summary>
        /// Initializes sample data for the multi-tenant application
        /// </summary>
        public static async Task InitializeSampleDataAsync(IServiceProvider serviceProvider)
{
    // Get required services
    var tenantService = serviceProvider.GetRequiredService<ITenantService>();
    var userRepository = serviceProvider.GetRequiredService<IUserRepository>();
    var tenantRepository = serviceProvider.GetRequiredService<ITenantRepository>();
    var tenantUserRepository = serviceProvider.GetRequiredService<ITenantUserRepository>();
    var passwordHasher = serviceProvider.GetRequiredService<IPasswordHasher>();
    var logger = serviceProvider.GetRequiredService<ILogger<IServiceProvider>>();

    try
    {
        // Check if we already have tenants
        var existingTenants = await tenantRepository.GetAllAsync();
        if (existingTenants.Any())
        {
            logger.LogInformation("Sample data already exists. Skipping initialization.");
            return;
        }

        logger.LogInformation("Initializing basic tenant structure...");

        // 1. Create System Admin User
        var (adminHash, adminSalt) = passwordHasher.HashPassword("Admin123!");
        var adminUser = new User
        {
            Email = "admin@example.com",
            Username = "admin",
            FullName = "System Administrator",
            PasswordHash = adminHash,
            PasswordSalt = adminSalt,
            IsEmailVerified = true,
            IsSystemAdmin = true
        };
        
        await userRepository.InsertOneAsync(adminUser);
        logger.LogInformation("Created system admin user: {Email}", adminUser.Email);

        // 2. Create Shared Tenant First
        var tenant2Result = await tenantService.CreateTenantAsync(new TenantCreationRequest
        {
            TenantInfo = new TenantInfo
            {
                Name = "GlobalTech",
                Code = "globaltech",
                Description = "Shared tenant sample",
                TenantType = TenantType.Shared,
                Contact = new ContactInfo
                {
                    AdminName = "Jane Smith",
                    AdminEmail = "jane@globaltech.com",
                    CompanyName = "GlobalTech Inc",
                    CompanyWebsite = "https://globaltech.example.com"
                },
                Subscription = new SubscriptionInfo
                {
                    Tier = "Standard",
                    MaxUsers = 25,
                    MaxStorageMB = 5120
                }
            },
            AdminInfo = new TenantAdminInfo
            {
                Email = "admin@globaltech.com",
                FullName = "GlobalTech Administrator",
                Password = "Global123!",
                SendWelcomeEmail = false
            },
            Configuration = new TenantConfigurationInfo
            {
                ProvisionImmediately = true,
                FeatureFlags = new Dictionary<string, bool>
                {
                    { "shared_resources", true },
                    { "basic_reporting", true }
                }
            }
        });

        logger.LogInformation("Created shared tenant: {TenantName} ({TenantCode})", 
            tenant2Result.TenantName, tenant2Result.TenantCode);
            
        // Wait for provisioning to complete
        if (!tenant2Result.IsProvisioned)
        {
            logger.LogInformation("Explicitly provisioning shared tenant...");
            var sharedProvisioned = await tenantService.ProvisionAsync(tenant2Result.TenantId);
            logger.LogInformation("Shared tenant provisioning result: {Success}", sharedProvisioned);
        }

        // 3. Create Dedicated Tenant Second
        var tenant1Result = await tenantService.CreateTenantAsync(new TenantCreationRequest
        {
            TenantInfo = new TenantInfo
            {
                Name = "Acme Corporation",
                Code = "acme",
                Description = "Dedicated tenant for demonstration",
                TenantType = TenantType.Dedicated,
                Contact = new ContactInfo
                {
                    AdminName = "John Doe",
                    AdminEmail = "john@acme.com",
                    CompanyName = "Acme Corp",
                    CompanyWebsite = "https://acme.example.com"
                },
                Subscription = new SubscriptionInfo
                {
                    Tier = "Enterprise",
                    MaxUsers = 100,
                    MaxStorageMB = 10240
                }
            },
            AdminInfo = new TenantAdminInfo
            {
                Email = "admin@acme.com",
                FullName = "Acme Administrator",
                Password = "Acme123!",
                SendWelcomeEmail = false
            },
            Configuration = new TenantConfigurationInfo
            {
                ProvisionImmediately = true,
                FeatureFlags = new Dictionary<string, bool>
                {
                    { "advanced_reporting", true },
                    { "api_access", true }
                }
            }
        });

        logger.LogInformation("Created dedicated tenant: {TenantName} ({TenantCode})", 
            tenant1Result.TenantName, tenant1Result.TenantCode);
            
        // Wait for provisioning to complete
        if (!tenant1Result.IsProvisioned)
        {
            logger.LogInformation("Explicitly provisioning dedicated tenant...");
            var dedicatedProvisioned = await tenantService.ProvisionAsync(tenant1Result.TenantId);
            logger.LogInformation("Dedicated tenant provisioning result: {Success}", dedicatedProvisioned);
        }
        
        // 4. Add system admin to both tenants
        try
        {
            if (!string.IsNullOrEmpty(tenant2Result.TenantId))
            {
                logger.LogInformation("Adding system admin to GlobalTech (shared) tenant");
                var adminTenantUser = new TenantUser
                {
                    UserId = adminUser.Id,
                    TenantId = tenant2Result.TenantId,
                    Role = "SystemAdmin",
                    Permissions =
                    [
                        "manage_users",
                        "manage_tenants",
                        "access_reports",
                        "admin_settings"
                    ],
                    AddedDate = DateTime.UtcNow,
                    AddedBy = "system"
                };
                
                await tenantUserRepository.InsertOneAsync(adminTenantUser);
                logger.LogInformation("Added system admin to shared tenant {TenantName}", tenant2Result.TenantName);
            }
            
            if (!string.IsNullOrEmpty(tenant1Result.TenantId))
            {
                logger.LogInformation("Adding system admin to Acme (dedicated) tenant");
                var adminTenantUser = new TenantUser
                {
                    UserId = adminUser.Id,
                    TenantId = tenant1Result.TenantId,
                    Role = "SystemAdmin",
                    Permissions =
                    [
                        "manage_users",
                        "manage_tenants",
                        "access_reports",
                        "admin_settings"
                    ],
                    AddedDate = DateTime.UtcNow,
                    AddedBy = "system"
                };
                
                await tenantUserRepository.InsertOneAsync(adminTenantUser);
                logger.LogInformation("Added system admin to dedicated tenant {TenantName}", tenant1Result.TenantName);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error adding system admin to tenants");
        }
          
        logger.LogInformation("Basic tenant structure initialized successfully. Use API endpoints to add products.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error during initial tenant setup");
    }
}
    }
}