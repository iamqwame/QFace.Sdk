public class ProductService(
    ProductRepository repository,
    ILogger<ProductService> logger)
    : IProductService
{
    public async Task<ProductSearchResult> SearchProductsAsync(ProductSearchRequest request)
    {
        try 
        {
            var products = await repository.SearchProductsAsync(
                request.SearchText, 
                request.Page, 
                request.PageSize);

            return new ProductSearchResult
            {
                Products = products,
                TotalCount = products.Count,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling(products.Count / (double)request.PageSize)
            };
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error searching products");
            return new ProductSearchResult();
        }
    }

    public async Task<Product> GetProductByIdAsync(string id)
    {
        return await repository.GetProductByIdAsync(id);
    }

    public async Task<bool> CreateProductAsync(Product product)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(product.Name))
        {
            logger.LogWarning("Cannot create product with empty name");
            return false;
        }

        // Set creation timestamp
        product.CreatedDate = DateTime.UtcNow;
        product.LastModifiedDate = DateTime.UtcNow;

        return await repository.SaveProductAsync(product);
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        // Basic validation
        if (string.IsNullOrWhiteSpace(product.Id))
        {
            logger.LogWarning("Cannot update product without ID");
            return false;
        }

        // Update modification timestamp
        product.LastModifiedDate = DateTime.UtcNow;

        return await repository.SaveProductAsync(product);
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        return await repository.DeleteProductAsync(id);
    }
}