using Microsoft.AspNetCore.Mvc;

namespace Demo.ElasticSearch.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IProductService productService,
        ILogger<ProductsController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    /// <summary>
    /// Search products with advanced filtering
    /// </summary>
    [HttpPost("search")]
    public async Task<ActionResult<ProductSearchResult>> SearchProducts(
        [FromBody] ProductSearchRequest request)
    {
        try
        {
            var result = await _productService.SearchProductsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get all products (paginated)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ProductSearchResult>> GetProducts(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        try
        {
            var request = new ProductSearchRequest
            {
                Page = page,
                PageSize = pageSize
            };
            
            var result = await _productService.SearchProductsAsync(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Get a specific product by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(string id)
    {
        try
        {
            var product = await _productService.GetProductByIdAsync(id);
            
            if (product == null)
                return NotFound($"Product with ID {id} not found");
                
            return Ok(product);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create a new product
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product)
    {
        try
        {
            if (product == null)
                return BadRequest("Product data is required");

            var result = await _productService.CreateProductAsync(product);
            
            if (!result)
                return BadRequest("Failed to create product");
                
            return CreatedAtAction(
                nameof(GetProduct), 
                new { id = product.Id }, 
                product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating product");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Update an existing product
    /// </summary>
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
    {
        try
        {
            if (product == null)
                return BadRequest("Product data is required");
                
            if (id != product.Id)
                return BadRequest("ID mismatch between route and body");

            var result = await _productService.UpdateProductAsync(product);
            
            if (!result)
                return NotFound($"Product with ID {id} not found");
                
            return Ok(product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Delete a product (soft delete)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        try
        {
            var result = await _productService.DeleteProductAsync(id);
            
            if (!result)
                return NotFound($"Product with ID {id} not found");
                
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product {ProductId}", id);
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Create sample products for testing OpenSearch connectivity
    /// </summary>
    [HttpPost("sample-data")]
    public async Task<ActionResult> CreateSampleData()
    {
        try
        {
            var sampleProducts = new List<Product>
            {
                new Product
                {
                    Name = "MacBook Pro 16\"",
                    Description = "Apple MacBook Pro with M3 chip, 16-inch display",
                    Price = 2499.99m,
                    CategoryId = "laptops",
                    Brand = "Apple",
                    Tags = new List<string> { "laptop", "premium", "apple", "m3" },
                    StockQuantity = 25,
                    SKU = "MBP16-M3-512",
                    Featured = true,
                    Rating = 4.8,
                    ReviewCount = 128,
                    Details = new ProductDetails
                    {
                        Manufacturer = "Apple",
                        Model = "MacBook Pro 16\"",
                        SKU = "MBP16-M3-512",
                        Specifications = new Dictionary<string, string>
                        {
                            { "Processor", "Apple M3 Pro" },
                            { "RAM", "18GB" },
                            { "Storage", "512GB SSD" },
                            { "Display", "16.2-inch Liquid Retina XDR" }
                        }
                    }
                },
                new Product
                {
                    Name = "Dell XPS 13",
                    Description = "Dell XPS 13 ultrabook with Intel Core i7",
                    Price = 1299.99m,
                    CategoryId = "laptops",
                    Brand = "Dell",
                    Tags = new List<string> { "laptop", "ultrabook", "dell", "intel" },
                    StockQuantity = 15,
                    SKU = "XPS13-I7-512",
                    Featured = false,
                    Rating = 4.5,
                    ReviewCount = 89,
                    Details = new ProductDetails
                    {
                        Manufacturer = "Dell",
                        Model = "XPS 13",
                        SKU = "XPS13-I7-512",
                        Specifications = new Dictionary<string, string>
                        {
                            { "Processor", "Intel Core i7-1360P" },
                            { "RAM", "16GB" },
                            { "Storage", "512GB SSD" },
                            { "Display", "13.4-inch FHD+" }
                        }
                    }
                },
                new Product
                {
                    Name = "iPhone 15 Pro",
                    Description = "Apple iPhone 15 Pro with A17 Pro chip",
                    Price = 999.99m,
                    CategoryId = "smartphones",
                    Brand = "Apple",
                    Tags = new List<string> { "smartphone", "apple", "a17", "pro" },
                    StockQuantity = 50,
                    SKU = "IP15P-128-TB",
                    Featured = true,
                    OnSale = true,
                    SalePrice = 899.99m,
                    Rating = 4.7,
                    ReviewCount = 256,
                    Details = new ProductDetails
                    {
                        Manufacturer = "Apple",
                        Model = "iPhone 15 Pro",
                        SKU = "IP15P-128-TB",
                        Specifications = new Dictionary<string, string>
                        {
                            { "Processor", "A17 Pro" },
                            { "Storage", "128GB" },
                            { "Camera", "48MP Main" },
                            { "Display", "6.1-inch Super Retina XDR" }
                        }
                    }
                }
            };

            foreach (var product in sampleProducts)
            {
                await _productService.CreateProductAsync(product);
            }

            return Ok(new { 
                Message = $"Created {sampleProducts.Count} sample products", 
                Count = sampleProducts.Count,
                Products = sampleProducts.Select(p => new { p.Id, p.Name, p.Price }).ToList()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating sample data");
            return StatusCode(500, "Internal server error");
        }
    }

    /// <summary>
    /// Test OpenSearch connectivity
    /// </summary>
    [HttpGet("health")]
    public async Task<ActionResult> HealthCheck()
    {
        try
        {
            // Try to get a count of products to test connectivity
            var request = new ProductSearchRequest { PageSize = 0 };
            var result = await _productService.SearchProductsAsync(request);
            
            return Ok(new 
            { 
                Status = "Healthy",
                OpenSearchConnected = true,
                TotalProducts = result.TotalCount,
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Health check failed");
            return StatusCode(500, new 
            { 
                Status = "Unhealthy",
                OpenSearchConnected = false,
                Error = ex.Message,
                Timestamp = DateTime.UtcNow
            });
        }
    }
}
