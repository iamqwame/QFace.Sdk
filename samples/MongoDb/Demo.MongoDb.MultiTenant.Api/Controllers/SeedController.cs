namespace Demo.MongoDb.MultiTenant.Api.Controllers;

[ApiController]
[Route("api/seed")]
public class SeedController : ControllerBase
{
    private readonly IMongoRepository<Product> _productRepository;
    private readonly ITenantAccessor _tenantAccessor;
    private readonly ITenantRepository _tenantRepository;
    private readonly ILogger<SeedController> _logger;

    public SeedController(
        IMongoRepository<Product> productRepository,
        ITenantAccessor tenantAccessor,
        ITenantRepository tenantRepository,
        ILogger<SeedController> logger)
    {
        _productRepository = productRepository;
        _tenantAccessor = tenantAccessor;
        _tenantRepository = tenantRepository;
        _logger = logger;
    }

    [HttpPost("products/{tenantCode}")]
    [AllowAnonymous] // Only for demo purposes, secure this in production
    public async Task<IActionResult> SeedProducts(string tenantCode)
    {
        try
        {
            // Find tenant by code
            var tenant = await _tenantRepository.GetByCodeAsync(tenantCode);
            if (tenant == null)
                return NotFound($"Tenant with code '{tenantCode}' not found");

            // Set tenant context
            _tenantAccessor.SetCurrentTenantId(tenant.Id);
            _logger.LogInformation("Set tenant context to {TenantId} for product seeding", tenant.Id);

            // Create products based on tenant
            List<Product> products;
            
            if (tenant.Code.Equals("globaltech", StringComparison.OrdinalIgnoreCase))
            {
                // Products for shared tenant
                products = new List<Product>
                {
                    new Product
                    {
                        Name = "Enterprise Server",
                        Description = "High-performance server for enterprise applications",
                        Price = 2999.99m,
                        StockQuantity = 15,
                        SKU = "GT-SRV-001",
                        Categories = new List<string> { "Servers", "Enterprise" }
                    },
                    new Product
                    {
                        Name = "Cloud Storage Solution",
                        Description = "Secure cloud storage with enterprise support",
                        Price = 199.99m,
                        StockQuantity = 100,
                        SKU = "GT-CLD-001",
                        Categories = new List<string> { "Cloud", "Storage" }
                    },
                    new Product
                    {
                        Name = "Security Gateway",
                        Description = "Advanced security appliance for network protection",
                        Price = 1499.99m,
                        StockQuantity = 25,
                        SKU = "GT-SEC-001",
                        Categories = new List<string> { "Security", "Network" }
                    },
                    new Product
                    {
                        Name = "Business Analytics Suite",
                        Description = "Comprehensive analytics platform for business intelligence",
                        Price = 499.99m,
                        StockQuantity = 50,
                        SKU = "GT-BA-001",
                        Categories = new List<string> { "Analytics", "Business" }
                    }
                };
            }
            else if (tenant.Code.Equals("acme", StringComparison.OrdinalIgnoreCase))
            {
                // Products for dedicated tenant
                products = new List<Product>
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
                    },
                    new Product
                    {
                        Name = "Gadget Basic",
                        Description = "Essential gadget for everyday use",
                        Price = 24.99m,
                        StockQuantity = 200,
                        SKU = "ACME-GAD-001",
                        Categories = new List<string> { "Gadgets", "Essential" }
                    },
                    new Product
                    {
                        Name = "Gadget Deluxe",
                        Description = "Premium gadget with extended features",
                        Price = 74.99m,
                        StockQuantity = 50,
                        SKU = "ACME-GAD-002",
                        Categories = new List<string> { "Gadgets", "Premium" }
                    }
                };
            }
            else
            {
                // Generic products for any tenant
                products = new List<Product>
                {
                    new Product
                    {
                        Name = "Sample Product 1",
                        Description = "Description for sample product 1",
                        Price = 99.99m,
                        StockQuantity = 100,
                        SKU = $"{tenant.Code.ToUpper()}-001",
                        Categories = new List<string> { "Sample" }
                    },
                    new Product
                    {
                        Name = "Sample Product 2",
                        Description = "Description for sample product 2",
                        Price = 149.99m,
                        StockQuantity = 50,
                        SKU = $"{tenant.Code.ToUpper()}-002",
                        Categories = new List<string> { "Sample" }
                    }
                };
            }

            // Insert products
            _logger.LogInformation("Inserting {Count} products for tenant {TenantCode}", products.Count, tenant.Code);
            await _productRepository.InsertManyAsync(products);

            // Clear tenant context
            _tenantAccessor.ClearCurrentTenant();

            return Ok(new { 
                message = $"Successfully seeded {products.Count} products for tenant '{tenant.Name}'",
                products = products.Select(p => new { p.Id, p.Name, p.SKU })
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error seeding products for tenant {TenantCode}", tenantCode);
            
            // Ensure tenant context is cleared
            _tenantAccessor.ClearCurrentTenant();
            
            return StatusCode(500, new { error = $"Error seeding products: {ex.Message}" });
        }
    }
}