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
            var tenantAccessor = serviceProvider.GetRequiredService<ITenantAccessor>();
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

                logger.LogInformation("Initializing sample data...");

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

                // 2. Create Sample Tenants
                var tenant1Result = await tenantService.CreateTenantAsync(new TenantCreationRequest
                {
                    TenantInfo = new TenantInfo
                    {
                        Name = "Acme Corporation",
                        Code = "acme",
                        Description = "A sample tenant for demonstration",
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
                        FeatureFlags = new Dictionary<string, bool>
                        {
                            { "advanced_reporting", true },
                            { "api_access", true }
                        }
                    }
                });

                logger.LogInformation("Created tenant: {TenantName} ({TenantCode})", 
                    tenant1Result.TenantName, tenant1Result.TenantCode);

                var tenant2Result = await tenantService.CreateTenantAsync(new TenantCreationRequest
                {
                    TenantInfo = new TenantInfo
                    {
                        Name = "GlobalTech",
                        Code = "globaltech",
                        Description = "Another sample tenant",
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
                    }
                });

                logger.LogInformation("Created tenant: {TenantName} ({TenantCode})", 
                    tenant2Result.TenantName, tenant2Result.TenantCode);

                // 3. Add sample products for Acme tenant
                if (!string.IsNullOrEmpty(tenant1Result.TenantId))
                {
                    try
                    {
                        // Use the tenant accessor to set context
                        logger.LogInformation("Setting tenant context for Acme products: {TenantId}", tenant1Result.TenantId);
                        tenantAccessor.SetCurrentTenantId(tenant1Result.TenantId);
                        
                        // Verify tenant context
                        var currentTenantId = tenantAccessor.GetCurrentTenantId();
                        logger.LogInformation("Current tenant context: {CurrentTenantId}", currentTenantId);
                        
                        // Get repository with tenant context
                        logger.LogInformation("Getting product repository");
                        var productRepo = serviceProvider.GetRequiredService<IMongoRepository<Product>>();
                        logger.LogInformation("Product repository type: {Type}", productRepo.GetType().FullName);
                        
                        var acmeProducts = new List<Product>
                        {
                            new Product
                            {
                                Name = "Widget Alpha",
                                Description = "The premier widget for all your needs",
                                Price = 19.99m,
                                StockQuantity = 150,
                                SKU = "ACME-WID-001",
                                Categories = new List<string> { "Widgets", "Bestsellers" }
                            },
                            new Product
                            {
                                Name = "Widget Pro",
                                Description = "Professional grade widget with advanced features",
                                Price = 49.99m,
                                StockQuantity = 75,
                                SKU = "ACME-WID-002",
                                Categories = new List<string> { "Widgets", "Professional" }
                            }
                        };
                        
                        logger.LogInformation("Inserting {Count} Acme products", acmeProducts.Count);
                        await productRepo.InsertManyAsync(acmeProducts);
                        logger.LogInformation("Successfully added {Count} products to tenant {TenantName}", 
                            acmeProducts.Count, tenant1Result.TenantName);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, "Error adding products to Acme tenant");
                    }
                }
                
                // 5. Add system admin to both tenants
                // First tenant
                try
                {
                    if (!string.IsNullOrEmpty(tenant1Result.TenantId))
                    {
                        logger.LogInformation("Adding system admin to Acme tenant");
                        var adminTenantUser = new TenantUser
                        {
                            UserId = adminUser.Id,
                            TenantId = tenant1Result.TenantId,
                            Role = "SystemAdmin",
                            Permissions = new List<string>
                            {
                                "manage_users",
                                "manage_tenants",
                                "access_reports",
                                "admin_settings"
                            },
                            AddedDate = DateTime.UtcNow,
                            AddedBy = "system"
                        };
                        
                        await tenantUserRepository.InsertOneAsync(adminTenantUser);
                        logger.LogInformation("Added system admin to tenant {TenantName}", tenant1Result.TenantName);
                    }
                    
                    // Second tenant
                    if (!string.IsNullOrEmpty(tenant2Result.TenantId))
                    {
                        logger.LogInformation("Adding system admin to GlobalTech tenant");
                        var adminTenantUser = new TenantUser
                        {
                            UserId = adminUser.Id,
                            TenantId = tenant2Result.TenantId,
                            Role = "SystemAdmin",
                            Permissions = new List<string>
                            {
                                "manage_users",
                                "manage_tenants",
                                "access_reports",
                                "admin_settings"
                            },
                            AddedDate = DateTime.UtcNow,
                            AddedBy = "system"
                        };
                        
                        await tenantUserRepository.InsertOneAsync(adminTenantUser);
                        logger.LogInformation("Added system admin to tenant {TenantName}", tenant2Result.TenantName);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error adding system admin to tenants");
                }
                
                // Clear tenant context
                tenantAccessor.ClearCurrentTenant();
                
                logger.LogInformation("Sample data initialization completed successfully");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error during sample data initialization");
            }
        }
    }
}