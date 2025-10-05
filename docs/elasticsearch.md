"CustomerName": "Test Customer",
"CustomerPhone": "0247761922",
"ReferenceNumber": "901355427",
"TellerId": "test",
"TellerName": "Test Teller",
"BankBranch": "Main Branch",
"Notes": "First-time applicant",
"FormReferenceId": "901355427",
"FormPinCode": "TP5CUX8TNZ",
"Category": "Application Forms Fees",
"BankTransactionId": "TEST-20251005135542",
"ReferenceApplicationTypeId": "15",
"PaymentItemCategory": "Application Forms Fees",
"PaymentItemTags": [
"undergraduate"
],
"IpAddress": "127.0.0.1",
"Id": "e3d77acb5fc748ceb76710ab",
"CreatedDate": "2025-10-05T13:55:42.755858Z",
"CreatedBy": "",
"LastModifiedDate": "2025-10-05T13:55:42.755858Z",
"LastModifiedBy": "",
"IsActive": true
}
dbug: QFace.Sdk.RabbitMq.Actors.RabbitMqConsumerActor[0]
[RabbitMQ] Creating instance of consumer type 'UMaTAdmission.General.Consumers.AdmissionConsumer'
fail: QFace.Sdk.RabbitMq.Actors.RabbitMqConsumerActor[0]
[RabbitMQ] Error creating or invoking consumer 'AdmissionConsumer'
System.InvalidOperationException: Unable to resolve service for type 'UMaTAdmission.Providers.Shared.Repositories.ApplicantRepository' while attempting to activate 'UMaTAdmission.General.Consumers.AdmissionConsumer'.
at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteFactory.CreateArgumentCallSites(ServiceIdentifier serviceIdentifier, Type implementationType, CallSiteChain callSiteChain, ParameterInfo[] parameters, Boolean throwIfCallSiteNotFound)
at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteFactory.CreateConstructorCallSite(ResultCache lifetime, ServiceIdentifier serviceIdentifier, Type implementationType, CallSiteChain callSiteChain)
at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteFactory.TryCreateExact(ServiceDescriptor descriptor, ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain, Int32 slot)
at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteFactory.TryCreateExact(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteFactory.CreateCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
at Microsoft.Extensions.DependencyInjection.ServiceLookup.CallSiteFactory.GetCallSite(ServiceIdentifier serviceIdentifier, CallSiteChain callSiteChain)
at Microsoft.Extensions.DependencyInjection.ServiceProvider.CreateServiceAccessor(ServiceIdentifier serviceIdentifier)
at System.Collections.Concurrent.ConcurrentDictionary`2.GetOrAdd(TKey key, Func`2 valueFactory)
at Microsoft.Extensions.DependencyInjection.ServiceProvider.GetService(ServiceIdentifier serviceIdentifier, ServiceProviderEngineScope serviceProviderEngineScope)
at Microsoft.Extensions.DependencyInjection.ServiceLookup.ServiceProviderEngineScope.GetService(Type serviceType)
at Microsoft.Extensions.DependencyInjection.ServiceProviderServiceExtensions.GetRequiredService(IServiceProvider provider, Type serviceType)
at QFace.Sdk.RabbitMq.Actors.RabbitMqConsumerActor.HandleConsumeMessage(ConsumeMessage message)
fail: QFace.Sdk.RabbitMq.Actors.RabbitMqConsumerActor[0]
[RabbitMQ] Error processing message from queue 'umat.admissions.payment.queue': {
"TransactionId": "506adce0-7fd3-4e00-b406-0bb0fa6e86c2",
"PaymentItemId": "68c89d0691741d30eeafa280",
"PaymentItemName": "Undergraduate Admission Form (Ghanaian)",
"PaymentItemDescription": "UG Admission \u2013 Ghanaian",
"OriginalItemId": 15,
# QFace OpenSearch SDK Documentation

The QFace OpenSearch SDK provides a comprehensive solution for working with OpenSearch and Elasticsearch in .NET applications. This document covers installation, configuration, and usage examples for both standard and advanced scenarios.

**✨ Now supports both OpenSearch and Elasticsearch!** Perfect for DigitalOcean managed OpenSearch, AWS OpenSearch Service, and self-hosted deployments.

## 🚀 **Version 2.0 - OpenSearch Compatible!**

### ⬆️ **Migration from Version 1.x**

If you're upgrading from the previous Elasticsearch-only version:

```bash
# Old package (v1.x)
dotnet remove package QFace.Sdk.Elasticsearch

# New package (v2.x)
dotnet add package QFace.Sdk.OpenSearch
```

**✅ Your code remains the same!** The API is fully backward compatible.

### 🎆 **What's New in 2.0**

- ✅ **OpenSearch Native Support** - Uses OpenSearch.Client instead of deprecated NEST
- ✅ **DigitalOcean Ready** - Optimized for managed OpenSearch services
- ✅ **Backward Compatible** - All existing code works unchanged
- ✅ **Modern Dependencies** - Latest OpenSearch client libraries
- ✅ **Enhanced Security** - Better SSL and authentication handling

## Table of Contents

1. [Installation](#installation)
2. [Configuration](#configuration)
3. [Setup](#setup)
4. [Working with Documents](#working-with-documents)
5. [Repository Pattern](#repository-pattern)
6. [Custom Repositories](#custom-repositories)
7. [Advanced Features](#advanced-features)
8. [Best Practices](#best-practices)
9. [Troubleshooting](#troubleshooting)

## Installation

Install the package via NuGet:

```bash
dotnet add package QFace.Sdk.OpenSearch
```

**Note:** Package name changed from `QFace.Sdk.Elasticsearch` to `QFace.Sdk.OpenSearch` in version 2.0 for OpenSearch compatibility.

Add the following namespaces to your files:

```csharp
using QFace.Sdk.Elasticsearch;
using QFace.Sdk.Elasticsearch.Models;
using QFace.Sdk.Elasticsearch.Repositories;
using QFace.Sdk.Elasticsearch.Services;
using QFace.Sdk.Elasticsearch.Options;
```

## Configuration

### Configuration for DigitalOcean OpenSearch

For DigitalOcean managed OpenSearch (like your setup):

```json
{
  "Elasticsearch": {
    "NodeUrls": "https://db-opensearch-do-user-7791352-0.e.db.ondigitalocean.com:25060",
    "Username": "doadmin",
    "Password": "AVNS_vzu4qwuXrT819br9zgl",
    "DefaultIndexPrefix": "myapp",
    "EnableSsl": true,
    "ValidateSslCertificate": false,
    "ConnectionTimeoutSeconds": 30,
    "RequestTimeoutSeconds": 30,
    "MaxRetries": 3,
    "RetryTimeoutSeconds": 60,
    "EnableDebugMode": false,
    "Sniffing": {
      "Enabled": false,
      "IntervalSeconds": 60,
      "SniffOnStartup": false,
      "SniffOnConnectionFailure": false
    },
    "IndexNaming": {
      "Strategy": "PrefixedLowerCase",
      "UseTypeNameAsDefault": true,
      "IncludeEnvironmentName": true,
      "EnvironmentName": "prod"
    }
  }
}
```

### Configuration for Local Development

For local Elasticsearch or OpenSearch:

```json
{
  "Elasticsearch": {
    "NodeUrls": "http://localhost:9200",
    "DefaultIndexPrefix": "myapp",
    "Username": "",
    "Password": "",
    "ApiKey": "",
    "EnableSsl": true,
    "ValidateSslCertificate": true,
    "ConnectionTimeoutSeconds": 30,
    "RequestTimeoutSeconds": 30,
    "MaxRetries": 3,
    "RetryTimeoutSeconds": 60,
    "EnableDebugMode": false,
    "Sniffing": {
      "Enabled": false,
      "IntervalSeconds": 60,
      "SniffOnStartup": false,
      "SniffOnConnectionFailure": true
    },
    "IndexNaming": {
      "Strategy": "PrefixedLowerCase",
      "UseTypeNameAsDefault": true,
      "IncludeEnvironmentName": true,
      "EnvironmentName": "dev"
    }
  }
}
```

### Important Notes for OpenSearch

⚠️ **Critical Settings for Managed OpenSearch (DigitalOcean, AWS):**

1. **❌ Disable Sniffing**: `"Enabled": false` - Managed services don't support node discovery
2. **✅ Enable SSL**: `"EnableSsl": true` - Always use HTTPS for production
3. **⚠️ Certificate Validation**: `"ValidateSslCertificate": false` - May be required for managed services
4. **🔐 Use Basic Auth**: Username/password authentication is most reliable
5. **📋 Single Node**: Perfect configuration for managed services

### Supported Platforms

✅ **OpenSearch** (any version)  
✅ **DigitalOcean Managed OpenSearch**  
✅ **AWS OpenSearch Service**  
✅ **Elasticsearch** (7.x and 8.x)  
✅ **Self-hosted OpenSearch/Elasticsearch**  

### Index Naming Strategies

Available strategies:

- `Raw`: Uses the class name as-is (e.g., "Product")
- `LowerCase`: Converts to lowercase (e.g., "product")
- `PrefixedLowerCase`: Adds prefix and converts to lowercase (e.g., "myapp-dev-product")

## Setup

### Basic Setup

Register OpenSearch services in your `Program.cs` or `Startup.cs`:

```csharp
// Using configuration from appsettings.json
builder.Services.AddElasticsearch(builder.Configuration);

// Register repositories for your documents
builder.Services.AddElasticsearchRepository<Product>();
builder.Services.AddElasticsearchRepository<Category>();
builder.Services.AddElasticsearchRepository<Order>();
```

### DigitalOcean OpenSearch Setup

```csharp
// For DigitalOcean managed OpenSearch
builder.Services.AddElasticsearch(
    "https://db-opensearch-do-user-7791352-0.e.db.ondigitalocean.com:25060",
    "myapp");

// Configure authentication
builder.Services.Configure<ElasticsearchOptions>(options => {
    options.Username = "doadmin";
    options.Password = "AVNS_vzu4qwuXrT819br9zgl";
    options.EnableSsl = true;
    options.ValidateSslCertificate = false;
    options.Sniffing.Enabled = false;
});
```

### Using Custom Connection

```csharp
// With explicit connection string and prefix
builder.Services.AddElasticsearch(
    "http://localhost:9200",
    "myapp");
```

### Advanced Setup

For more control over the connection process:

```csharp
// Register services manually
builder.Services.Configure<ElasticsearchOptions>(options => {
    options.NodeUrls = "http://localhost:9200";
    options.DefaultIndexPrefix = "myapp";
    options.EnableSsl = true;
    options.ConnectionTimeoutSeconds = 30;
});

builder.Services.AddSingleton<IElasticsearchClientFactory, ElasticsearchClientFactory>();
builder.Services.AddSingleton<IOpenSearchClient>(sp =>
    sp.GetRequiredService<IElasticsearchClientFactory>().GetClient());
builder.Services.AddSingleton<IIndexNamingService, IndexNamingService>();

// Register with automatic scanning for repositories and documents
builder.Services.AddElasticsearch(
    builder.Configuration,
    "Elasticsearch",
    assembliesToScan: new[] { Assembly.GetExecutingAssembly() });

// Validate that all used repositories are registered
builder.Services.ValidateElasticsearchRepositories();
```

## Working with Documents

### Creating Document Models

Create your document classes by inheriting from `EsBaseDocument`:

```csharp
public class Product : EsBaseDocument
{
    [Text(Analyzer = "standard")]
    public string Name { get; set; }

    [Text(Analyzer = "standard")]
    public string Description { get; set; }

    [Number(NumberType.Double)]
    public decimal Price { get; set; }

    [Keyword]
    public string Category { get; set; }

    [Keyword]
    public List<string> Tags { get; set; } = new();

    [Number(NumberType.Integer)]
    public int StockQuantity { get; set; }

    // Nested document
    [Object]
    public ProductDetails Details { get; set; } = new();
}

public class ProductDetails
{
    [Keyword]
    public string Manufacturer { get; set; }

    [Keyword]
    public string Model { get; set; }

    [Keyword]
    public string SKU { get; set; }

    [Object]
    public Dictionary<string, string> Specifications { get; set; } = new();
}
```

### Base Document Properties

The `EsBaseDocument` class includes the following properties automatically:

```csharp
// Unique identifier
[Keyword]
public string Id { get; set; } = Guid.NewGuid().ToString();

// Creation timestamp
[Date]
public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

// Creator identifier
[Keyword]
public string CreatedBy { get; set; } = string.Empty;

// Last modification timestamp
[Date]
public DateTime LastModifiedDate { get; set; } = DateTime.UtcNow;

// Last modifier identifier
[Keyword]
public string LastModifiedBy { get; set; } = string.Empty;

// Active status flag (for soft delete)
[Boolean]
public bool IsActive { get; set; } = true;

// Internal score from Elasticsearch for query results
[Ignore]
public double? Score { get; set; }
```

## Repository Pattern

### Using the Default Repository

Inject and use `IElasticsearchRepository<T>` in your services:

```csharp
public class ProductService
{
    private readonly IElasticsearchRepository<Product> _repository;

    public ProductService(IElasticsearchRepository<Product> repository)
    {
        _repository = repository;
    }

    // Get all products
    public async Task<IEnumerable<Product>> GetAllProductsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetAllAsync(cancellationToken: cancellationToken);
    }

    // Get a product by ID
    public async Task<Product> GetProductByIdAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetByIdAsync(id, cancellationToken);
    }

    // Search for products
    public async Task<(IEnumerable<Product> Documents, long Total)> SearchProductsAsync(
        string searchText,
        string[] fields = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _repository.SearchAsync(
            searchText,
            fields,
            page,
            pageSize,
            cancellationToken: cancellationToken);
    }

    // Create a new product
    public async Task<bool> CreateProductAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        return await _repository.IndexAsync(product, cancellationToken: cancellationToken);
    }

    // Update a product
    public async Task<bool> UpdateProductAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateAsync(product, cancellationToken: cancellationToken);
    }

    // Delete a product (soft delete)
    public async Task<bool> SoftDeleteProductAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.SoftDeleteAsync(id, cancellationToken: cancellationToken);
    }

    // Delete a product (hard delete)
    public async Task<bool> DeleteProductAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.DeleteAsync(id, cancellationToken: cancellationToken);
    }

    // Restore a soft-deleted product
    public async Task<bool> RestoreProductAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _repository.RestoreAsync(id, cancellationToken: cancellationToken);
    }
}
```

### Complete Repository Interface

The `IElasticsearchRepository<T>` interface includes the following methods:

```csharp
// Get all documents
Task<IEnumerable<TDocument>> GetAllAsync(
    bool includeInactive = false,
    CancellationToken cancellationToken = default);

// Get a document by ID
Task<TDocument> GetByIdAsync(
    string id,
    CancellationToken cancellationToken = default);

// Search documents
Task<(IEnumerable<TDocument> Documents, long Total)> SearchAsync(
    string searchText,
    string[] fields = null,
    int page = 1,
    int pageSize = 20,
    bool includeInactive = false,
    CancellationToken cancellationToken = default);

// Query documents using a query descriptor
Task<(IEnumerable<TDocument> Documents, long Total)> QueryAsync(
    Func<QueryContainerDescriptor<TDocument>, QueryContainer> queryDescriptor,
    int page = 1,
    int pageSize = 20,
    bool includeInactive = false,
    CancellationToken cancellationToken = default);

// Filter documents using a LINQ expression
Task<(IEnumerable<TDocument> Documents, long Total)> FilterAsync(
    Expression<Func<TDocument, bool>> filterExpression,
    int page = 1,
    int pageSize = 20,
    bool includeInactive = false,
    CancellationToken cancellationToken = default);

// Index a document
Task<bool> IndexAsync(
    TDocument document,
    Refresh refresh = Refresh.WaitFor,
    CancellationToken cancellationToken = default);

// Index multiple documents
Task<bool> IndexManyAsync(
    IEnumerable<TDocument> documents,
    Refresh refresh = Refresh.WaitFor,
    CancellationToken cancellationToken = default);

// Update a document
Task<bool> UpdateAsync(
    TDocument document,
    Refresh refresh = Refresh.WaitFor,
    CancellationToken cancellationToken = default);

// Update a document partially using a script
Task<bool> UpdatePartialAsync(
    string id,
    Func<ScriptDescriptor, IScript> scriptBuilder,
    Refresh refresh = Refresh.WaitFor,
    CancellationToken cancellationToken = default);

// Delete a document
Task<bool> DeleteAsync(
    string id,
    Refresh refresh = Refresh.WaitFor,
    CancellationToken cancellationToken = default);

// Soft delete a document by setting IsActive to false
Task<bool> SoftDeleteAsync(
    string id,
    Refresh refresh = Refresh.WaitFor,
    CancellationToken cancellationToken = default);

// Restore a soft-deleted document by setting IsActive to true
Task<bool> RestoreAsync(
    string id,
    Refresh refresh = Refresh.WaitFor,
    CancellationToken cancellationToken = default);

// Check if an index exists
Task<bool> IndexExistsAsync(
    CancellationToken cancellationToken = default);

// Create an index if it doesn't exist
Task<bool> CreateIndexAsync(
    Func<CreateIndexDescriptor, ICreateIndexRequest> mappingSelector = null,
    CancellationToken cancellationToken = default);

// Delete an index
Task<bool> DeleteIndexAsync(
    CancellationToken cancellationToken = default);

// Count documents
Task<long> CountAsync(
    Expression<Func<TDocument, bool>> filterExpression = null,
    bool includeInactive = false,
    CancellationToken cancellationToken = default);

// Execute an aggregation query
Task<TAggregate> AggregateAsync<TAggregate>(
    Func<AggregationContainerDescriptor<TDocument>, IAggregationContainer> aggregationSelector,
    Expression<Func<TDocument, bool>> filterExpression = null,
    bool includeInactive = false,
    CancellationToken cancellationToken = default) where TAggregate : class;
```

## Custom Repositories

### Creating Custom Repositories

Create custom repositories by extending `ElasticsearchRepository<T>`:

```csharp
public class ProductRepository : ElasticsearchRepository<Product>
{
    public ProductRepository(
        IOpenSearchClient client,
        string indexName,
        ILogger<ProductRepository> logger)
        : base(client, indexName, logger)
    {
    }

    // Create custom index mappings
    public override async Task<bool> CreateIndexAsync(
        Func<CreateIndexDescriptor, ICreateIndexRequest> mappingSelector = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var exists = await IndexExistsAsync(cancellationToken);
            if (exists)
            {
                return true; // Index already exists
            }

            // Custom index mapping
            var response = await _client.Indices.CreateAsync(IndexName, c => c
                .Map<Product>(m => m
                    .AutoMap() // Auto-map from attributes
                    .Properties(ps => ps
                        // Custom mapping for specific fields
                        .Text(t => t
                            .Name(p => p.Name)
                            .Analyzer("standard")
                            .Fields(fs => fs
                                .Keyword(k => k.Name("keyword"))))
                        .Text(t => t
                            .Name(p => p.Description)
                            .Analyzer("standard"))
                        .Keyword(k => k
                            .Name(p => p.Category))
                        .Number(n => n
                            .Name(p => p.Price)
                            .Type(NumberType.Double))
                        .Keyword(k => k
                            .Name(p => p.Tags))
                        .Number(n => n
                            .Name(p => p.StockQuantity)
                            .Type(NumberType.Integer))
                        .Object<ProductDetails>(o => o
                            .Name(p => p.Details)
                            .Properties(dps => dps
                                .Keyword(k => k.Name(d => d.Manufacturer))
                                .Keyword(k => k.Name(d => d.Model))
                                .Keyword(k => k.Name(d => d.SKU)))))),
                cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error creating index {IndexName}: {Error}",
                    IndexName, response.DebugInformation);
                return false;
            }

            _logger.LogInformation("Index {IndexName} created", IndexName);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating index {IndexName}", IndexName);
            return false;
        }
    }

    // Custom search method with additional filters
    public async Task<(IEnumerable<Product> Products, long Total)> SearchProductsAsync(
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
            // Build query
            Func<QueryContainerDescriptor<Product>, QueryContainer> queryBuilder = q =>
            {
                var queries = new List<QueryContainer>();

                // Add text search if provided
                if (!string.IsNullOrEmpty(searchText))
                {
                    queries.Add(q.MultiMatch(mm => mm
                        .Fields(f => f
                            .Field(p => p.Name, 2.0) // Boost name field
                            .Field(p => p.Description))
                        .Query(searchText)
                        .Type(TextQueryType.BestFields)
                        .Fuzziness(Fuzziness.Auto)));
                }

                // Add category filter if provided
                if (!string.IsNullOrEmpty(category))
                {
                    queries.Add(q.Term(t => t.Field(p => p.Category).Value(category)));
                }

                // Add price range filter if provided
                if (minPrice.HasValue)
                {
                    queries.Add(q.Range(r => r.Field(p => p.Price).GreaterThanOrEquals(minPrice.Value)));
                }

                if (maxPrice.HasValue)
                {
                    queries.Add(q.Range(r => r.Field(p => p.Price).LessThanOrEquals(maxPrice.Value)));
                }

                // Combine all queries
                if (queries.Count == 0)
                {
                    return q.MatchAll();
                }
                else if (queries.Count == 1)
                {
                    return queries[0];
                }
                else
                {
                    return q.Bool(b => b.Must(queries.ToArray()));
                }
            };

            // Execute query
            return await QueryAsync(queryBuilder, page, pageSize, false, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products");
            return (Enumerable.Empty<Product>(), 0);
        }
    }

    // Get popular products based on score
    public async Task<List<Product>> GetPopularProductsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var searchResponse = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Size(limit)
                .Sort(sort => sort.Descending("_score"))
                .Query(q => q
                    .Term(t => t.Field(p => p.IsActive).Value(true))),
                cancellationToken);

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Error getting popular products: {Error}",
                    searchResponse.DebugInformation);
                return new List<Product>();
            }

            return searchResponse.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting popular products");
            return new List<Product>();
        }
    }

    // Update stock with script
    public async Task<bool> UpdateStockAsync(
        string productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await UpdatePartialAsync(
                productId,
                s => s
                    .Source("ctx._source.stockQuantity = params.quantity")
                    .Params(p => p.Add("quantity", quantity)),
                Refresh.WaitFor,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating stock for product {ProductId}", productId);
            return false;
        }
    }

    // Increment stock (for inventory adjustments)
    public async Task<bool> IncrementStockAsync(
        string productId,
        int amount,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await UpdatePartialAsync(
                productId,
                s => s
                    .Source("ctx._source.stockQuantity += params.amount")
                    .Params(p => p.Add("amount", amount)),
                Refresh.WaitFor,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error incrementing stock for product {ProductId}", productId);
            return false;
        }
    }

    // Add a tag
    public async Task<bool> AddTagAsync(
        string productId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await UpdatePartialAsync(
                productId,
                s => s
                    .Source("if (!ctx._source.tags.contains(params.tag)) { ctx._source.tags.add(params.tag); }")
                    .Params(p => p.Add("tag", tag)),
                Refresh.WaitFor,
                cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding tag {Tag} to product {ProductId}", tag, productId);
            return false;
        }
    }

    // Remove a tag
    public async Task<bool> RemoveTagAsync(
        string productId,
        string tag,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await UpdatePartialAsync(
                productId,
                s => s
                    .Source("ctx._source.tags.removeIf(t -> t == params.tag)")
                    .Params(p => p.Add("tag", tag)),
                Refresh.WaitFor,
                cancellationToken);
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
builder.Services.AddElasticsearchRepository<Product, ProductRepository>();

// Register with custom index name
builder.Services.AddElasticsearchRepository<Product, ProductRepository>("custom_products");
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
    public async Task<(IEnumerable<Product> Products, long Total)> SearchProductsWithFiltersAsync(
        string searchText,
        string category = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _repository.SearchProductsAsync(
            searchText,
            category,
            minPrice,
            maxPrice,
            page,
            pageSize,
            cancellationToken);
    }

    public async Task<List<Product>> GetPopularProductsAsync(
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        return await _repository.GetPopularProductsAsync(limit, cancellationToken);
    }

    // Inventory management
    public async Task<bool> UpdateInventoryAsync(
        string productId,
        int newQuantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.UpdateStockAsync(productId, newQuantity, cancellationToken);
    }

    public async Task<bool> AddInventoryAsync(
        string productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.IncrementStockAsync(productId, quantity, cancellationToken);
    }

    public async Task<bool> RemoveInventoryAsync(
        string productId,
        int quantity,
        CancellationToken cancellationToken = default)
    {
        return await _repository.IncrementStockAsync(productId, -quantity, cancellationToken);
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

## Advanced Features

### Working with Aggregations

Elasticsearch excels at aggregations for analytics:

```csharp
public class ProductAnalyticsService
{
    private readonly IElasticsearchRepository<Product> _repository;

    public ProductAnalyticsService(IElasticsearchRepository<Product> repository)
    {
        _repository = repository;
    }

    // Get product count by category
    public async Task<Dictionary<string, long>> GetProductCountByCategoryAsync(
        CancellationToken cancellationToken = default)
    {
        var aggregationResults = await _repository.AggregateAsync<AggregateDictionary>(
            a => a.Terms("categories", t => t.Field(p => p.Category)),
            null,
            false,
            cancellationToken);

        var result = new Dictionary<string, long>();

        if (aggregationResults != null)
        {
            var categoryAggregation = aggregationResults.Terms("categories");
            foreach (var bucket in categoryAggregation.Buckets)
            {
                result[bucket.Key] = bucket.DocCount;
            }
        }

        return result;
    }

    // Get price statistics
    public async Task<PriceStatistics> GetPriceStatisticsAsync(
        string category = null,
        CancellationToken cancellationToken = default)
    {
        Expression<Func<Product, bool>> filter = null;
        if (!string.IsNullOrEmpty(category))
        {
            filter = p => p.Category == category;
        }

        var aggregationResults = await _repository.AggregateAsync<AggregateDictionary>(
            a => a.Stats("price_stats", s => s.Field(p => p.Price)),
            filter,
            false,
            cancellationToken);

        if (aggregationResults == null)
        {
            return new PriceStatistics();
        }

        var priceStats = aggregationResults.Stats("price_stats");

        return new PriceStatistics
        {
            Min = priceStats.Min ?? 0,
            Max = priceStats.Max ?? 0,
            Avg = priceStats.Average ?? 0,
            Sum = priceStats.Sum ?? 0,
            Count = priceStats.Count ?? 0
        };
    }

    // Price distribution histogram
    public async Task<Dictionary<double, long>> GetPriceHistogramAsync(
        double interval = 100,
        CancellationToken cancellationToken = default)
    {
        var aggregationResults = await _repository.AggregateAsync<AggregateDictionary>(
            a => a.Histogram("price_histogram", h => h
                .Field(p => p.Price)
                .Interval(interval)
                .MinimumDocumentCount(1)),
            null,
            false,
            cancellationToken);

        var result = new Dictionary<double, long>();

        if (aggregationResults != null)
        {
            var histogram = aggregationResults.Histogram("price_histogram");
            foreach (var bucket in histogram.Buckets)
            {
                result[bucket.Key] = bucket.DocCount;
            }
        }

        return result;
    }
}

public class PriceStatistics
{
    public double Min { get; set; }
    public double Max { get; set; }
    public double Avg { get; set; }
    public double Sum { get; set; }
    public long Count { get; set; }
}
```

### Search with Highlighting

Implement search with highlighting to emphasize matched terms:

```csharp
public class SearchService
{
    private readonly IOpenSearchClient _client;
    private readonly string _indexName;

    public SearchService(
        IOpenSearchClient client,
        IIndexNamingService indexNamingService)
    {
        _client = client;
        _indexName = indexNamingService.GetIndexName<Product>();
    }

    public async Task<SearchResults<Product>> SearchWithHighlightingAsync(
        string searchText,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(_indexName)
                .From((page - 1) * pageSize)
                .Size(pageSize)
                .Query(q => q
                    .Bool(b => b
                        .Must(mu => mu
                            .MultiMatch(mm => mm
                                .Fields(f => f
                                    .Field(p => p.Name, 2.0)
                                    .Field(p => p.Description))
                                .Query(searchText)
                                .Type(TextQueryType.BestFields)
                                .Fuzziness(Fuzziness.Auto)))
                        .Filter(fi => fi
                            .Term(t => t
                                .Field(p => p.IsActive)
                                .Value(true)))))
                .Highlight(h => h
                    .Fields(
                        f => f.Field(p => p.Name)
                            .PreTags("<em>")
                            .PostTags("</em>")
                            .NumberOfFragments(0), // Return full field
                        f => f.Field(p => p.Description)
                            .PreTags("<em>")
                            .PostTags("</em>")
                            .NumberOfFragments(3) // Return up to 3 fragments
                            .FragmentSize(150)) // Each fragment is 150 characters
                    .Encoder(HighlighterEncoder.Html)),
                cancellationToken);

            if (!response.IsValid)
            {
                throw new Exception($"Search error: {response.DebugInformation}");
            }

            var results = new SearchResults<Product>
            {
                TotalHits = response.Total,
                TotalPages = (int)Math.Ceiling(response.Total / (double)pageSize),
                CurrentPage = page,
                PageSize = pageSize,
                Hits = new List<SearchHit<Product>>()
            };

            foreach (var hit in response.Hits)
            {
                var searchHit = new SearchHit<Product>
                {
                    Document = hit.Source,
                    Score = hit.Score,
                    Highlights = new Dictionary<string, List<string>>()
                };

                // Add highlight fragments
                if (hit.Highlight != null)
                {
                    foreach (var highlight in hit.Highlight)
                    {
                        searchHit.Highlights[highlight.Key] = highlight.Value.ToList();
                    }
                }

                results.Hits.Add(searchHit);
            }

            return results;
        }
        catch (Exception ex)
        {
            throw new Exception($"Error searching with highlighting: {ex.Message}", ex);
        }
    }
}

public class SearchResults<T>
{
    public long TotalHits { get; set; }
    public int TotalPages { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }
    public List<SearchHit<T>> Hits { get; set; }
}

public class SearchHit<T>
{
    public T Document { get; set; }
    public double Score { get; set; }
    public Dictionary<string, List<string>> Highlights { get; set; }
}
```

### Working with Geo Data

Elasticsearch has great support for geo-spatial queries:

```csharp
public class Location : EsBaseDocument
{
    [Keyword]
    public string Name { get; set; }

    [Text]
    public string Description { get; set; }

    [Keyword]
    public string Type { get; set; }

    [GeoPoint]
    public GeoLocation Coordinates { get; set; }

    [Keyword]
    public List<string> Tags { get; set; } = new();

    [Number(NumberType.Double)]
    public double Rating { get; set; }
}

public class GeoLocation
{
    [Number(NumberType.Double)]
    public double Lat { get; set; }

    [Number(NumberType.Double)]
    public double Lon { get; set; }
}

public class LocationRepository : ElasticsearchRepository<Location>
{
    public LocationRepository(
        IOpenSearchClient client,
        string indexName,
        ILogger<LocationRepository> logger)
        : base(client, indexName, logger)
    {
    }

    // Find locations within a radius
    public async Task<List<Location>> FindLocationsWithinRadiusAsync(
        double latitude,
        double longitude,
        string distance,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.SearchAsync<Location>(s => s
                .Index(IndexName)
                .Size(100)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .GeoDistance(g => g
                                .Field(l => l.Coordinates)
                                .Distance(distance)
                                .Location(latitude, longitude)))
                        .Must(m => m
                            .Term(t => t
                                .Field(l => l.IsActive)
                                .Value(true)))))
                .Sort(sort => sort
                    .GeoDistance(g => g
                        .Field(l => l.Coordinates)
                        .Points(new GeoLocation { Lat = latitude, Lon = longitude })
                        .Order(SortOrder.Ascending)
                        .DistanceType(GeoDistanceType.Arc))),
                cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error finding locations within radius: {Error}",
                    response.DebugInformation);
                return new List<Location>();
            }

            return response.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding locations within radius");
            return new List<Location>();
        }
    }

    // Find locations within a bounding box
    public async Task<List<Location>> FindLocationsInBoundingBoxAsync(
        double topLeftLat,
        double topLeftLon,
        double bottomRightLat,
        double bottomRightLon,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.SearchAsync<Location>(s => s
                .Index(IndexName)
                .Size(100)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .GeoBoundingBox(g => g
                                .Field(l => l.Coordinates)
                                .TopLeft(topLeftLat, topLeftLon)
                                .BottomRight(bottomRightLat, bottomRightLon)))
                        .Must(m => m
                            .Term(t => t
                                .Field(l => l.IsActive)
                                .Value(true))))),
                cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error finding locations in bounding box: {Error}",
                    response.DebugInformation);
                return new List<Location>();
            }

            return response.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding locations in bounding box");
            return new List<Location>();
        }
    }

    // Find locations within a polygon
    public async Task<List<Location>> FindLocationsInPolygonAsync(
        List<GeoLocation> points,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.SearchAsync<Location>(s => s
                .Index(IndexName)
                .Size(100)
                .Query(q => q
                    .Bool(b => b
                        .Filter(f => f
                            .GeoPolygon(g => g
                                .Field(l => l.Coordinates)
                                .Points(points)))
                        .Must(m => m
                            .Term(t => t
                                .Field(l => l.IsActive)
                                .Value(true))))),
                cancellationToken);

            if (!response.IsValid)
            {
                _logger.LogError("Error finding locations in polygon: {Error}",
                    response.DebugInformation);
                return new List<Location>();
            }

            return response.Documents.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error finding locations in polygon");
            return new List<Location>();
        }
    }
}
```

### Bulk Operations

For better performance when dealing with large datasets:

```csharp
public class BulkOperationsService
{
    private readonly IOpenSearchClient _client;
    private readonly IIndexNamingService _indexNamingService;
    private readonly ILogger<BulkOperationsService> _logger;

    public BulkOperationsService(
        IOpenSearchClient client,
        IIndexNamingService indexNamingService,
        ILogger<BulkOperationsService> logger)
    {
        _client = client;
        _indexNamingService = indexNamingService;
        _logger = logger;
    }

    public async Task<BulkResult> BulkIndexDocumentsAsync<T>(
        List<T> documents,
        int batchSize = 1000,
        CancellationToken cancellationToken = default) where T : EsBaseDocument
    {
        try
        {
            var result = new BulkResult();
            var indexName = _indexNamingService.GetIndexName<T>();
            var batches = CreateBatches(documents, batchSize);

            foreach (var batch in batches)
            {
                var bulkRequest = new BulkRequest(indexName)
                {
                    Operations = new List<IBulkOperation>()
                };

                foreach (var document in batch)
                {
                    if (string.IsNullOrEmpty(document.Id))
                    {
                        document.Id = Guid.NewGuid().ToString();
                    }

                    document.CreatedDate = DateTime.UtcNow;
                    document.LastModifiedDate = DateTime.UtcNow;

                    bulkRequest.Operations.Add(new BulkIndexOperation<T>(document)
                    {
                        Id = document.Id
                    });
                }

                var response = await _client.BulkAsync(bulkRequest, cancellationToken);

                if (response.IsValid)
                {
                    result.SuccessCount += response.Items.Count(i => !i.IsValid);
                    result.TotalProcessed += response.Items.Count;
                }
                else
                {
                    result.ErrorCount += response.Items.Count(i => !i.IsValid);
                    result.TotalProcessed += response.Items.Count;
                    result.Errors.AddRange(response.ItemsWithErrors.Select(i => i.Error.Reason));

                    _logger.LogError("Bulk index errors: {Errors}",
                        string.Join(", ", response.ItemsWithErrors.Select(i => i.Error.Reason)));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk indexing");
            return new BulkResult
            {
                ErrorCount = documents.Count,
                TotalProcessed = documents.Count,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<BulkResult> BulkUpdateDocumentsAsync<T>(
        List<T> documents,
        int batchSize = 1000,
        CancellationToken cancellationToken = default) where T : EsBaseDocument
    {
        try
        {
            var result = new BulkResult();
            var indexName = _indexNamingService.GetIndexName<T>();
            var batches = CreateBatches(documents, batchSize);

            foreach (var batch in batches)
            {
                var bulkRequest = new BulkRequest(indexName)
                {
                    Operations = new List<IBulkOperation>()
                };

                foreach (var document in batch)
                {
                    if (string.IsNullOrEmpty(document.Id))
                    {
                        throw new ArgumentException("Document ID cannot be null for update operations");
                    }

                    document.LastModifiedDate = DateTime.UtcNow;

                    bulkRequest.Operations.Add(new BulkUpdateOperation<T, object>(
                        new Id(document.Id),
                        new BulkUpdateDescriptor<T, object>().Doc(document)));
                }

                var response = await _client.BulkAsync(bulkRequest, cancellationToken);

                if (response.IsValid)
                {
                    result.SuccessCount += response.Items.Count(i => i.IsValid);
                    result.TotalProcessed += response.Items.Count;
                }
                else
                {
                    result.ErrorCount += response.Items.Count(i => !i.IsValid);
                    result.TotalProcessed += response.Items.Count;
                    result.Errors.AddRange(response.ItemsWithErrors.Select(i => i.Error.Reason));

                    _logger.LogError("Bulk update errors: {Errors}",
                        string.Join(", ", response.ItemsWithErrors.Select(i => i.Error.Reason)));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk updating");
            return new BulkResult
            {
                ErrorCount = documents.Count,
                TotalProcessed = documents.Count,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    public async Task<BulkResult> BulkDeleteDocumentsAsync<T>(
        List<string> documentIds,
        int batchSize = 1000,
        CancellationToken cancellationToken = default) where T : EsBaseDocument
    {
        try
        {
            var result = new BulkResult();
            var indexName = _indexNamingService.GetIndexName<T>();
            var batches = CreateBatches(documentIds, batchSize);

            foreach (var batch in batches)
            {
                var bulkRequest = new BulkRequest(indexName)
                {
                    Operations = new List<IBulkOperation>()
                };

                foreach (var id in batch)
                {
                    bulkRequest.Operations.Add(new BulkDeleteOperation<T>(id));
                }

                var response = await _client.BulkAsync(bulkRequest, cancellationToken);

                if (response.IsValid)
                {
                    result.SuccessCount += response.Items.Count(i => i.IsValid);
                    result.TotalProcessed += response.Items.Count;
                }
                else
                {
                    result.ErrorCount += response.Items.Count(i => !i.IsValid);
                    result.TotalProcessed += response.Items.Count;
                    result.Errors.AddRange(response.ItemsWithErrors.Select(i => i.Error.Reason));

                    _logger.LogError("Bulk delete errors: {Errors}",
                        string.Join(", ", response.ItemsWithErrors.Select(i => i.Error.Reason)));
                }
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in bulk deleting");
            return new BulkResult
            {
                ErrorCount = documentIds.Count,
                TotalProcessed = documentIds.Count,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    private List<List<T>> CreateBatches<T>(List<T> items, int batchSize)
    {
        var batches = new List<List<T>>();

        for (int i = 0; i < items.Count; i += batchSize)
        {
            batches.Add(items.Skip(i).Take(batchSize).ToList());
        }

        return batches;
    }
}

public class BulkResult
{
    public int SuccessCount { get; set; } = 0;
    public int ErrorCount { get; set; } = 0;
    public int TotalProcessed { get; set; } = 0;
    public List<string> Errors { get; set; } = new List<string>();
}
```

## Best Practices

### Indexing

Properly indexing fields is crucial for performance:

```csharp
public override async Task<bool> CreateIndexAsync(
    Func<CreateIndexDescriptor, ICreateIndexRequest> mappingSelector = null,
    CancellationToken cancellationToken = default)
{
    try
    {
        var exists = await IndexExistsAsync(cancellationToken);
        if (exists)
        {
            return true; // Index already exists
        }

        // Rules for effective indexing:
        // 1. Use appropriate field mappings (text, keyword, date, etc.)
        // 2. Add field analyzers for text fields
        // 3. Set up multi-fields for both full-text search and exact matches
        // 4. Configure numeric fields with the right precision

        var response = await _client.Indices.CreateAsync(IndexName, c => c
            .Settings(s => s
                .Analysis(a => a
                    .Analyzers(an => an
                        .Standard("standard_analyzer", sa => sa
                            .StopWords("_english_"))
                        .Custom("text_analyzer", ca => ca
                            .Tokenizer("standard")
                            .Filters("lowercase", "asciifolding", "trim")))))
            .Map<Product>(m => m
                // Text fields
                .Properties(ps => ps
                    .Text(t => t
                        .Name(p => p.Name)
                        .Analyzer("text_analyzer")
                        .Fields(f => f
                            .Keyword(k => k.Name("keyword"))))
                    .Text(t => t
                        .Name(p => p.Description)
                        .Analyzer("text_analyzer"))

                    // Keyword fields
                    .Keyword(k => k
                        .Name(p => p.Category))
                    .Keyword(k => k
                        .Name(p => p.Tags))

                    // Numeric fields
                    .Number(n => n
                        .Name(p => p.Price)
                        .Type(NumberType.Double))
                    .Number(n => n
                        .Name(p => p.StockQuantity)
                        .Type(NumberType.Integer))

                    // Nested object
                    .Object<ProductDetails>(o => o
                        .Name(p => p.Details)
                        .Properties(dp => dp
                            .Keyword(k => k.Name(d => d.Manufacturer))
                            .Keyword(k => k.Name(d => d.Model))
                            .Keyword(k => k.Name(d => d.SKU))
                            .Object<Dictionary<string, string>>(do => do
                                .Name(d => d.Specifications)))))),
            cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError("Error creating index {IndexName}: {Error}",
                IndexName, response.DebugInformation);
            return false;
        }

        _logger.LogInformation("Index {IndexName} created", IndexName);
        return true;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating index {IndexName}", IndexName);
        return false;
    }
}
```

### Document Design

Best practices for Elasticsearch document design:

1. **Use Appropriate Field Types**: Match field types to how they'll be used

   ```csharp
   [Text(Analyzer = "standard")] // For full-text search
   public string Description { get; set; }

   [Keyword] // For exact matches, filtering, and aggregations
   public string Category { get; set; }

   [Date] // For date operations
   public DateTime PublishedDate { get; set; }

   [Number(NumberType.Double)] // For numeric operations
   public decimal Price { get; set; }

   [GeoPoint] // For geo queries
   public GeoLocation Location { get; set; }
   ```

2. **Use Multi-Fields for Different Access Patterns**

   ```csharp
   // In index mapping
   .Text(t => t
       .Name(p => p.Name)
       .Analyzer("standard")
       .Fields(f => f
           .Keyword(k => k.Name("keyword")))) // Access via Name.keyword
   ```

3. **Flatten Hierarchical Data When Appropriate**

   ```csharp
   // Simplified document for search
   public class ProductDocument : EsBaseDocument
   {
       public string Name { get; set; }
       public string Description { get; set; }
       public string Category { get; set; }
       public decimal Price { get; set; }

       // Flattened manufacturer info
       public string ManufacturerName { get; set; }
       public string ManufacturerCountry { get; set; }

       // Flattened variant info
       public List<string> AvailableColors { get; set; }
       public List<string> AvailableSizes { get; set; }
   }
   ```

4. **Use Nested Objects for Complex Structures**

   ```csharp
   public class Product : EsBaseDocument
   {
       public string Name { get; set; }
       public decimal Price { get; set; }

       [Nested]
       public List<Review> Reviews { get; set; }
   }

   public class Review
   {
       public string UserId { get; set; }
       public int Rating { get; set; }
       public string Text { get; set; }
       public DateTime Date { get; set; }
   }
   ```

5. **Optimize for Your Access Patterns**
   ```csharp
   // For high-volume time-based data
   .Settings(s => s
       .NumberOfShards(5)
       .NumberOfReplicas(1)
       .RefreshInterval(TimeSpan.FromSeconds(30)))
   ```

### Performance Optimization

Techniques for optimizing Elasticsearch performance:

1. **Use Source Filtering**: Retrieve only the fields you need

   ```csharp
   var response = await _client.SearchAsync<Product>(s => s
       .Index(IndexName)
       .Source(src => src
           .Includes(i => i
               .Fields(
                   f => f.Id,
                   f => f.Name,
                   f => f.Price)))
       .Query(q => q.Term(t => t.Field(f => f.Category).Value(category))));
   ```

2. **Use Pagination**: Always use From and Size for large result sets

   ```csharp
   var response = await _client.SearchAsync<Product>(s => s
       .Index(IndexName)
       .From((page - 1) * pageSize)
       .Size(pageSize)
       .Query(q => q.MatchAll()));
   ```

3. **Use the Scroll API**: For processing large datasets in batches

   ```csharp
   public async Task ProcessAllProductsAsync(
       Func<IEnumerable<Product>, Task> processor,
       CancellationToken cancellationToken = default)
   {
       var scrollTimeout = "2m";
       var batchSize = 1000;

       var response = await _client.SearchAsync<Product>(s => s
           .Index(IndexName)
           .Size(batchSize)
           .Scroll(scrollTimeout)
           .Query(q => q.MatchAll()),
           cancellationToken);

       while (response.Documents.Any() && !cancellationToken.IsCancellationRequested)
       {
           // Process batch
           await processor(response.Documents);

           // Get next batch
           response = await _client.ScrollAsync<Product>(
               scrollTimeout,
               response.ScrollId,
               cancellationToken);
       }

       // Clear scroll
       await _client.ClearScrollAsync(new ClearScrollRequest(response.ScrollId), cancellationToken);
   }
   ```

4. **Use the Bulk API**: For multiple document operations

   ```csharp
   // See the BulkOperationsService example above
   ```

5. **Use Query Cache**: For frequently used filtered queries
   ```csharp
   var response = await _client.SearchAsync<Product>(s => s
       .Index(IndexName)
       .RequestCache(true)
       .Query(q => q
           .Bool(b => b
               .Filter(f => f
                   .Term(t => t
                       .Field(p => p.Category)
                       .Value("electronics"))))));
   ```

## Troubleshooting

### OpenSearch-Specific Issues

**1. DigitalOcean OpenSearch Connection Problems**

```json
// Correct configuration for DigitalOcean
{
  "Elasticsearch": {
    "NodeUrls": "https://db-opensearch-do-user-7791352-0.e.db.ondigitalocean.com:25060",
    "Username": "doadmin",
    "Password": "AVNS_vzu4qwuXrT819br9zgl",
    "EnableSsl": true,
    "ValidateSslCertificate": false,  // ← Key for managed services
    "Sniffing": {
      "Enabled": false               // ← Must be disabled
    }
  }
}
```

**2. SSL Certificate Errors**

```csharp
// If you get SSL certificate validation errors:
"ValidateSslCertificate": false

// For production, try to use proper certificates:
"ValidateSslCertificate": true
```

**3. Authentication Failures**

```bash
# Verify credentials work with curl:
curl -u "doadmin:AVNS_vzu4qwuXrT819br9zgl" \
  "https://db-opensearch-do-user-7791352-0.e.db.ondigitalocean.com:25060"
```

**4. Connection Timeout Issues**

```json
{
  "Elasticsearch": {
    "ConnectionTimeoutSeconds": 60,  // Increase for slow networks
    "RequestTimeoutSeconds": 60,     // Increase for large operations
    "MaxRetries": 5                  // More retries for unstable connections
  }
}
```

**5. Debugging Connection Issues**

```json
{
  "Elasticsearch": {
    "EnableDebugMode": true  // Enable to see actual requests/responses
  }
}
```

### Common Errors

1. **Connection Issues**

   - Check the Elasticsearch server is running
   - Verify connection string and authentication details
   - Check network connectivity and firewall settings
   - Verify SSL certificate if using HTTPS

2. **Query Performance Problems**

   - Check if appropriate indices exist
   - Use query profiling to identify slow queries
   - Optimize mappings for your query patterns
   - Increase server resources if necessary

3. **Mapping Exceptions**

   - Ensure document fields match the index mapping
   - Use explicit mappings rather than dynamic mappings
   - Keep field names consistent across documents

4. **Out of Memory Errors**

   - Limit result set sizes with pagination
   - Use scroll API for processing large datasets
   - Increase JVM heap size on the Elasticsearch server
   - Optimize query filters to reduce memory usage

5. **Version Compatibility Issues**
   - Ensure NEST client version is compatible with your Elasticsearch server
   - Check breaking changes in Elasticsearch documentation
   - Test upgrades in a non-production environment first

### Debugging Elasticsearch Queries

```csharp
// Enable debug mode
options.EnableDebugMode = true;

// Log query parameters
public async Task<List<Product>> SearchProductsDebugAsync(
    string searchText,
    CancellationToken cancellationToken = default)
{
    try
    {
        var searchDescriptor = new SearchDescriptor<Product>()
            .Index(IndexName)
            .Query(q => q.Match(m => m.Field(f => f.Name).Query(searchText)));

        // Log the actual request being sent
        var requestJson = _client.RequestResponseSerializer.SerializeToString(searchDescriptor);
        _logger.LogDebug("Elasticsearch request: {Request}", requestJson);

        var response = await _client.SearchAsync<Product>(searchDescriptor, cancellationToken);

        if (!response.IsValid)
        {
            _logger.LogError("Search error: {Error}", response.DebugInformation);
            return new List<Product>();
        }

        return response.Documents.ToList();
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error searching products");
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

public class ElasticsearchServiceWithRetry
{
    private readonly IElasticsearchRepository<Product> _repository;
    private readonly ILogger<ElasticsearchServiceWithRetry> _logger;
    private readonly IAsyncPolicy _retryPolicy;

    public ElasticsearchServiceWithRetry(
        IElasticsearchRepository<Product> repository,
        ILogger<ElasticsearchServiceWithRetry> logger)
    {
        _repository = repository;
        _logger = logger;

        // Create retry policy for transient errors
        _retryPolicy = Policy
            .Handle<ElasticsearchClientException>()
            .Or<TaskCanceledException>()
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                3, // Number of retries
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(exception,
                        "Elasticsearch operation failed. Retrying ({RetryCount}/3) after {RetryDelay}...",
                        retryCount, timeSpan);
                });
    }

    public async Task<Product> GetProductWithRetryAsync(
        string id,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
            await _repository.GetByIdAsync(id, cancellationToken));
    }

    public async Task<bool> IndexProductWithRetryAsync(
        Product product,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
            await _repository.IndexAsync(product, cancellationToken: cancellationToken));
    }

    public async Task<(IEnumerable<Product> Documents, long Total)> SearchProductsWithRetryAsync(
        string searchText,
        string[] fields = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
            await _repository.SearchAsync(
                searchText, fields, page, pageSize,
                cancellationToken: cancellationToken));
    }
}
```

## Complete Example: E-commerce Application

Here's a complete example of using QFace.Sdk.Elasticsearch in an e-commerce application:

```csharp
// 1. Register Elasticsearch services
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add Elasticsearch
builder.Services.AddElasticsearch(builder.Configuration);

// Register repositories
builder.Services.AddElasticsearchRepository<Product, ProductRepository>();
builder.Services.AddElasticsearchRepository<Category, CategoryRepository>();
builder.Services.AddElasticsearchRepository<Order, OrderRepository>();

// Register services
builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<ISearchService, SearchService>();

// 2. Document Models

// Product.cs
public class Product : EsBaseDocument
{
    [Text(Analyzer = "standard")]
    public string Name { get; set; }

    [Text(Analyzer = "standard")]
    public string Description { get; set; }

    [Number(NumberType.Double)]
    public decimal Price { get; set; }

    [Keyword]
    public string CategoryId { get; set; }

    [Keyword]
    public string Brand { get; set; }

    [Keyword]
    public List<string> Tags { get; set; } = new();

    [Number(NumberType.Integer)]
    public int StockQuantity { get; set; }

    [Keyword]
    public string SKU { get; set; }

    [Keyword]
    public string ImageUrl { get; set; }

    [Object]
    public ProductDetails Details { get; set; } = new();

    [Number(NumberType.Double)]
    public double Rating { get; set; }

    [Number(NumberType.Integer)]
    public int ReviewCount { get; set; }

    [Boolean]
    public bool Featured { get; set; }

    [Boolean]
    public bool OnSale { get; set; }

    [Number(NumberType.Double)]
    public decimal? SalePrice { get; set; }
}

// 3. Repositories

// ProductRepository.cs
public class ProductRepository : ElasticsearchRepository<Product>
{
    public ProductRepository(
        IElasticClient client,
        string indexName,
        ILogger<ProductRepository> logger)
        : base(client, indexName, logger)
    {
    }

    // Custom search with filters, sorting, and facets
    public async Task<ProductSearchResult> SearchProductsAsync(
        ProductSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Build query
            QueryContainer BuildQuery()
            {
                var queries = new List<QueryContainer>();

                // Text search
                if (!string.IsNullOrEmpty(request.SearchText))
                {
                    queries.Add(new MultiMatchQuery
                    {
                        Fields = new[] { "name^3", "description", "brand^2", "tags" },
                        Query = request.SearchText,
                        Type = TextQueryType.BestFields,
                        Fuzziness = Fuzziness.Auto
                    });
                }

                // Category filter
                if (!string.IsNullOrEmpty(request.CategoryId))
                {
                    queries.Add(new TermQuery
                    {
                        Field = "categoryId",
                        Value = request.CategoryId
                    });
                }

                // Brand filter
                if (!string.IsNullOrEmpty(request.Brand))
                {
                    queries.Add(new TermQuery
                    {
                        Field = "brand",
                        Value = request.Brand
                    });
                }

                // Price range filter
                if (request.MinPrice.HasValue || request.MaxPrice.HasValue)
                {
                    var rangeQuery = new RangeQuery { Field = "price" };

                    if (request.MinPrice.HasValue)
                        rangeQuery.GreaterThanOrEquals = request.MinPrice.Value;

                    if (request.MaxPrice.HasValue)
                        rangeQuery.LessThanOrEquals = request.MaxPrice.Value;

                    queries.Add(rangeQuery);
                }

                // Tag filters
                if (request.Tags != null && request.Tags.Any())
                {
                    queries.Add(new TermsQuery
                    {
                        Field = "tags",
                        Terms = request.Tags.ToArray()
                    });
                }

                // Featured/Sale filters
                if (request.OnlyFeatured)
                {
                    queries.Add(new TermQuery
                    {
                        Field = "featured",
                        Value = true
                    });
                }

                if (request.OnlyOnSale)
                {
                    queries.Add(new TermQuery
                    {
                        Field = "onSale",
                        Value = true
                    });
                }

                // Stock availability filter
                if (request.InStockOnly)
                {
                    queries.Add(new RangeQuery
                    {
                        Field = "stockQuantity",
                        GreaterThan = 0
                    });
                }

                // Always include active filter
                queries.Add(new TermQuery
                {
                    Field = "isActive",
                    Value = true
                });

                // Combine all queries with AND logic
                if (queries.Count == 0)
                    return new MatchAllQuery();

                if (queries.Count == 1)
                    return queries[0];

                return new BoolQuery
                {
                    Must = queries
                };
            }

            // Build sort
            ISort[] BuildSort()
            {
                var sorts = new List<ISort>();

                switch (request.SortBy)
                {
                    case ProductSortOptions.PriceLowToHigh:
                        sorts.Add(new FieldSort { Field = "price", Order = SortOrder.Ascending });
                        break;
                    case ProductSortOptions.PriceHighToLow:
                        sorts.Add(new FieldSort { Field = "price", Order = SortOrder.Descending });
                        break;
                    case ProductSortOptions.Rating:
                        sorts.Add(new FieldSort { Field = "rating", Order = SortOrder.Descending });
                        break;
                    case ProductSortOptions.NewestFirst:
                        sorts.Add(new FieldSort { Field = "createdDate", Order = SortOrder.Descending });
                        break;
                    case ProductSortOptions.Relevance:
                    default:
                        if (!string.IsNullOrEmpty(request.SearchText))
                            sorts.Add(new ScoreSort { Order = SortOrder.Descending });
                        else
                            sorts.Add(new FieldSort { Field = "name.keyword", Order = SortOrder.Ascending });
                        break;
                }

                return sorts.ToArray();
            }

            // Build aggregations
            Func<AggregationContainerDescriptor<Product>, IAggregationContainer> BuildAggregations()
            {
                return a => a
                    .Terms("categories", t => t
                        .Field("categoryId")
                        .Size(50))
                    .Terms("brands", t => t
                        .Field("brand")
                        .Size(50))
                    .Terms("tags", t => t
                        .Field("tags")
                        .Size(50))
                    .Range("price_ranges", r => r
                        .Field("price")
                        .Ranges(
                            rr => rr.To(50),
                            rr => rr.From(50).To(100),
                            rr => rr.From(100).To(200),
                            rr => rr.From(200).To(500),
                            rr => rr.From(500)))
                    .Filter("on_sale", f => f
                        .Filter(ff => ff
                            .Term(t => t
                                .Field(p => p.OnSale)
                                .Value(true))))
                    .Filter("in_stock", f => f
                        .Filter(ff => ff
                            .Range(r => r
                                .Field(p => p.StockQuantity)
                                .GreaterThan(0))));
            }

            // Execute search
            var searchResponse = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .From((request.Page - 1) * request.PageSize)
                .Size(request.PageSize)
                .Query(_ => BuildQuery())
                .Sort(BuildSort)
                .Aggregations(BuildAggregations),
                cancellationToken);

            if (!searchResponse.IsValid)
            {
                _logger.LogError("Error searching products: {Error}", searchResponse.DebugInformation);
                return new ProductSearchResult
                {
                    Products = new List<Product>(),
                    TotalCount = 0,
                    TotalPages = 0,
                    Page = request.Page,
                    PageSize = request.PageSize,
                    Facets = new ProductFacets()
                };
            }

            // Extract facets
            var facets = new ProductFacets();

            // Category facets
            if (searchResponse.Aggregations.Terms("categories") is TermsAggregate<string> categoryAgg)
            {
                facets.Categories = categoryAgg.Buckets
                    .Select(b => new FacetValue { Value = b.Key, Count = b.DocCount })
                    .ToList();
            }

            // Brand facets
            if (searchResponse.Aggregations.Terms("brands") is TermsAggregate<string> brandAgg)
            {
                facets.Brands = brandAgg.Buckets
                    .Select(b => new FacetValue { Value = b.Key, Count = b.DocCount })
                    .ToList();
            }

            // Tag facets
            if (searchResponse.Aggregations.Terms("tags") is TermsAggregate<string> tagAgg)
            {
                facets.Tags = tagAgg.Buckets
                    .Select(b => new FacetValue { Value = b.Key, Count = b.DocCount })
                    .ToList();
            }

            // Price range facets
            if (searchResponse.Aggregations.Range("price_ranges") is RangeAggregate priceAgg)
            {
                facets.PriceRanges = priceAgg.Buckets
                    .Select(b => new PriceRangeFacet
                    {
                        From = b.From,
                        To = b.To,
                        Count = b.DocCount
                    })
                    .ToList();
            }

            // On sale facet
            if (searchResponse.Aggregations.Filter("on_sale") is SingleBucketAggregate onSaleAgg)
            {
                facets.OnSaleCount = onSaleAgg.DocCount;
            }

            // In stock facet
            if (searchResponse.Aggregations.Filter("in_stock") is SingleBucketAggregate inStockAgg)
            {
                facets.InStockCount = inStockAgg.DocCount;
            }

            // Build result
            return new ProductSearchResult
            {
                Products = searchResponse.Documents.ToList(),
                TotalCount = searchResponse.Total,
                TotalPages = (int)Math.Ceiling(searchResponse.Total / (double)request.PageSize),
                Page = request.Page,
                PageSize = request.PageSize,
                Facets = facets
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching products with facets");
            throw;
        }
    }
}

// 4. Models

// ProductSearchRequest.cs
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

// ProductSearchResult.cs
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

// 5. Services

// ProductService.cs
public interface IProductService
{
    Task<ProductSearchResult> SearchProductsAsync(ProductSearchRequest request);
    Task<Product> GetProductByIdAsync(string id);
    Task<bool> CreateProductAsync(Product product);
    Task<bool> UpdateProductAsync(Product product);
    Task<bool> DeleteProductAsync(string id);
}

public class ProductService : IProductService
{
    private readonly ProductRepository _repository;

    public ProductService(ProductRepository repository)
    {
        _repository = repository;
    }

    public async Task<ProductSearchResult> SearchProductsAsync(ProductSearchRequest request)
    {
        return await _repository.SearchProductsAsync(request);
    }

    public async Task<Product> GetProductByIdAsync(string id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<bool> CreateProductAsync(Product product)
    {
        // Validate product
        if (string.IsNullOrEmpty(product.Name))
            throw new ArgumentException("Product name is required");

        if (product.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero");

        // Set audit fields
        product.CreatedDate = DateTime.UtcNow;
        product.LastModifiedDate = DateTime.UtcNow;

        return await _repository.IndexAsync(product);
    }

    public async Task<bool> UpdateProductAsync(Product product)
    {
        // Validate product
        if (string.IsNullOrEmpty(product.Id))
            throw new ArgumentException("Product ID is required for updates");

        if (string.IsNullOrEmpty(product.Name))
            throw new ArgumentException("Product name is required");

        if (product.Price <= 0)
            throw new ArgumentException("Product price must be greater than zero");

        // Set audit fields
        product.LastModifiedDate = DateTime.UtcNow;

        return await _repository.UpdateAsync(product);
    }

    public async Task<bool> DeleteProductAsync(string id)
    {
        return await _repository.SoftDeleteAsync(id);
    }
}

// 6. Controllers

// ProductsController.cs
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }

    [HttpGet]
    public async Task<ActionResult<ProductSearchResult>> GetProducts([FromQuery] ProductSearchRequest request)
    {
        var result = await _productService.SearchProductsAsync(request);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(string id)
    {
        var product = await _productService.GetProductByIdAsync(id);

        if (product == null)
            return NotFound();

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        try
        {
            var result = await _productService.CreateProductAsync(product);

            if (!result)
                return BadRequest("Failed to create product");

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(string id, Product product)
    {
        if (id != product.Id)
            return BadRequest("ID mismatch");

        try
        {
            var result = await _productService.UpdateProductAsync(product);

            if (!result)
                return NotFound();

            return NoContent();
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(string id)
    {
        var result = await _productService.DeleteProductAsync(id);

        if (!result)
            return NotFound();

        return NoContent();
    }
}
```

This completes the documentation for the QFace OpenSearch SDK. The SDK provides a powerful and flexible solution for working with both OpenSearch and Elasticsearch in .NET applications, from simple CRUD operations to complex search scenarios with facets, filters, and aggregations.

## 🎆 **OpenSearch 2.0 Benefits**

By migrating to OpenSearch.Client, the SDK now offers:

- ✅ **Future-Proof**: Built on actively maintained OpenSearch libraries
- ✅ **Performance**: Better connection handling and resource management
- ✅ **Compatibility**: Works with both OpenSearch and Elasticsearch
- ✅ **DigitalOcean Optimized**: Perfect configuration for managed services
- ✅ **Security**: Enhanced SSL and authentication support

By following the repository pattern, the SDK makes it easy to work with OpenSearch/Elasticsearch in a familiar way for .NET developers, while still leveraging the full power of modern search capabilities.

For best results, make sure to design your document models with search in mind, use appropriate field mappings, and optimize your queries for performance. The examples in this documentation should provide a good starting point for building your own OpenSearch-powered applications.

---

**QFace OpenSearch SDK v2.0** - Modern search capabilities with the reliability of the repository pattern. 🚀
