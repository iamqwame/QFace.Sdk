# QFace MongoDB SDK Documentation

The QFace MongoDB SDK provides a comprehensive solution for working with MongoDB in .NET applications. This document covers installation, configuration, and usage examples for both standard and multi-tenant scenarios.

## Table of Contents

1. [Installation](#installation)
2. [Configuration](#configuration)
3. [Setup](#setup)
4. [Working with Documents](#working-with-documents)
5. [Repository Pattern](#repository-pattern)
6. [Custom Repositories](#custom-repositories)
7. [Multi-Tenant Support](#multi-tenant-support)
8. [Advanced Features](#advanced-features)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

## Installation

Install the package via NuGet:

```bash
dotnet add package QFace.Sdk.MongoDb
```

Add the following namespaces to your files:

```csharp
using QFace.Sdk.MongoDb;
using QFace.Sdk.MongoDb.Models;
using QFace.Sdk.MongoDb.Repositories;
using QFace.Sdk.MongoDb.Services;
```

## Configuration

### appsettings.json

Add the following configuration to your `appsettings.json` file:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://username:password@localhost:27017",
    "DatabaseName": "YourDatabaseName",
    "UseSsl": true,
    "ConnectionTimeoutSeconds": 30,
    "ConnectionPool": {
      "MaxSize": 100,
      "MinSize": 10,
      "MaxConnectionLifeTimeMinutes": 30,
      "WaitQueueTimeoutMilliseconds": 5000
    },
    "CollectionNaming": {
      "Strategy": "PluralCamelCase",
      "ForceLowerCase": true
    }
  }
}
```

### Collection Naming Strategies

Available strategies:

- `Raw`: Uses the class name as-is (e.g., "Product")
- `CamelCase`: Converts to camelCase (e.g., "product")
- `Plural`: Pluralizes the class name (e.g., "Products")
- `PluralCamelCase`: Pluralizes and converts to camelCase (e.g., "products")

## Setup

### Basic Setup

Register MongoDB services in your `Program.cs` or `Startup.cs`:

```csharp
// Using configuration from appsettings.json
builder.Services.AddMongoDb(builder.Configuration);

// Register repositories for your documents
builder.Services.AddMongoRepository<Product>();
builder.Services.AddMongoRepository<Category>();
builder.Services.AddMongoRepository<Order>();
```

### Using Custom Database Connection

```csharp
// With explicit connection string and database name
builder.Services.AddMongoDb(
    "mongodb://username:password@localhost:27017", 
    "YourDatabaseName");
```

### Custom Connection Implementation

For more control over the connection process:

```csharp
builder.Services.Configure<MongoDbOptions>(options => {
    options.ConnectionString = "mongodb://username:password@localhost:27017";
    options.DatabaseName = "YourDatabaseName";
    options.UseSsl = true;
    options.ConnectionTimeoutSeconds = 30;
    options.ConnectionPool = new ConnectionPoolOptions {
        MaxSize = 100,
        MinSize = 10
    };
});

builder.Services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
builder.Services.AddSingleton<IMongoClient>(sp => 
    sp.GetRequiredService<IMongoDbClientFactory>().GetClient());
builder.Services.AddSingleton<IMongoDatabase>(sp => 
    sp.GetRequiredService<IMongoDbClientFactory>().GetDatabase());
```

## Working with Documents

### Creating Document Models

Create your document classes by inheriting from `BaseDocument`:

```csharp
public class Product : BaseDocument
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public int StockQuantity { get; set; }
    
    // Nested document
    public ProductDetails Details { get; set; } = new();
}

public class ProductDetails
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string SKU { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
}
```

### Base Document Properties

The `BaseDocument` class includes the following properties automatically:

```csharp
// Unique identifier
[BsonId]
[BsonRepresentation(BsonType.ObjectId)]
public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

// Creation timestamp
[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

// Creator identifier
public string CreatedBy { get; set; } = string.Empty;

// Last modification timestamp
[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

// Last modifier identifier
public string LastModifiedBy { get; set; } = string.Empty;

// Active status flag (for soft delete)
public bool IsActive { get; set; } = true;
```

## Repository Pattern

### Using the Default Repository

Inject and use `IMongoRepository<T>` in your services:

```csharp
public class ProductService
{
    private readonly IMongoRepository<Product> _repository;
    
    public ProductService(IMongoRepository<Product> repository)
    {
        _repository = repository;
    }
    
    // Get all products
    public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken: cancellationToken);
        return products.ToList();
    }
    
    // Get a product by ID
    public async Task<Product> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }
    
    // Find products by a condition
    public async Task<List<Product>> FindProductsByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var products = await _repository.FindAsync(p => p.Tags.Contains(tag), cancellationToken: cancellationToken);
        return products.ToList();
    }
    
    // Create a new product
    public async Task CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _repository.InsertOneAsync(product, cancellationToken);
    }
    
    // Update a product
    public async Task<bool> UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateAsync(product, cancellationToken);
    }
    
    // Delete a product (soft delete)
    public async Task<bool> SoftDeleteProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.SoftDeleteByIdAsync(id, cancellationToken);
    }
    
    // Delete a product (hard delete)
    public async Task<bool> HardDeleteProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteByIdAsync(id, cancellationToken);
    }
    
    // Restore a soft-deleted product
    public async Task<bool> RestoreProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.RestoreByIdAsync(id, cancellationToken);
    }
}
```

### Complete Repository Interface

The `IMongoRepository<T>` interface includes the following methods:

```csharp
// Get all documents
Task<IEnumerable<TDocument>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

// Get a document by ID
Task<TDocument> GetByIdAsync(string id, CancellationToken cancellationToken = default);

// Find a single document by a condition
Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression, bool includeInactive = false, CancellationToken cancellationToken = default);

// Find documents by a condition
Task<IEnumerable<TDocument>> FindAsync(Expression<Func<TDocument, bool>> filterExpression, bool includeInactive = false, CancellationToken cancellationToken = default);

// Insert a single document
Task InsertOneAsync(TDocument document, CancellationToken cancellationToken = default);

// Insert multiple documents
Task InsertManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);

// Update a document
Task<bool> UpdateAsync(TDocument document, CancellationToken cancellationToken = default);

// Replace a document
Task<bool> ReplaceOneAsync(TDocument document, CancellationToken cancellationToken = default);

// Delete a document (hard delete)
Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default);

// Soft delete a document
Task<bool> SoftDeleteByIdAsync(string id, CancellationToken cancellationToken = default);

// Restore a soft-deleted document
Task<bool> RestoreByIdAsync(string id, CancellationToken cancellationToken = default);

// Create indexes
Task CreateIndexesAsync(CancellationToken cancellationToken = default);
```

## Custom Repositories

### Creating Custom Repositories

Create custom repositories by extending `MongoRepository<T>`:

```csharp
public class ProductRepository : MongoRepository<Product>
{
    public ProductRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger<ProductRepository> logger)
        : base(database, collectionName, logger)
    {
    }
    
    // Override to create custom indexes
    public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Define indexes
            var nameIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Name),
                new CreateIndexOptions { Background = true, Name = "name_idx" }
            );
            
            var categoryIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Category),
                new CreateIndexOptions { Background = true, Name = "category_idx" }
            );
            
            var priceIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Price),
                new CreateIndexOptions { Background = true, Name = "price_idx" }
            );
            
            var tagsIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Tags),
                new CreateIndexOptions { Background = true, Name = "tags_idx" }
            );
            
            var textIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Text(p => p.Name).Text(p => p.Description),
                new CreateIndexOptions { Background = true, Name = "text_idx" }
            );
            
            // Create all indexes
            await _collection.Indexes.CreateManyAsync(
                new[] { nameIndex, categoryIndex, priceIndex, tagsIndex, textIndex },
                cancellationToken);
            
            _logger.LogInformation("Created indexes for product collection");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating indexes for product collection");
        }
    }
    
    // Custom query methods
    
    // Find products by price range
    public async Task<List<Product>> FindByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Gte(p => p.Price, minPrice),
                Builders<Product>.Filter.Lte(p => p.Price, maxPrice),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            return await _collection.Find(filter)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Price))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding products by price range {MinPrice} to {MaxPrice}", 
                minPrice, maxPrice);
            return new List<Product>();
        }
    }
    
    // Find products by category with pagination
    public async Task<(List<Product> Products, long TotalCount)> FindByCategoryPaginatedAsync(
        string category,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.Category, category),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            var totalCount = await _collection.CountDocumentsAsync(filter, 
                new CountOptions(), cancellationToken);
            
            var products = await _collection.Find(filter)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Name))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);
                
            return (products, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding products by category {Category} with pagination", category);
            return (new List<Product>(), 0);
        }
    }
    
    // Search products by text
    public async Task<List<Product>> SearchProductsAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Text(searchText),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            return await _collection.Find(filter)
                .Sort(Builders<Product>.Sort.MetaTextScore("score"))
                .Limit(50)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products for text {SearchText}", searchText);
            return new List<Product>();
        }
    }
    
    // Update stock quantity atomically
    public async Task<bool> UpdateStockQuantityAsync(
        string productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .Set(p => p.StockQuantity, quantity)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock quantity for product {ProductId}", productId);
            return false;
        }
    }
    
    // Increment stock quantity (useful for inventory adjustments)
    public async Task<bool> IncrementStockQuantityAsync(
        string productId, 
        int amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .Inc(p => p.StockQuantity, amount)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing stock quantity for product {ProductId}", productId);
            return false;
        }
    }
    
    // Add a tag to a product
    public async Task<bool> AddTagAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .AddToSet(p => p.Tags, tag)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tag {Tag} to product {ProductId}", tag, productId);
            return false;
        }
    }
    
    // Remove a tag from a product
    public async Task<bool> RemoveTagAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .Pull(p => p.Tags, tag)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag {Tag} from product {ProductId}", tag, productId);
            return false;
        }
    }
}
```

### Registering Custom Repositories

Register custom repositories in your startup:

```csharp
// Register custom repository
builder.Services.AddMongoRepository<Product, ProductRepository>();

// Register with custom collection name
builder.Services.AddMongoRepository<Product, ProductRepository>("custom_products");
```

### Using Custom Repositories in Services

```csharp
public class EnhancedProductService
{
    private readonly ProductRepository _repository;
    
    public EnhancedProductService(ProductRepository repository)
    {
        _repository = repository;
    }
    
    // Use custom repository methods
    public async Task<List<Product>> GetProductsByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        return await _repository.FindByPriceRangeAsync(minPrice, maxPrice, cancellationToken);
    }
    
    public async Task<(List<Product> Products, long TotalCount, int TotalPages)> GetProductsByCategoryPaginatedAsync(
        string category,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.FindByCategoryPaginatedAsync(
            category, page, pageSize, cancellationToken);
            
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);
        
        return (result.Products, result.TotalCount, totalPages);
    }
    
    public async Task<List<Product>> SearchProductsAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        return await _repository.SearchProductsAsync(searchText, cancellationToken);
    }
    
    // Inventory management
    public async Task<bool> UpdateInventoryAsync(
        string productId, 
        int newQuantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateStockQuantityAsync(productId, newQuantity, cancellationToken);
    }
    
    public async Task<bool> AddInventoryAsync(
        string productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.IncrementStockQuantityAsync(productId, quantity, cancellationToken);
    }
    
    public async Task<bool> RemoveInventoryAsync(
        string productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.IncrementStockQuantityAsync(productId, -quantity, cancellationToken);
    }
    
    // Tag management
    public async Task<bool> AddTagToProductAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        return await _repository.AddTagAsync(productId, tag, cancellationToken);
    }
    
    public async Task<bool> RemoveTagFromProductAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        return await _repository.RemoveTagAsync(productId, tag, cancellationToken);
    }
}
```

## Multi-Tenant Support

### Setting Up Multi-Tenant Support

First, create a tenant service to determine the current tenant:

```csharp
// Create a tenant service interface
public interface ITenantService
{
    string GetCurrentTenantId();
}

// Implement the tenant service for your application
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GetCurrentTenantId()
    {
        // Get tenant ID from header
        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        
        // Or from claim
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = _httpContextAccessor.HttpContext?.User.FindFirstValue("TenantId");
        }
        
        // Fall back to default if not found
        return tenantId ?? "default";
    }
}

// Register the tenant service
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddHttpContextAccessor();
```

### Registering Multi-Tenant Repositories

```csharp
// Register multi-tenant repositories
builder.Services.AddMultiTenantRepository<Product>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());
    
builder.Services.AddMultiTenantRepository<Order>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());
    
// With custom database name format (default is "{0}_db")
builder.Services.AddMultiTenantRepository<Customer>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId(),
    databaseNameFormat: "tenant_{0}");
    
// With custom collection name
builder.Services.AddMultiTenantRepository<User>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId(),
    collectionName: "users");
```

### Creating Custom Multi-Tenant Repositories

```csharp
public class TenantProductRepository : MultiTenantRepository<Product>
{
    public TenantProductRepository(
        IMongoDbProvider dbProvider,
        string collectionName,
        string tenantId,
        ILogger<TenantProductRepository> logger,
        string databaseNameFormat = "{0}_db")
        : base(dbProvider, collectionName, tenantId, logger, databaseNameFormat)
    {
    }
    
    // Custom methods similar to the regular repository example
    // Replace _collection with Collection property
    
    public async Task<List<Product>> FindByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Gte(p => p.Price, minPrice),
                Builders<Product>.Filter.Lte(p => p.Price, maxPrice),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            return await Collection.Find(filter)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Price))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding products by price range {MinPrice} to {MaxPrice} for tenant {TenantId}", 
                minPrice, maxPrice, _tenantId);
            return new List<Product>();
        }
    }
    
    // Add other custom methods as needed
}
```

### Register and Use Custom Multi-Tenant Repository

```csharp
// Register custom multi-tenant repository
builder.Services.AddMultiTenantRepository<Product, TenantProductRepository>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());

// Use in a service
public class TenantProductService
{
    private readonly TenantProductRepository _repository;
    private readonly ITenantService _tenantService;
    
    public TenantProductService(
        TenantProductRepository repository,
        ITenantService tenantService)
    {
        _repository = repository;
        _tenantService = tenantService;
    }
    
    public async Task<List<Product>> GetProductsByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        return await _repository.FindByPriceRangeAsync(minPrice, maxPrice, cancellationToken);
    }
    
    // Other methods as needed
}
```

### Using the MongoDbProvider Directly

For more control over databases and collections:

```csharp
public class MultiDatabaseService
{
    private readonly IMongoDbProvider _dbProvider;
    private readonly ITenantService _tenantService;
    
    public MultiDatabaseService(
        IMongoDbProvider dbProvider,
        ITenantService tenantService)
    {
        _dbProvider = dbProvider;
        _tenantService = tenantService;
    }
    
    // Get collection from a specific database
    public async Task<List<Product>> GetProductsFromDatabaseAsync(
        string databaseName,
        CancellationToken cancellationToken = default)
    {
        var collection = _dbProvider.GetCollection<Product>("products", databaseName);
        return await collection.Find(_ => true).ToListAsync(cancellationToken);
    }
    
    // Get collection from tenant database
    public async Task<List<Product>> GetProductsForTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var collection = _dbProvider.GetTenantCollection<Product>(tenantId, "products");
        return await collection.Find(_ => true).ToListAsync(cancellationToken);
    }
    
    // Perform operations across multiple tenant databases
    public async Task<Dictionary<string, int>> GetProductCountsForAllTenantsAsync(
        List<string> tenantIds,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, int>();
        
        foreach (var tenantId in tenantIds)
        {
            var collection = _dbProvider.GetTenantCollection<Product>(tenantId, "products");
            var count = await collection.CountDocumentsAsync(FilterDefinition<Product>.Empty, null, cancellationToken);
            results[tenantId] = (int)count;
        }
        
        return results;
    }
}
```

## Advanced Features

### Working with Transactions

MongoDB supports transactions (for replica sets) when you need atomic operations:

```csharp
public class OrderService
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoRepository<Order> _orderRepository;
    private readonly IMongoRepository<Product> _productRepository;
    private readonly IMongoRepository<Customer> _customerRepository;
    
    public OrderService(
        IMongoClient mongoClient,
        IMongoRepository<Order> orderRepository,
        IMongoRepository<Product> productRepository,
        IMongoRepository<Customer> customerRepository)
    {
        _mongoClient = mongoClient;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
    }
    
    public async Task<bool> CreateOrderWithInventoryUpdateAsync(Order order, string customerId)
    {
        using (var session = await _mongoClient.StartSessionAsync())
        {
            session.StartTransaction();
            
            try
            {
                // 1. Create order
                await _orderRepository.InsertOneAsync(order);
                
                // 2. Update inventory for each product
                foreach (var item in order.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        // Not enough inventory
                        await session.AbortTransactionAsync();
                        return false;
                    }
                    
                    product.StockQuantity -= item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
                
                // 3. Update customer's order history
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer != null)
                {
                    customer.LastOrderDate = DateTime.UtcNow;
                    customer.TotalOrders++;
                    customer.TotalSpent += order.Total;
                    await _customerRepository.UpdateAsync(customer);
                }
                
                // Commit the transaction
                await session.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                // If any operation fails, abort the transaction
                await session.AbortTransactionAsync();
                return false;
            }
        }
    }
}
```

### Using Aggregation Pipeline

For complex queries, you can use MongoDB's aggregation pipeline:

```csharp
public class SalesAnalyticsRepository : MongoRepository<Order>
{
    public SalesAnalyticsRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger<SalesAnalyticsRepository> logger)
        : base(database, collectionName, logger)
    {
    }
    
    // Get sales by category
    public async Task<List<CategorySales>> GetSalesByCategoryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
           var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "OrderDate", new BsonDocument
                        {
                            { "$gte", startDate },
                            { "$lte", endDate }
                        }
                    },
                    { "IsActive", true }
                }),
                new BsonDocument("$unwind", "$Items"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Items.Category" },
                    { "totalSales", new BsonDocument("$sum", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })) },
                    { "count", new BsonDocument("$sum", 1) },
                    { "averageOrderValue", new BsonDocument("$avg", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })) }
                }),
                new BsonDocument("$sort", new BsonDocument("totalSales", -1))
            };
            
            var results = await _collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);
                
            return results.Select(doc => new CategorySales
            {
                Category = doc["_id"].AsString,
                TotalSales = doc["totalSales"].AsDecimal,
                Count = doc["count"].AsInt32,
                AverageOrderValue = doc["averageOrderValue"].AsDecimal
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting sales by category from {StartDate} to {EndDate}", 
                startDate, endDate);
            return new List<CategorySales>();
        }
    }
    
    // Get sales by day
    public async Task<List<DailySales>> GetDailySalesAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument
                {
                    { "OrderDate", new BsonDocument
                        {
                            { "$gte", startDate },
                            { "$lte", endDate }
                        }
                    },
                    { "IsActive", true }
                }),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", new BsonDocument
                        {
                            { "year", new BsonDocument("$year", "$OrderDate") },
                            { "month", new BsonDocument("$month", "$OrderDate") },
                            { "day", new BsonDocument("$dayOfMonth", "$OrderDate") }
                        }
                    },
                    { "totalSales", new BsonDocument("$sum", "$Total") },
                    { "orderCount", new BsonDocument("$sum", 1) },
                    { "averageOrderValue", new BsonDocument("$avg", "$Total") }
                }),
                new BsonDocument("$sort", new BsonDocument("_id.year", 1)
                    .Add("_id.month", 1)
                    .Add("_id.day", 1))
            };
            
            var results = await _collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);
                
            return results.Select(doc => new DailySales
            {
                Date = new DateTime(
                    doc["_id"]["year"].AsInt32,
                    doc["_id"]["month"].AsInt32,
                    doc["_id"]["day"].AsInt32),
                TotalSales = doc["totalSales"].AsDecimal,
                OrderCount = doc["orderCount"].AsInt32,
                AverageOrderValue = doc["averageOrderValue"].AsDecimal
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting daily sales from {StartDate} to {EndDate}", 
                startDate, endDate);
            return new List<DailySales>();
        }
    }
    
    // Get top selling products
    public async Task<List<TopSellingProduct>> GetTopSellingProductsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var pipeline = new BsonDocument[]
            {
                new BsonDocument("$match", new BsonDocument("IsActive", true)),
                new BsonDocument("$unwind", "$Items"),
                new BsonDocument("$group", new BsonDocument
                {
                    { "_id", "$Items.ProductId" },
                    { "name", new BsonDocument("$first", "$Items.ProductName") },
                    { "totalQuantity", new BsonDocument("$sum", "$Items.Quantity") },
                    { "totalRevenue", new BsonDocument("$sum", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })) }
                }),
                new BsonDocument("$sort", new BsonDocument("totalQuantity", -1)),
                new BsonDocument("$limit", limit)
            };
            
            var results = await _collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
                .ToListAsync(cancellationToken);
                
            return results.Select(doc => new TopSellingProduct
            {
                ProductId = doc["_id"].AsString,
                ProductName = doc["name"].AsString,
                TotalQuantity = doc["totalQuantity"].AsInt32,
                TotalRevenue = doc["totalRevenue"].AsDecimal
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting top selling products");
            return new List<TopSellingProduct>();
        }
    }
}

// Result classes for aggregation queries
public class CategorySales
{
    public string Category { get; set; }
    public decimal TotalSales { get; set; }
    public int Count { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class DailySales
{
    public DateTime Date { get; set; }
    public decimal TotalSales { get; set; }
    public int OrderCount { get; set; }
    public decimal AverageOrderValue { get; set; }
}

public class TopSellingProduct
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public int TotalQuantity { get; set; }
    public decimal TotalRevenue { get; set; }
}
```

### Using Fluent Aggregation API

You can also use the more type-safe fluent API for aggregations:

```csharp
public async Task<List<CategorySales>> GetSalesByCategoryFluentAsync(
    DateTime startDate,
    DateTime endDate,
    CancellationToken cancellationToken = default)
{
    try
    {
        var results = await _collection.Aggregate()
            .Match(o => 
                o.OrderDate >= startDate && 
                o.OrderDate <= endDate && 
                o.IsActive)
            .Unwind<Order, OrderItemUnwind>(o => o.Items)
            .Group(
                key => key.Items.Category, 
                group => new CategorySales
                {
                    Category = group.Key,
                    TotalSales = group.Sum(o => o.Items.Quantity * o.Items.UnitPrice),
                    Count = group.Count(),
                    AverageOrderValue = group.Average(o => o.Items.Quantity * o.Items.UnitPrice)
                })
            .SortByDescending(r => r.TotalSales)
            .ToListAsync(cancellationToken);
            
        return results;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting sales by category from {StartDate} to {EndDate}", 
            startDate, endDate);
        return new List<CategorySales>();
    }
}
```

### Using LINQ Queries (MongoDB C# Driver)

MongoDB's C# driver supports LINQ for simpler queries:

```csharp
public async Task<List<Product>> FindProductsWithLinqAsync(
    string category,
    decimal minPrice,
    CancellationToken cancellationToken = default)
{
    try
    {
        return await _collection.AsQueryable()
            .Where(p => 
                p.Category == category && 
                p.Price >= minPrice && 
                p.IsActive)
            .OrderBy(p => p.Price)
            .ToListAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error finding products with LINQ");
        return new List<Product>();
    }
}
```

### Change Streams (for Real-time Updates)

MongoDB supports change streams for listening to collection changes:

```csharp
public class ProductChangeTracker : IHostedService
{
    private readonly IMongoRepository<Product> _repository;
    private readonly ILogger<ProductChangeTracker> _logger;
    private IChangeStreamCursor<ChangeStreamDocument<Product>> _cursor;
    private CancellationTokenSource _cts;
    
    public ProductChangeTracker(
        IMongoRepository<Product> repository,
        ILogger<ProductChangeTracker> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public Task StartAsync(CancellationToken cancellationToken)
    {
        _cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        
        // Get the collection field via reflection
        var field = _repository.GetType().BaseType.GetField("_collection", 
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            
        var collection = (IMongoCollection<Product>)field.GetValue(_repository);
        
        // Start monitoring changes
        var pipeline = new EmptyPipelineDefinition<ChangeStreamDocument<Product>>()
            .Match(change => 
                change.OperationType == ChangeStreamOperationType.Insert || 
                change.OperationType == ChangeStreamOperationType.Update || 
                change.OperationType == ChangeStreamOperationType.Replace);
                
        _cursor = collection.Watch(pipeline);
        
        Task.Run(async () => await MonitorChangesAsync(_cts.Token));
        
        return Task.CompletedTask;
    }
    
    private async Task MonitorChangesAsync(CancellationToken cancellationToken)
    {
        await foreach (var change in _cursor.ToEnumerable(cancellationToken))
        {
            try
            {
                switch (change.OperationType)
                {
                    case ChangeStreamOperationType.Insert:
                        _logger.LogInformation("Product inserted: {ProductId}", 
                            change.FullDocument.Id);
                        // Process new product
                        break;
                    case ChangeStreamOperationType.Update:
                    case ChangeStreamOperationType.Replace:
                        _logger.LogInformation("Product updated: {ProductId}", 
                            change.FullDocument.Id);
                        // Process updated product
                        break;
                }
                
                // Notify other services about the change
                // e.g., broadcast via SignalR, update cache, etc.
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing product change");
            }
        }
    }
    
    public Task StopAsync(CancellationToken cancellationToken)
    {
        _cts?.Cancel();
        _cursor?.Dispose();
        return Task.CompletedTask;
    }
}
```

## Best Practices

### Indexing

Properly indexing collections is crucial for performance:

```csharp
public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
{
    // Rules for effective indexing:
    // 1. Index fields frequently used in queries (e.g., Name, Category)
    // 2. Index fields used for sorting
    // 3. Create compound indexes for queries that filter on multiple fields
    // 4. Use text indexes for full-text search
    // 5. Use unique indexes where appropriate
    
    var indexes = new List<CreateIndexModel<Product>>
    {
        // Simple indexes
        new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.Name),
            new CreateIndexOptions { Background = true }
        ),
        
        // Compound indexes (order matters! Put equality filters first, then range filters)
        new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys
                .Ascending(p => p.Category)    // Equality filter
                .Ascending(p => p.Price),      // Range filter or sort
            new CreateIndexOptions { Background = true }
        ),
        
        // Text index for searching
        new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Text(p => p.Name).Text(p => p.Description),
            new CreateIndexOptions { Background = true }
        ),
        
        // Unique index
        new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.SKU),
            new CreateIndexOptions { Background = true, Unique = true }
        ),
        
        // TTL index for expiring documents
        new CreateIndexModel<Product>(
            Builders<Product>.IndexKeys.Ascending(p => p.ExpiryDate),
            new CreateIndexOptions { Background = true, ExpireAfter = TimeSpan.Zero }
        )
    };
    
    await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
}
```

### Document Design

Best practices for MongoDB document design:

1. **Embed Related Data**: Use embedded documents for one-to-few relationships
   ```csharp
   public class Order : BaseDocument
   {
       public string CustomerId { get; set; }
       public DateTime OrderDate { get; set; }
       public List<OrderItem> Items { get; set; } = new();
       public Address ShippingAddress { get; set; }
       public PaymentInfo Payment { get; set; }
   }
   ```

2. **Reference Related Data**: Use references for one-to-many or many-to-many relationships
   ```csharp
   public class Customer : BaseDocument
   {
       public string Name { get; set; }
       public string Email { get; set; }
       public List<string> OrderIds { get; set; } = new(); // References to Order documents
   }
   ```

3. **Balance Between Embedding and Referencing**
   ```csharp
   // Good approach for a blog post with comments
   public class BlogPost : BaseDocument
   {
       public string Title { get; set; }
       public string Content { get; set; }
       public string AuthorId { get; set; }
       
       // Embed some recent comments inline
       public List<Comment> RecentComments { get; set; } = new();
       
       // Reference to all comments collection
       public string CommentsCollectionName { get; set; }
   }
   ```

4. **Avoid Unbounded Arrays**: Use separate collections for potentially large arrays

5. **Use Proper Data Types**: Choose appropriate .NET types that map well to BSON types

### Performance Optimization

Techniques for optimizing MongoDB performance:

1. **Use Projections**: Retrieve only the fields you need
   ```csharp
   var products = await _collection.Find(filter)
       .Project(Builders<Product>.Projection
           .Include(p => p.Id)
           .Include(p => p.Name)
           .Include(p => p.Price))
       .ToListAsync();
   ```

2. **Pagination**: Always use Skip and Limit for large result sets
   ```csharp
   var pageSize = 20;
   var page = 1;
   
   var products = await _collection.Find(filter)
       .Skip((page - 1) * pageSize)
       .Limit(pageSize)
       .ToListAsync();
   ```

3. **Batch Processing**: Process large datasets in batches
   ```csharp
   using var cursor = await _collection.Find(filter).ToCursorAsync();
   
   while (await cursor.MoveNextAsync())
   {
       foreach (var product in cursor.Current)
       {
           // Process product
       }
   }
   ```

4. **Use Covered Queries**: Design queries that can be satisfied entirely by an index
   ```csharp
   // Create an index that covers the query
   await _collection.Indexes.CreateOneAsync(
       Builders<Product>.IndexKeys
           .Ascending(p => p.Category)
           .Ascending(p => p.Name));
   
   // Use a covered query (projection includes only indexed fields)
   var products = await _collection.Find(p => p.Category == "Electronics")
       .Project(Builders<Product>.Projection
           .Include(p => p.Category)
           .Include(p => p.Name)
           .Exclude(p => p.Id))
       .ToListAsync();
   ```

5. **Use Bulk Operations**: For multiple document operations
   ```csharp
   public async Task UpdateManyProductPricesAsync(
       Dictionary<string, decimal> productPriceUpdates,
       CancellationToken cancellationToken = default)
   {
       var bulkOps = new List<WriteModel<Product>>();
       
       foreach (var kvp in productPriceUpdates)
       {
           var filter = Builders<Product>.Filter.Eq(p => p.Id, kvp.Key);
           var update = Builders<Product>.Update
               .Set(p => p.Price, kvp.Value)
               .Set(p => p.LastModifiedDate, DateTime.UtcNow);
               
           bulkOps.Add(new UpdateOneModel<Product>(filter, update));
       }
       
       if (bulkOps.Count > 0)
       {
           await _collection.BulkWriteAsync(bulkOps, cancellationToken: cancellationToken);
       }
   }
   ```

## Troubleshooting

### Common Errors

1. **Connection Issues**
    - Check the connection string
    - Verify network connectivity
    - Check MongoDB server status
    - Verify authentication credentials

2. **Query Performance Problems**
    - Check for missing indexes
    - Review query execution plan: `db.collection.explain("executionStats").find(...)`
    - Look for large documents or arrays
    - Check for inefficient queries (e.g., not using indexes)

3. **Serialization Errors**
    - Check for circular references
    - Verify property types match BSON types
    - Use BsonIgnore attribute for properties that shouldn't be serialized

4. **DuplicateKey Exceptions**
    - Check for unique index violations
    - Handle duplicate keys gracefully

5. **WriteConflict Exceptions**
    - Consider using transactions for complex operations
    - Implement retry logic for write conflicts

### Debugging MongoDB Operations

```csharp
// Enable MongoDB command logging
builder.Logging.AddFilter("MongoDB.Driver", LogLevel.Debug);

// Log MongoDB queries in the repository
public async Task<List<Product>> FindProductsByQueryAsync(string category, CancellationToken cancellationToken = default)
{
    try
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Category, category);
        
        // Log the filter for debugging
        _logger.LogDebug("MongoDB Query: {Filter}", filter.Render(
            _collection.DocumentSerializer, 
            _collection.Settings.SerializerRegistry));
        
        return await _collection.Find(filter).ToListAsync(cancellationToken);
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error finding products by category {Category}", category);
        return new List<Product>();
    }
}
```

### Implementing Retry Logic

Use Polly for retry logic on transient errors:

```csharp
// Add Polly package
// dotnet add package Polly

using Polly;

// Create a retry policy
var retryPolicy = Policy
    .Handle<MongoConnectionException>()
    .Or<MongoExecutionTimeoutException>()
    .WaitAndRetryAsync(
        3,
        retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
        (exception, timeSpan, retryCount, context) =>
        {
            _logger.LogWarning(exception, "MongoDB operation failed. Retrying ({RetryCount}/3)...", retryCount);
        });

// Use the policy
public async Task<Product> GetProductWithRetryAsync(string id, CancellationToken cancellationToken = default)
{
    return await retryPolicy.ExecuteAsync(async () => 
        await _repository.GetByIdAsync(id, cancellationToken));
}
```

## Complete Example: E-commerce Application

Here's a complete example of using QFace.Sdk.MongoDb in an e-commerce application:

```csharp
// 1. Register MongoDB services
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add MongoDB
builder.Services.AddMongoDb(builder.Configuration);

// Register repositories
builder.Services.AddMongoRepository<Product, ProductRepository>();
builder.Services.AddMongoRepository<Order, OrderRepository>();
builder.Services.AddMongoRepository<Customer, CustomerRepository>();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ICustomerService, CustomerService>();

// 2. Document Models

// Product.cs
public class Product : BaseDocument
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public int StockQuantity { get; set; }
    public string SKU { get; set; }
    public string ImageUrl { get; set; }
    public ProductDetails Details { get; set; } = new();
}

public class ProductDetails
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
    public List<string> Features { get; set; } = new();
    public decimal Weight { get; set; }
    public string Dimensions { get; set; }
}

// Order.cs
public class Order : BaseDocument
{
    public string CustomerId { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    public List<OrderItem> Items { get; set; } = new();
    public decimal Subtotal { get; set; }
    public decimal Tax { get; set; }
    public decimal Shipping { get; set; }
    public decimal Total { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public Address ShippingAddress { get; set; }
    public PaymentInfo Payment { get; set; }
}

public class OrderItem
{
    public string ProductId { get; set; }
    public string ProductName { get; set; }
    public string ProductSKU { get; set; }
    public string Category { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal => Quantity * UnitPrice;
}

public enum OrderStatus
{
    Pending,
    Processing,
    Shipped,
    Delivered,
    Cancelled,
    Refunded
}

// Customer.cs
public class Customer : BaseDocument
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }
    public List<Address> Addresses { get; set; } = new();
    public DateTime LastOrderDate { get; set; }
    public int TotalOrders { get; set; }
    public decimal TotalSpent { get; set; }
}

public class Address
{
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public bool IsDefault { get; set; }
}

public class PaymentInfo
{
    public string Method { get; set; }
    public string TransactionId { get; set; }
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Status { get; set; }
}

// 3. Custom Repositories

// ProductRepository.cs
public class ProductRepository : MongoRepository<Product>
{
    public ProductRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger<ProductRepository> logger)
        : base(database, collectionName, logger)
    {
    }
    
    public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        var indexes = new List<CreateIndexModel<Product>>
        {
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Name),
                new CreateIndexOptions { Background = true }
            ),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Category),
                new CreateIndexOptions { Background = true }
            ),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Price),
                new CreateIndexOptions { Background = true }
            ),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Tags),
                new CreateIndexOptions { Background = true }
            ),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Text(p => p.Name).Text(p => p.Description),
                new CreateIndexOptions { Background = true }
            ),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.SKU),
                new CreateIndexOptions { Background = true, Unique = true }
            )
        };
        
        await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
    }
    
    // Search products
    public async Task<List<Product>> SearchProductsAsync(
        string searchText,
        string category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filterBuilder = Builders<Product>.Filter;
            var filters = new List<FilterDefinition<Product>>
            {
                filterBuilder.Eq(p => p.IsActive, true)
            };
            
            // Add text search if provided
            if (!string.IsNullOrEmpty(searchText))
            {
                filters.Add(filterBuilder.Text(searchText));
            }
            
            // Add category filter if provided
            if (!string.IsNullOrEmpty(category))
            {
                filters.Add(filterBuilder.Eq(p => p.Category, category));
            }
            
            // Add price range filter if provided
            if (minPrice.HasValue)
            {
                filters.Add(filterBuilder.Gte(p => p.Price, minPrice.Value));
            }
            
            if (maxPrice.HasValue)
            {
                filters.Add(filterBuilder.Lte(p => p.Price, maxPrice.Value));
            }
            
            var filter = filterBuilder.And(filters);
            
            // Create sort definition based on search text
            var sort = string.IsNullOrEmpty(searchText)
                ? Builders<Product>.Sort.Ascending(p => p.Name)
                : Builders<Product>.Sort.MetaTextScore("score");
            
            return await _collection.Find(filter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return new List<Product>();
        }
    }
    
    // Update stock
    public async Task<bool> UpdateStockAsync(
        string productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .Set(p => p.StockQuantity, quantity)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
            
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
            return false;
        }
    }
}

// 4. Service Implementation

// ProductService.cs
public interface IProductService
{
    Task<List<Product>> SearchProductsAsync(string searchText, string category = null, 
        decimal? minPrice = null, decimal? maxPrice = null, int page = 1, int pageSize = 20);
    Task<Product> GetProductByIdAsync(string id);
    Task<Product> GetProductBySkuAsync(string sku);
    Task<string> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string id, bool softDelete = true);
    Task<bool> UpdateStockAsync(string productId, int quantity);
}

public class ProductService : IProductService
{
    private readonly ProductRepository _repository;
    private readonly ILogger<ProductService> _logger;
    
    public ProductService(
        ProductRepository repository,
        ILogger<ProductService> logger)
    {
        _repository = repository;
        _logger = logger;
    }
    
    public async Task<List<Product>> SearchProductsAsync(
        string searchText, 
        string category = null, 
        decimal? minPrice = null, 
        decimal? maxPrice = null, 
        int page = 1, 
        int pageSize = 20)
    {
        return await _repository.SearchProductsAsync(
            searchText, category, minPrice, maxPrice, page, pageSize);
    }
    
    public async Task<Product> GetProductByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }
    
    public async Task<Product> GetProductBySkuAsync(string sku)
    {
        return await _repository.FindOneAsync(p => p.SKU == sku);
    }
    
    public async Task<string> CreateProductAsync(Product product)
    {
        // Validate product
        if (string.IsNullOrEmpty(product.Name))
        {
            throw new ArgumentException("Product name is required");
        }
        
        if (product.Price <= 0)
        {
            throw new ArgumentException("Product price must be greater than zero");
        }
        
        // Check if SKU already exists
        if (!string.IsNullOrEmpty(product.SKU))
        {
            var existingProduct = await _repository.FindOneAsync(p => p.SKU == product.SKU);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"Product with SKU {product.SKU} already exists");
            }
        }
        
        // Set audit fields
        product.CreatedDate = DateTime.UtcNow;
        # QFace MongoDB SDK Documentation

The QFace MongoDB SDK provides a comprehensive solution for working with MongoDB in .NET applications. This document covers installation, configuration, and usage examples for both standard and multi-tenant scenarios.

## Table of Contents

1. [Installation](#installation)
2. [Configuration](#configuration)
3. [Setup](#setup)
4. [Working with Documents](#working-with-documents)
5. [Repository Pattern](#repository-pattern)
6. [Custom Repositories](#custom-repositories)
7. [Multi-Tenant Support](#multi-tenant-support)
8. [Advanced Features](#advanced-features)
9. [Best Practices](#best-practices)
10. [Troubleshooting](#troubleshooting)

## Installation

Install the package via NuGet:

```bash
dotnet add package QFace.Sdk.MongoDb
```

Add the following namespaces to your files:

```csharp
using QFace.Sdk.MongoDb;
using QFace.Sdk.MongoDb.Models;
using QFace.Sdk.MongoDb.Repositories;
using QFace.Sdk.MongoDb.Services;
```

## Configuration

### appsettings.json

Add the following configuration to your `appsettings.json` file:

```json
{
  "MongoDb": {
    "ConnectionString": "mongodb://username:password@localhost:27017",
    "DatabaseName": "YourDatabaseName",
    "UseSsl": true,
    "ConnectionTimeoutSeconds": 30,
    "ConnectionPool": {
      "MaxSize": 100,
      "MinSize": 10,
      "MaxConnectionLifeTimeMinutes": 30,
      "WaitQueueTimeoutMilliseconds": 5000
    },
    "CollectionNaming": {
      "Strategy": "PluralCamelCase",
      "ForceLowerCase": true
    }
  }
}
```

### Collection Naming Strategies

Available strategies:

- `Raw`: Uses the class name as-is (e.g., "Product")
- `CamelCase`: Converts to camelCase (e.g., "product")
- `Plural`: Pluralizes the class name (e.g., "Products")
- `PluralCamelCase`: Pluralizes and converts to camelCase (e.g., "products")

## Setup

### Basic Setup

Register MongoDB services in your `Program.cs` or `Startup.cs`:

```csharp
// Using configuration from appsettings.json
builder.Services.AddMongoDb(builder.Configuration);

// Register repositories for your documents
builder.Services.AddMongoRepository<Product>();
builder.Services.AddMongoRepository<Category>();
builder.Services.AddMongoRepository<Order>();
```

### Using Custom Database Connection

```csharp
// With explicit connection string and database name
builder.Services.AddMongoDb(
    "mongodb://username:password@localhost:27017", 
    "YourDatabaseName");
```

### Custom Connection Implementation

For more control over the connection process:

```csharp
builder.Services.Configure<MongoDbOptions>(options => {
    options.ConnectionString = "mongodb://username:password@localhost:27017";
    options.DatabaseName = "YourDatabaseName";
    options.UseSsl = true;
    options.ConnectionTimeoutSeconds = 30;
    options.ConnectionPool = new ConnectionPoolOptions {
        MaxSize = 100,
        MinSize = 10
    };
});

builder.Services.AddSingleton<IMongoDbClientFactory, MongoDbClientFactory>();
builder.Services.AddSingleton<IMongoClient>(sp => 
    sp.GetRequiredService<IMongoDbClientFactory>().GetClient());
builder.Services.AddSingleton<IMongoDatabase>(sp => 
    sp.GetRequiredService<IMongoDbClientFactory>().GetDatabase());
```

## Working with Documents

### Creating Document Models

Create your document classes by inheriting from `BaseDocument`:

```csharp
public class Product : BaseDocument
{
    public string Name { get; set; }
    public string Description { get; set; }
    public decimal Price { get; set; }
    public string Category { get; set; }
    public List<string> Tags { get; set; } = new();
    public int StockQuantity { get; set; }
    
    // Nested document
    public ProductDetails Details { get; set; } = new();
}

public class ProductDetails
{
    public string Manufacturer { get; set; }
    public string Model { get; set; }
    public string SKU { get; set; }
    public Dictionary<string, string> Specifications { get; set; } = new();
}
```

### Base Document Properties

The `BaseDocument` class includes the following properties automatically:

```csharp
// Unique identifier
[BsonId]
[BsonRepresentation(BsonType.ObjectId)]
public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

// Creation timestamp
[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

// Creator identifier
public string CreatedBy { get; set; } = string.Empty;

// Last modification timestamp
[BsonDateTimeOptions(Kind = DateTimeKind.Utc)]
public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

// Last modifier identifier
public string LastModifiedBy { get; set; } = string.Empty;

// Active status flag (for soft delete)
public bool IsActive { get; set; } = true;
```

## Repository Pattern

### Using the Default Repository

Inject and use `IMongoRepository<T>` in your services:

```csharp
public class ProductService
{
    private readonly IMongoRepository<Product> _repository;
    
    public ProductService(IMongoRepository<Product> repository)
    {
        _repository = repository;
    }
    
    // Get all products
    public async Task<List<Product>> GetAllProductsAsync(CancellationToken cancellationToken = default)
    {
        var products = await _repository.GetAllAsync(cancellationToken: cancellationToken);
        return products.ToList();
    }
    
    // Get a product by ID
    public async Task<Product> GetProductByIdAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }
    
    // Find products by a condition
    public async Task<List<Product>> FindProductsByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        var products = await _repository.FindAsync(p => p.Tags.Contains(tag), cancellationToken: cancellationToken);
        return products.ToList();
    }
    
    // Create a new product
    public async Task CreateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        await _repository.InsertOneAsync(product, cancellationToken);
    }
    
    // Update a product
    public async Task<bool> UpdateProductAsync(Product product, CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateAsync(product, cancellationToken);
    }
    
    // Delete a product (soft delete)
    public async Task<bool> SoftDeleteProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.SoftDeleteByIdAsync(id, cancellationToken);
    }
    
    // Delete a product (hard delete)
    public async Task<bool> HardDeleteProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteByIdAsync(id, cancellationToken);
    }
    
    // Restore a soft-deleted product
    public async Task<bool> RestoreProductAsync(string id, CancellationToken cancellationToken = default)
    {
        return await _repository.RestoreByIdAsync(id, cancellationToken);
    }
}
```

### Complete Repository Interface

The `IMongoRepository<T>` interface includes the following methods:

```csharp
// Get all documents
Task<IEnumerable<TDocument>> GetAllAsync(bool includeInactive = false, CancellationToken cancellationToken = default);

// Get a document by ID
Task<TDocument> GetByIdAsync(string id, CancellationToken cancellationToken = default);

// Find a single document by a condition
Task<TDocument> FindOneAsync(Expression<Func<TDocument, bool>> filterExpression, bool includeInactive = false, CancellationToken cancellationToken = default);

// Find documents by a condition
Task<IEnumerable<TDocument>> FindAsync(Expression<Func<TDocument, bool>> filterExpression, bool includeInactive = false, CancellationToken cancellationToken = default);

// Insert a single document
Task InsertOneAsync(TDocument document, CancellationToken cancellationToken = default);

// Insert multiple documents
Task InsertManyAsync(IEnumerable<TDocument> documents, CancellationToken cancellationToken = default);

// Update a document
Task<bool> UpdateAsync(TDocument document, CancellationToken cancellationToken = default);

// Replace a document
Task<bool> ReplaceOneAsync(TDocument document, CancellationToken cancellationToken = default);

// Delete a document (hard delete)
Task<bool> DeleteByIdAsync(string id, CancellationToken cancellationToken = default);

// Soft delete a document
Task<bool> SoftDeleteByIdAsync(string id, CancellationToken cancellationToken = default);

// Restore a soft-deleted document
Task<bool> RestoreByIdAsync(string id, CancellationToken cancellationToken = default);

// Create indexes
Task CreateIndexesAsync(CancellationToken cancellationToken = default);
```

## Custom Repositories

### Creating Custom Repositories

Create custom repositories by extending `MongoRepository<T>`:

```csharp
public class ProductRepository : MongoRepository<Product>
{
    public ProductRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger<ProductRepository> logger)
        : base(database, collectionName, logger)
    {
    }
    
    // Override to create custom indexes
    public override async Task CreateIndexesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            // Define indexes
            var nameIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Name),
                new CreateIndexOptions { Background = true, Name = "name_idx" }
            );
            
            var categoryIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Category),
                new CreateIndexOptions { Background = true, Name = "category_idx" }
            );
            
            var priceIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Price),
                new CreateIndexOptions { Background = true, Name = "price_idx" }
            );
            
            var tagsIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Tags),
                new CreateIndexOptions { Background = true, Name = "tags_idx" }
            );
            
            var textIndex = new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Text(p => p.Name).Text(p => p.Description),
                new CreateIndexOptions { Background = true, Name = "text_idx" }
            );
            
            // Create all indexes
            await _collection.Indexes.CreateManyAsync(
                new[] { nameIndex, categoryIndex, priceIndex, tagsIndex, textIndex },
                cancellationToken);
            
            _logger.LogInformation("Created indexes for product collection");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating indexes for product collection");
        }
    }
    
    // Custom query methods
    
    // Find products by price range
    public async Task<List<Product>> FindByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Gte(p => p.Price, minPrice),
                Builders<Product>.Filter.Lte(p => p.Price, maxPrice),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            return await _collection.Find(filter)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Price))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding products by price range {MinPrice} to {MaxPrice}", 
                minPrice, maxPrice);
            return new List<Product>();
        }
    }
    
    // Find products by category with pagination
    public async Task<(List<Product> Products, long TotalCount)> FindByCategoryPaginatedAsync(
        string category,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Eq(p => p.Category, category),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            var totalCount = await _collection.CountDocumentsAsync(filter, 
                new CountOptions(), cancellationToken);
            
            var products = await _collection.Find(filter)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Name))
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);
                
            return (products, totalCount);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding products by category {Category} with pagination", category);
            return (new List<Product>(), 0);
        }
    }
    
    // Search products by text
    public async Task<List<Product>> SearchProductsAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Text(searchText),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            return await _collection.Find(filter)
                .Sort(Builders<Product>.Sort.MetaTextScore("score"))
                .Limit(50)
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products for text {SearchText}", searchText);
            return new List<Product>();
        }
    }
    
    // Update stock quantity atomically
    public async Task<bool> UpdateStockQuantityAsync(
        string productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .Set(p => p.StockQuantity, quantity)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock quantity for product {ProductId}", productId);
            return false;
        }
    }
    
    // Increment stock quantity (useful for inventory adjustments)
    public async Task<bool> IncrementStockQuantityAsync(
        string productId, 
        int amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .Inc(p => p.StockQuantity, amount)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing stock quantity for product {ProductId}", productId);
            return false;
        }
    }
    
    // Add a tag to a product
    public async Task<bool> AddTagAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .AddToSet(p => p.Tags, tag)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tag {Tag} to product {ProductId}", tag, productId);
            return false;
        }
    }
    
    // Remove a tag from a product
    public async Task<bool> RemoveTagAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
            var update = Builders<Product>.Update
                .Pull(p => p.Tags, tag)
                .Set(p => p.LastModifiedDate, DateTime.UtcNow);
                
            var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
            
            return result.IsAcknowledged && result.ModifiedCount > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing tag {Tag} from product {ProductId}", tag, productId);
            return false;
        }
    }
}
```

### Registering Custom Repositories

Register custom repositories in your startup:

```csharp
// Register custom repository
builder.Services.AddMongoRepository<Product, ProductRepository>();

// Register with custom collection name
builder.Services.AddMongoRepository<Product, ProductRepository>("custom_products");
```

### Using Custom Repositories in Services

```csharp
public class EnhancedProductService
{
    private readonly ProductRepository _repository;
    
    public EnhancedProductService(ProductRepository repository)
    {
        _repository = repository;
    }
    
    // Use custom repository methods
    public async Task<List<Product>> GetProductsByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        return await _repository.FindByPriceRangeAsync(minPrice, maxPrice, cancellationToken);
    }
    
    public async Task<(List<Product> Products, long TotalCount, int TotalPages)> GetProductsByCategoryPaginatedAsync(
        string category,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var result = await _repository.FindByCategoryPaginatedAsync(
            category, page, pageSize, cancellationToken);
            
        var totalPages = (int)Math.Ceiling(result.TotalCount / (double)pageSize);
        
        return (result.Products, result.TotalCount, totalPages);
    }
    
    public async Task<List<Product>> SearchProductsAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        return await _repository.SearchProductsAsync(searchText, cancellationToken);
    }
    
    // Inventory management
    public async Task<bool> UpdateInventoryAsync(
        string productId, 
        int newQuantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateStockQuantityAsync(productId, newQuantity, cancellationToken);
    }
    
    public async Task<bool> AddInventoryAsync(
        string productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.IncrementStockQuantityAsync(productId, quantity, cancellationToken);
    }
    
    public async Task<bool> RemoveInventoryAsync(
        string productId, 
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.IncrementStockQuantityAsync(productId, -quantity, cancellationToken);
    }
    
    // Tag management
    public async Task<bool> AddTagToProductAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        return await _repository.AddTagAsync(productId, tag, cancellationToken);
    }
    
    public async Task<bool> RemoveTagFromProductAsync(
        string productId, 
        string tag,
        CancellationToken cancellationToken = default)
    {
        return await _repository.RemoveTagAsync(productId, tag, cancellationToken);
    }
}
```

## Multi-Tenant Support

### Setting Up Multi-Tenant Support

First, create a tenant service to determine the current tenant:

```csharp
// Create a tenant service interface
public interface ITenantService
{
    string GetCurrentTenantId();
}

// Implement the tenant service for your application
public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    
    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }
    
    public string GetCurrentTenantId()
    {
        // Get tenant ID from header
        var tenantId = _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault();
        
        // Or from claim
        if (string.IsNullOrEmpty(tenantId))
        {
            tenantId = _httpContextAccessor.HttpContext?.User.FindFirstValue("TenantId");
        }
        
        // Fall back to default if not found
        return tenantId ?? "default";
    }
}

// Register the tenant service
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddHttpContextAccessor();
```

### Registering Multi-Tenant Repositories

```csharp
// Register multi-tenant repositories
builder.Services.AddMultiTenantRepository<Product>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());
    
builder.Services.AddMultiTenantRepository<Order>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());
    
// With custom database name format (default is "{0}_db")
builder.Services.AddMultiTenantRepository<Customer>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId(),
    databaseNameFormat: "tenant_{0}");
    
// With custom collection name
builder.Services.AddMultiTenantRepository<User>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId(),
    collectionName: "users");
```

### Creating Custom Multi-Tenant Repositories

```csharp
public class TenantProductRepository : MultiTenantRepository<Product>
{
    public TenantProductRepository(
        IMongoDbProvider dbProvider,
        string collectionName,
        string tenantId,
        ILogger<TenantProductRepository> logger,
        string databaseNameFormat = "{0}_db")
        : base(dbProvider, collectionName, tenantId, logger, databaseNameFormat)
    {
    }
    
    // Custom methods similar to the regular repository example
    // Replace _collection with Collection property
    
    public async Task<List<Product>> FindByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var filter = Builders<Product>.Filter.And(
                Builders<Product>.Filter.Gte(p => p.Price, minPrice),
                Builders<Product>.Filter.Lte(p => p.Price, maxPrice),
                Builders<Product>.Filter.Eq(p => p.IsActive, true)
            );
            
            return await Collection.Find(filter)
                .Sort(Builders<Product>.Sort.Ascending(p => p.Price))
                .ToListAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding products by price range {MinPrice} to {MaxPrice} for tenant {TenantId}", 
                minPrice, maxPrice, _tenantId);
            return new List<Product>();
        }
    }
    
    // Add other custom methods as needed
}
```

### Register and Use Custom Multi-Tenant Repository

```csharp
// Register custom multi-tenant repository
builder.Services.AddMultiTenantRepository<Product, TenantProductRepository>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());

// Use in a service
public class TenantProductService
{
    private readonly TenantProductRepository _repository;
    private readonly ITenantService _tenantService;
    
    public TenantProductService(
        TenantProductRepository repository,
        ITenantService tenantService)
    {
        _repository = repository;
        _tenantService = tenantService;
    }
    
    public async Task<List<Product>> GetProductsByPriceRangeAsync(
        decimal minPrice, 
        decimal maxPrice,
        CancellationToken cancellationToken = default)
    {
        return await _repository.FindByPriceRangeAsync(minPrice, maxPrice, cancellationToken);
    }
    
    // Other methods as needed
}
```

### Using the MongoDbProvider Directly

For more control over databases and collections:

```csharp
public class MultiDatabaseService
{
    private readonly IMongoDbProvider _dbProvider;
    private readonly ITenantService _tenantService;
    
    public MultiDatabaseService(
        IMongoDbProvider dbProvider,
        ITenantService tenantService)
    {
        _dbProvider = dbProvider;
        _tenantService = tenantService;
    }
    
    // Get collection from a specific database
    public async Task<List<Product>> GetProductsFromDatabaseAsync(
        string databaseName,
        CancellationToken cancellationToken = default)
    {
        var collection = _dbProvider.GetCollection<Product>("products", databaseName);
        return await collection.Find(_ => true).ToListAsync(cancellationToken);
    }
    
    // Get collection from tenant database
    public async Task<List<Product>> GetProductsForTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var collection = _dbProvider.GetTenantCollection<Product>(tenantId, "products");
        return await collection.Find(_ => true).ToListAsync(cancellationToken);
    }
    
    // Perform operations across multiple tenant databases
    public async Task<Dictionary<string, int>> GetProductCountsForAllTenantsAsync(
        List<string> tenantIds,
        CancellationToken cancellationToken = default)
    {
        var results = new Dictionary<string, int>();
        
        foreach (var tenantId in tenantIds)
        {
            var collection = _dbProvider.GetTenantCollection<Product>(tenantId, "products");
            var count = await collection.CountDocumentsAsync(FilterDefinition<Product>.Empty, null, cancellationToken);
            results[tenantId] = (int)count;
        }
        
        return results;
    }
}
```

## Advanced Features

### Working with Transactions

MongoDB supports transactions (for replica sets) when you need atomic operations:

```csharp
public class OrderService
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoRepository<Order> _orderRepository;
    private readonly IMongoRepository<Product> _productRepository;
    private readonly IMongoRepository<Customer> _customerRepository;
    
    public OrderService(
        IMongoClient mongoClient,
        IMongoRepository<Order> orderRepository,
        IMongoRepository<Product> productRepository,
        IMongoRepository<Customer> customerRepository)
    {
        _mongoClient = mongoClient;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
        _customerRepository = customerRepository;
    }
    
    public async Task<bool> CreateOrderWithInventoryUpdateAsync(Order order, string customerId)
    {
        using (var session = await _mongoClient.StartSessionAsync())
        {
            session.StartTransaction();
            
            try
            {
                // 1. Create order
                await _orderRepository.InsertOneAsync(order);
                
                // 2. Update inventory for each product
                foreach (var item in order.Items)
                {
                    var product = await _productRepository.GetByIdAsync(item.ProductId);
                    if (product == null || product.StockQuantity < item.Quantity)
                    {
                        // Not enough inventory
                        await session.AbortTransactionAsync();
                        return false;
                    }
                    
                    product.StockQuantity -= item.Quantity;
                    await _productRepository.UpdateAsync(product);
                }
                
                // 3. Update customer's order history
                var customer = await _customerRepository.GetByIdAsync(customerId);
                if (customer != null)
                {
                    customer.LastOrderDate = DateTime.UtcNow;
                    customer.TotalOrders++;
                    customer.TotalSpent += order.Total;
                    await _customerRepository.UpdateAsync(customer);
                }
                
                // Commit the transaction
                await session.CommitTransactionAsync();
                return true;
            }
            catch (Exception)
            {
                // If any operation fails, abort the transaction
                await session.AbortTransactionAsync();
                return false;
            }
        }
    }
}
```

### Using Aggregation Pipeline

For complex queries, you can use MongoDB's aggregation pipeline:

```csharp
public class SalesAnalyticsRepository : MongoRepository<Order>
{
    public SalesAnalyticsRepository(
        IMongoDatabase database,
        string collectionName,
        ILogger<SalesAnalyticsRepository> logger)
        : base(database, collectionName, logger)
    {
    }
    
    // Get sales by category
    public async Task<List<CategorySales>> GetSalesByCategoryAsync(
        DateTime startDate,
        DateTime endDate,
        CancellationToken cancellationToken = default)
    {
        try
        {
           var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "OrderDate", new BsonDocument
                    {
                        { "$gte", startDate },
                        { "$lte", endDate }
                    }
                },
                { "IsActive", true }
            }),
            new BsonDocument("$unwind", "$Items"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$Items.Category" },
                { "totalSales", new BsonDocument("$sum", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })) },
                { "count", new BsonDocument("$sum", 1) },
                { "averageOrderValue", new BsonDocument("$avg", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })) }
            }),
            new BsonDocument("$sort", new BsonDocument("totalSales", -1))
        };
        
        var results = await _collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
            
        return results.Select(doc => new CategorySales
        {
            Category = doc["_id"].AsString,
            TotalSales = doc["totalSales"].AsDecimal,
            Count = doc["count"].AsInt32,
            AverageOrderValue = doc["averageOrderValue"].AsDecimal
        }).ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting sales by category from {StartDate} to {EndDate}", 
            startDate, endDate);
        return new List<CategorySales>();
    }
}

// Get sales by day
public async Task<List<DailySales>> GetDailySalesAsync(
    DateTime startDate,
    DateTime endDate,
    CancellationToken cancellationToken = default)
{
    try
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument
            {
                { "OrderDate", new BsonDocument
                    {
                        { "$gte", startDate },
                        { "$lte", endDate }
                    }
                },
                { "IsActive", true }
            }),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", new BsonDocument
                    {
                        { "year", new BsonDocument("$year", "$OrderDate") },
                        { "month", new BsonDocument("$month", "$OrderDate") },
                        { "day", new BsonDocument("$dayOfMonth", "$OrderDate") }
                    }
                },
                { "totalSales", new BsonDocument("$sum", "$Total") },
                { "orderCount", new BsonDocument("$sum", 1) },
                { "averageOrderValue", new BsonDocument("$avg", "$Total") }
            }),
            new BsonDocument("$sort", new BsonDocument("_id.year", 1)
                .Add("_id.month", 1)
                .Add("_id.day", 1))
        };
        
        var results = await _collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
            
        return results.Select(doc => new DailySales
        {
            Date = new DateTime(
                doc["_id"]["year"].AsInt32,
                doc["_id"]["month"].AsInt32,
                doc["_id"]["day"].AsInt32),
            TotalSales = doc["totalSales"].AsDecimal,
            OrderCount = doc["orderCount"].AsInt32,
            AverageOrderValue = doc["averageOrderValue"].AsDecimal
        }).ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting daily sales from {StartDate} to {EndDate}", 
            startDate, endDate);
        return new List<DailySales>();
    }
}

// Get top selling products
public async Task<List<TopSellingProduct>> GetTopSellingProductsAsync(
    int limit = 10,
    CancellationToken cancellationToken = default)
{
    try
    {
        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument("IsActive", true)),
            new BsonDocument("$unwind", "$Items"),
            new BsonDocument("$group", new BsonDocument
            {
                { "_id", "$Items.ProductId" },
                { "name", new BsonDocument("$first", "$Items.ProductName") },
                { "totalQuantity", new BsonDocument("$sum", "$Items.Quantity") },
                { "totalRevenue", new BsonDocument("$sum", new BsonDocument("$multiply", new BsonArray { "$Items.Quantity", "$Items.UnitPrice" })) }
            }),
            new BsonDocument("$sort", new BsonDocument("totalQuantity", -1)),
            new BsonDocument("$limit", limit)
        };
        
        var results = await _collection.Aggregate<BsonDocument>(pipeline, cancellationToken: cancellationToken)
            .ToListAsync(cancellationToken);
            
        return results.Select(doc => new TopSellingProduct
        {
            ProductId = doc["_id"].AsString,
            ProductName = doc["name"].AsString,
            TotalQuantity = doc["totalQuantity"].AsInt32,
            TotalRevenue = doc["totalRevenue"].AsDecimal
        }).ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error getting top selling products");
        return new List<TopSellingProduct>();
    }
}