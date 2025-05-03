public class SearchService : ISearchService
{
    private readonly ProductRepository _productRepository;
    private readonly ElasticsearchRepository<Category> _categoryRepository;
    private readonly ILogger<SearchService> _logger;

    public SearchService(
        ProductRepository productRepository,
        ElasticsearchRepository<Category> categoryRepository,
        ILogger<SearchService> logger)
    {
        _productRepository = productRepository;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async System.Threading.Tasks.Task<ProductSearchResult> GlobalSearchAsync(
        string searchText, 
        int page = 1, 
        int pageSize = 20)
    {
        try 
        {
            // Search products
            var products = await _productRepository.SearchProductsAsync(
                searchText, page, pageSize);

            // Optional: search categories (just an example)
            var categories = await _categoryRepository.SearchAsync(
                searchText, new[] { "name", "description" }, page, pageSize);

            return new ProductSearchResult
            {
                Products = products,
                TotalCount = products.Count,
                Page = page,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(products.Count / (double)pageSize),
                
                // Optional: you could add category search results to facets or metadata
                Facets = new ProductFacets 
                {
                    Categories = categories.Documents
                        .Select(c => new FacetValue 
                        { 
                            Value = c.Name, 
                            Count = 1 
                        })
                        .ToList()
                }
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in global search");
            return new ProductSearchResult();
        }
    }
}