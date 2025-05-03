var builder = WebApplication.CreateBuilder(args);

builder.Host.AddQFaceLogging();

builder.Services.AddElasticsearch(builder.Configuration);

builder.Services.AddElasticsearchRepository<Product>();
builder.Services.AddElasticsearchRepository<Category>();
builder.Services.AddElasticsearchRepository<Order>();

builder.Services.AddElasticsearchRepository<Product, ProductRepository>("products");

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISearchService, SearchService>();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.ValidateElasticsearchRepositories();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.MapPost("/api/products", async ([FromBody] Product product, IProductService productService) =>
{
    try 
    {
        var result = await productService.CreateProductAsync(product);
        return result ? Results.Ok(product) : Results.BadRequest("Failed to create product");
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});


app.MapGet("/api/products/{id}", async (string id, IProductService productService) =>
{
    var product = await productService.GetProductByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPut("/api/products/{id}", async (string id, [FromBody] Product update, IProductService productService) =>
{
    try 
    {
        if (id != update.Id)
            return Results.BadRequest("ID mismatch");

        var result = await productService.UpdateProductAsync(update);
        return result ? Results.Ok(update) : Results.NotFound();
    }
    catch (ArgumentException ex)
    {
        return Results.BadRequest(ex.Message);
    }
});

app.MapDelete("/api/products/{id}", async (string id, IProductService productService) =>
{
    var result = await productService.DeleteProductAsync(id);
    return result ? Results.Ok() : Results.NotFound();
});

app.Run();

// Base document models
public class Product : EsBaseDocument
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string CategoryId { get; set; }
    public string Brand { get; set; }
    public List<string> Tags { get; set; } = new();
    public int StockQuantity { get; set; }
    public string SKU { get; set; }
    public string ImageUrl { get; set; }
    public ProductDetails Details { get; set; } = new();
    public double Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool Featured { get; set; }
    public bool OnSale { get; set; }
    public decimal? SalePrice { get; set; }
}

public class ProductDetails
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string SKU { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
}

public class Category : EsBaseDocument
{
    public string Name { get; set; }
    public string Description { get; set; }
    public string ParentCategoryId { get; set; }
    public int ProductCount { get; set; }
}

public class Order : EsBaseDocument
{
    public string CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; }
    public List<OrderItem> Items { get; set; } = new();
}

public class OrderItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
}

// Search-related models
public class ProductSearchRequest
{
    public string SearchText { get; set; }
    public string CategoryId { get; set; }
    public string Brand { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public List<string> Tags { get; set; }
    public bool OnlyFeatured { get; set; }
    public bool OnlyOnSale { get; set; }
    public bool InStockOnly { get; set; }
    public ProductSortOptions SortBy { get; set; } = ProductSortOptions.Relevance;
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public enum ProductSortOptions
{
    Relevance,
    PriceLowToHigh,
    PriceHighToLow,
    Rating,
    NewestFirst
}

public class ProductSearchResult
{
    public List<Product> Products { get; set; }
    public long TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public ProductFacets Facets { get; set; }
}

public class ProductFacets
{
    public List<FacetValue> Categories { get; set; } = new();
    public List<FacetValue> Brands { get; set; } = new();
    public List<FacetValue> Tags { get; set; } = new();
    public List<PriceRangeFacet> PriceRanges { get; set; } = new();
    public long OnSaleCount { get; set; }
    public long InStockCount { get; set; }
}

public class FacetValue
{
    public string Value { get; set; }
    public long Count { get; set; }
}

public class PriceRangeFacet
{
    public double? From { get; set; }
    public double? To { get; set; }
    public long Count { get; set; }

    public string GetDisplayName()
    {
        if (From == null && To != null)
            return $"Under ${To}";
        else if (From != null && To == null)
            return $"${From} & Above";
        else
            return $"${From} - ${To}";
    }
}

// Service Interfaces
public interface IProductService
{
    Task<ProductSearchResult> SearchProductsAsync(ProductSearchRequest request);
    Task<Product> GetProductByIdAsync(string id);
    Task<bool> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string id);
}

public interface ICategoryService
{
    Task<List<Category>> GetAllCategoriesAsync();
    Task<Category> GetCategoryByIdAsync(string id);
    Task<bool> CreateCategoryAsync(Category category);
    Task<bool> UpdateCategoryAsync(Category category);
}

public interface IOrderService
{
    Task<Order> CreateOrderAsync(Order order);
    Task<Order> GetOrderByIdAsync(string id);
    Task<List<Order>> GetOrdersByCustomerIdAsync(string customerId);
    Task<bool> UpdateOrderStatusAsync(string orderId, string newStatus);
}

public interface ISearchService
{
    Task<ProductSearchResult> GlobalSearchAsync(string searchText, int page = 1, int pageSize = 20);
}

