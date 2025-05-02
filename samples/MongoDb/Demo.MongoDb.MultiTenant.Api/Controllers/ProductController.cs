namespace Demo.MongoDb.MultiTenant.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize] // Require authentication for all product endpoints
public class ProductController(
    IMongoRepository<Product> productRepository,
    ITenantAccessor tenantAccessor,
    ILogger<ProductController> logger)
    : ControllerBase
{
    private readonly ITenantAccessor _tenantAccessor = tenantAccessor;

    [HttpGet]
    public async Task<IActionResult> GetAllProducts([FromQuery] string? category = null, [FromQuery] bool includeInactive = false)
    {
        try
        {
            // The TenantAwareRepository automatically applies tenant filtering
            IEnumerable<Product> products;

            if (!string.IsNullOrEmpty(category))
            {
                // Filter by category if provided
                products = await productRepository.FindAsync(
                    p => p.Categories.Contains(category),
                    includeInactive);
            }
            else
            {
                // Get all products for the current tenant
                products = await productRepository.GetAllAsync(includeInactive);
            }

            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving products");
            return StatusCode(500, new { error = "An error occurred while retrieving products" });
        }
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetProductById(string id)
    {
        try
        {
            var product = await productRepository.GetByIdAsync(id);
            if (product == null)
                return NotFound();

            return Ok(product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error retrieving product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while retrieving the product" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> CreateProduct([FromBody] Product product)
    {
        if (product == null)
            return BadRequest("Invalid product data");

        try
        {
            // Tenant ID is automatically set by the TenantAwareRepository
            await productRepository.InsertOneAsync(product);

            return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating product");
            return StatusCode(500, new { error = "An error occurred while creating the product" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, [FromBody] Product product)
    {
        if (product == null)
            return BadRequest("Invalid product data");

        try
        {
            var existingProduct = await productRepository.GetByIdAsync(id);
            if (existingProduct == null)
                return NotFound();

            // Update properties
            existingProduct.Name = product.Name;
            existingProduct.Description = product.Description;
            existingProduct.Price = product.Price;
            existingProduct.StockQuantity = product.StockQuantity;
            existingProduct.SKU = product.SKU;
            existingProduct.Categories = product.Categories;

            var updated = await productRepository.UpdateAsync(existingProduct);
            if (!updated)
                return StatusCode(500, new { error = "Failed to update product" });

            return Ok(existingProduct);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while updating the product" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        try
        {
            var deleted = await productRepository.DeleteByIdAsync(id);
            if (!deleted)
                return NotFound();

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting product {Id}", id);
            return StatusCode(500, new { error = "An error occurred while deleting the product" });
        }
    }

    [HttpPost("bulk")]
    public async Task<IActionResult> CreateBulkProducts([FromBody] List<Product> products)
    {
        if (products == null || !products.Any())
            return BadRequest("No products provided");

        try
        {
            // Tenant ID is automatically set by the TenantAwareRepository
            await productRepository.InsertManyAsync(products);

            return Ok(new { message = $"Successfully created {products.Count} products" });
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating bulk products");
            return StatusCode(500, new { error = "An error occurred while creating products" });
        }
    }

    [HttpGet("search")]
    public async Task<IActionResult> SearchProducts([FromQuery] string? query, [FromQuery] decimal? minPrice, [FromQuery] decimal? maxPrice)
    {
        try
        {
            // Build dynamic filter expression
            Expression<Func<Product, bool>> filter = p => true;

            if (!string.IsNullOrEmpty(query))
            {
                query = query.ToLower();
                filter = p => p.Name.ToLower().Contains(query) || 
                              p.Description.ToLower().Contains(query) || 
                              p.SKU.ToLower().Contains(query);
            }

            if (minPrice.HasValue)
            {
                var minPriceValue = minPrice.Value;
                Expression<Func<Product, bool>> minPriceFilter = p => p.Price >= minPriceValue;
                // Combine filters (this is simplified - in a real app, you'd use PredicateBuilder or similar)
                filter = p => filter.Compile()(p) && minPriceFilter.Compile()(p);
            }

            if (maxPrice.HasValue)
            {
                var maxPriceValue = maxPrice.Value;
                Expression<Func<Product, bool>> maxPriceFilter = p => p.Price <= maxPriceValue;
                // Combine filters (this is simplified - in a real app, you'd use PredicateBuilder or similar)
                filter = p => filter.Compile()(p) && maxPriceFilter.Compile()(p);
            }

            var products = await productRepository.FindAsync(filter);
            return Ok(products);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching products");
            return StatusCode(500, new { error = "An error occurred while searching for products" });
        }
    }
}