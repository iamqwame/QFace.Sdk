# QFace MongoDB SDK

The MongoDB SDK provides a robust and flexible way to interact with MongoDB databases in your .NET applications. It supports both single-tenant and multi-tenant scenarios, with a clean repository pattern that makes database operations easier to manage and test.

## Features

- Simple setup with dependency injection
- Repository pattern for cleaner code organization
- Base document model with common fields
- Automatic collection naming strategies
- Configurable MongoDB connection options
- Support for multi-tenant database scenarios
- Logging and error handling
- Soft delete support

## Getting Started

### Installation

Install the package via NuGet:

```bash
dotnet add package QFace.Sdk.MongoDb
```

### Configuration

Add MongoDB configuration to your `appsettings.json`:

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

### Basic Setup

Register MongoDB services in your `Program.cs` or `Startup.cs`:

```csharp
// Using configuration from appsettings.json
builder.Services.AddMongoDb(builder.Configuration);

// Or with explicit connection string and database name
builder.Services.AddMongoDb(
    "mongodb://username:password@localhost:27017",
    "YourDatabaseName");
```

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
}
```

### Using the Default Repository

The simplest way to get started is to register and use the default repository:

```csharp
// Register repository in Program.cs/Startup.cs
builder.Services.AddMongoRepository<Product>();

// Use in your service
public class ProductService
{
    private readonly IMongoRepository<Product> _repository;

    public ProductService(IMongoRepository<Product> repository)
    {
        _repository = repository;
    }

    public async Task<List<Product>> GetAllProductsAsync()
    {
        var products = await _repository.GetAllAsync();
        return products.ToList();
    }

    public async Task<Product> GetProductByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task CreateProductAsync(Product product)
    {
        await _repository.InsertOneAsync(product);
    }

    public async Task UpdateProductAsync(Product product)
    {
        await _repository.UpdateAsync(product);
    }

    public async Task DeleteProductAsync(string id, bool softDelete = true)
    {
        if (softDelete)
        {
            await _repository.SoftDeleteByIdAsync(id);
        }
        else
        {
            await _repository.DeleteByIdAsync(id);
        }
    }
}
```

## Creating Custom Repositories

For more specific operations, create a custom repository by extending `MongoRepository<T>`:

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
        var indexes = new List<CreateIndexModel<Product>>
        {
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Category),
                new CreateIndexOptions { Background = true }
            ),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Text(p => p.Name).Text(p => p.Description),
                new CreateIndexOptions { Background = true }
            ),
            new CreateIndexModel<Product>(
                Builders<Product>.IndexKeys.Ascending(p => p.Tags),
                new CreateIndexOptions { Background = true }
            )
        };

        await _collection.Indexes.CreateManyAsync(indexes, cancellationToken);
    }

    // Custom methods
    public async Task<List<Product>> FindByPriceRangeAsync(
        decimal minPrice,
        decimal maxPrice,
        CancellationToken cancellationToken = default)
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

    public async Task<List<Product>> SearchProductsAsync(
        string searchText,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Text(searchText),
            Builders<Product>.Filter.Eq(p => p.IsActive, true)
        );

        return await _collection.Find(filter)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> UpdateStockAsync(
        string productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.Eq(p => p.Id, productId);
        var update = Builders<Product>.Update
            .Set(p => p.StockQuantity, quantity)
            .Set(p => p.LastModifiedDate, DateTime.UtcNow);

        var result = await _collection.UpdateOneAsync(filter, update, null, cancellationToken);
        return result.IsAcknowledged && result.ModifiedCount > 0;
    }
}
```

Register the custom repository:

```csharp
// Register custom repository
builder.Services.AddMongoRepository<Product, ProductRepository>();
```

## Multi-Tenant Support

For applications that need to support multiple tenants with separate databases, you can use the multi-tenant features:

### Setting Up Multi-Tenant Support

First, you need a way to get the current tenant ID. This often comes from an HTTP header, a claim, or another service:

```csharp
// Create a tenant service
public interface ITenantService
{
    string GetCurrentTenantId();
}

public class TenantService : ITenantService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public TenantService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public string GetCurrentTenantId()
    {
        // Get tenant ID from header, claim, or other source
        return _httpContextAccessor.HttpContext?.Request.Headers["X-Tenant-ID"].FirstOrDefault()
               ?? "default";
    }
}

// Register tenant service
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddHttpContextAccessor();
```

Then register the multi-tenant repositories:

```csharp
// Register multi-tenant repository
builder.Services.AddMultiTenantRepository<Product>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());
```

### Creating Custom Multi-Tenant Repositories

You can also create custom multi-tenant repositories:

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

    // Custom methods
    public async Task<List<Product>> GetFeaturedProductsAsync(
        CancellationToken cancellationToken = default)
    {
        var filter = Builders<Product>.Filter.And(
            Builders<Product>.Filter.Eq(p => p.IsActive, true),
            Builders<Product>.Filter.Gt(p => p.StockQuantity, 0)
        );

        return await Collection.Find(filter)
            .SortByDescending(p => p.CreatedDate)
            .Limit(10)
            .ToListAsync(cancellationToken);
    }
}
```

Register it:

```csharp
// Register custom multi-tenant repository
builder.Services.AddMultiTenantRepository<Product, TenantProductRepository>(
    sp => sp.GetRequiredService<ITenantService>().GetCurrentTenantId());
```

## Advanced Features

### Collection Naming Strategies

The SDK supports different collection naming strategies:

- `Raw`: Uses the class name as-is
- `CamelCase`: Converts to camelCase
- `Plural`: Pluralizes the class name
- `PluralCamelCase`: Pluralizes and converts to camelCase

Configure the strategy in appsettings.json:

```json
"CollectionNaming": {
  "Strategy": "PluralCamelCase",
  "ForceLowerCase": true
}
```

### Working with Multiple Databases

You can work with multiple databases using the `IMongoDbProvider`:

```csharp
public class MultiDatabaseService
{
    private readonly IMongoDbProvider _dbProvider;

    public MultiDatabaseService(IMongoDbProvider dbProvider)
    {
        _dbProvider = dbProvider;
    }

    public async Task<List<Product>> GetProductsFromSpecificDatabaseAsync(
        string databaseName,
        CancellationToken cancellationToken = default)
    {
        var collection = _dbProvider.GetCollection<Product>("products", databaseName);
        return await collection.Find(_ => true).ToListAsync(cancellationToken);
    }

    public async Task<List<Product>> GetProductsForTenantAsync(
        string tenantId,
        CancellationToken cancellationToken = default)
    {
        var collection = _dbProvider.GetTenantCollection<Product>(tenantId, "products");
        return await collection.Find(_ => true).ToListAsync(cancellationToken);
    }
}
```

### Using Transactions

For operations that require transactions:

```csharp
public class TransactionalService
{
    private readonly IMongoClient _mongoClient;
    private readonly IMongoRepository<Order> _orderRepository;
    private readonly IMongoRepository<Product> _productRepository;

    public TransactionalService(
        IMongoClient mongoClient,
        IMongoRepository<Order> orderRepository,
        IMongoRepository<Product> productRepository)
    {
        _mongoClient = mongoClient;
        _orderRepository = orderRepository;
        _productRepository = productRepository;
    }

    public async Task CreateOrderWithInventoryUpdateAsync(Order order)
    {
        using var session = await _mongoClient.StartSessionAsync();
        session.StartTransaction();

        try
        {
            // Create order
            await _orderRepository.InsertOneAsync(order);

            // Update inventory for each product
            foreach (var item in order.Items)
            {
                var product = await _productRepository.GetByIdAsync(item.ProductId);
                product.StockQuantity -= item.Quantity;
                await _productRepository.UpdateAsync(product);
            }

            // Commit transaction
            await session.CommitTransactionAsync();
        }
        catch
        {
            // Abort transaction on error
            await session.AbortTransactionAsync();
            throw;
        }
    }
}
```

## Best Practices

- **Use the repository pattern**: Avoid direct access to collections when possible
- **Create indexes**: Always define appropriate indexes for your queries
- **Use soft deletes**: Prefer `SoftDeleteByIdAsync` over `DeleteByIdAsync` for most use cases
- **Implement paging**: For large collections, always use paging with `.Skip().Limit()`
- **Handle errors**: Always catch and log exceptions from MongoDB operations
- **Use the correct data types**: Be mindful of data types and serialization options
- **Consider multi-tenant design**: For SaaS applications, use the multi-tenant repositories
- **Use transactions when needed**: For operations that need to be atomic, use transactions

## Common Pitfalls

- **Missing indexes**: Performance can degrade quickly without proper indexes
- **Large documents**: Keep documents under MongoDB's 16MB document size limit
- **Unbounded arrays**: Avoid arrays that can grow without limits
- **Not handling errors**: MongoDB operations can fail for many reasons
- **Mixing tenant data**: In multi-tenant scenarios, ensure data stays isolated
- **Ignoring schema evolution**: Plan for how document structures might change over time
