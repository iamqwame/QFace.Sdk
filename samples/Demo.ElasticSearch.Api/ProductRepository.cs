namespace Demo.ElasticSearch.Api;
public class ProductRepository : ElasticsearchRepository<Product>
{
    private readonly IOpenSearchClient _client;
    private readonly ILogger<ProductRepository> _logger;

    public ProductRepository(
        IOpenSearchClient client, 
        string indexName, 
        ILogger<ProductRepository> logger)
        : base(client, indexName, logger)
    {
        _client = client;
        _logger = logger;
    }

    // Simple search method
    public async Task<List<Product>> SearchProductsAsync(
        string searchText, 
        int page = 1, 
        int pageSize = 10)
    {
        try 
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .From((page - 1) * pageSize)
                .Size(pageSize)
                .Query(q => 
                    q.Match(m => m
                        .Field(f => f.Name)
                        .Query(searchText)
                    )
                )
            );

            return response.IsValid 
                ? response.Documents.ToList() 
                : new List<Product>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return new List<Product>();
        }
    }

    // Get product by ID
    public async Task<Product> GetProductByIdAsync(string id)
    {
        try 
        {
            var response = await _client.GetAsync<Product>(id, idx => idx.Index(IndexName));
            return response.IsValid ? response.Source : null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product by ID");
            return null;
        }
    }

    // Create or update product
    public async Task<bool> SaveProductAsync(Product product)
    {
        try 
        {
            if (string.IsNullOrEmpty(product.Id))
            {
                product.Id = Guid.NewGuid().ToString();
            }

            var response = await _client.IndexAsync(product, idx => idx.Index(IndexName));
            return response.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving product");
            return false;
        }
    }

    // Delete product
    public async Task<bool> DeleteProductAsync(string id)
    {
        try 
        {
            var response = await _client.DeleteAsync<Product>(id, idx => idx.Index(IndexName));
            return response.IsValid;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting product");
            return false;
        }
    }
}