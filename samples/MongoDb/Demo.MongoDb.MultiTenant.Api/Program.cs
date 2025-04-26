using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.MongoDb;
using QFace.Sdk.MongoDb.Config;
using QFace.Sdk.MongoDb.Repositories;
using QFace.Sdk.MongoDb.MultiTenant.Core;
using QFace.Sdk.MongoDb.MultiTenant.Middleware;
using QFace.Sdk.MongoDb.MultiTenant.Services;
using QFace.Sdk.MongoDb.MultiTenant.Models;
using QFace.Sdk.MongoDb.MultiTenant.Repositories;
using System.ComponentModel.DataAnnotations;
using QFace.Sdk.MongoDb.MultiTenant;

var builder = WebApplication.CreateBuilder(args);

// 1. Logging Configuration
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// 2. MongoDB setup
builder.Services.AddMongoDb(builder.Configuration);

// 3. Tenant Management setup
builder.Services.AddSingleton<ITenantAccessor, TenantAccessor>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantDatabaseManager, TenantDatabaseManager>();

// 4. Mongo repositories
builder.Services.AddTenantAwareRepository<ProductDocument>();
builder.Services.AddMongoRepository<Tenant, TenantRepository>();
builder.Services.AddMongoRepository<TenantUserDocument, TenantUserRepository>();

// 5. Additional repository bindings
builder.Services.AddScoped<ITenantRepository>(sp => 
    sp.GetRequiredService<IMongoRepository<Tenant>>() as TenantRepository);
builder.Services.AddScoped<ITenantUserRepository>(sp => 
    sp.GetRequiredService<IMongoRepository<TenantUserDocument>>() as TenantUserRepository);

// 6. ASP.NET Core services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 7. Tenant resolution configuration
var tenantResolutionOptions = new TenantResolutionOptions();
builder.Configuration.GetSection("TenantResolution").Bind(tenantResolutionOptions);
builder.Services.AddSingleton(tenantResolutionOptions);

var app = builder.Build();

// 8. Middleware configuration
app.UseMiddleware<TenantResolutionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

#region Product Endpoints

app.MapPost("/api/products", async (
    [FromBody] ProductCreateRequest productRequest, 
    ITenantAccessor tenantAccessor, 
    IMongoRepository<ProductDocument> repo,
    ILogger<Program> logger) =>
{
    // Validate input
    var validationContext = new ValidationContext(productRequest);
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(productRequest, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    // Get current tenant context
    var currentTenantId = tenantAccessor.GetCurrentTenantId();
    if (string.IsNullOrEmpty(currentTenantId))
    {
        return Results.BadRequest("No tenant context available");
    }

    // Create product document
    var product = new ProductDocument
    {
        Name = productRequest.Name,
        Price = productRequest.Price,
        TenantId = currentTenantId
    };

    try
    {
        await repo.InsertOneAsync(product);
        logger.LogInformation($"Product created for tenant {currentTenantId}: {product.Name}");
        return Results.Created($"/api/products/{product.Id}", product);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating product");
        return Results.Problem("An error occurred while creating the product");
    }
});

app.MapGet("/api/products", async (
    ITenantAccessor tenantAccessor, 
    IMongoRepository<ProductDocument> repo,
    ILogger<Program> logger) =>
{
    var currentTenantId = tenantAccessor.GetCurrentTenantId();
    if (string.IsNullOrEmpty(currentTenantId))
    {
        return Results.BadRequest("No tenant context available");
    }

    try
    {
        var products = await repo.FindAsync(p => p.TenantId == currentTenantId);
        return Results.Ok(products);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving products");
        return Results.Problem("An error occurred while retrieving products");
    }
});

app.MapGet("/api/products/{id}", async (
    string id, 
    ITenantAccessor tenantAccessor, 
    IMongoRepository<ProductDocument> repo,
    ILogger<Program> logger) =>
{
    var currentTenantId = tenantAccessor.GetCurrentTenantId();
    if (string.IsNullOrEmpty(currentTenantId))
    {
        return Results.BadRequest("No tenant context available");
    }

    try
    {
        var product = await repo.FindOneAsync(p => p.Id == id && p.TenantId == currentTenantId);
        return product is not null ? Results.Ok(product) : Results.NotFound();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error retrieving product");
        return Results.Problem("An error occurred while retrieving the product");
    }
});

app.MapPut("/api/products/{id}", async (
    string id, 
    [FromBody] ProductUpdateRequest updateRequest, 
    ITenantAccessor tenantAccessor, 
    IMongoRepository<ProductDocument> repo,
    ILogger<Program> logger) =>
{
    var currentTenantId = tenantAccessor.GetCurrentTenantId();
    if (string.IsNullOrEmpty(currentTenantId))
    {
        return Results.BadRequest("No tenant context available");
    }

    try
    {
        var existing = await repo.FindOneAsync(p => p.Id == id && p.TenantId == currentTenantId);
        if (existing == null) return Results.NotFound();

        existing.Name = updateRequest.Name;
        existing.Price = updateRequest.Price;

        var updated = await repo.UpdateAsync(existing);
        return updated ? Results.Ok(existing) : Results.Problem("Update failed");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error updating product");
        return Results.Problem("An error occurred while updating the product");
    }
});

app.MapDelete("/api/products/{id}", async (
    string id, 
    ITenantAccessor tenantAccessor, 
    IMongoRepository<ProductDocument> repo,
    ILogger<Program> logger) =>
{
    var currentTenantId = tenantAccessor.GetCurrentTenantId();
    if (string.IsNullOrEmpty(currentTenantId))
    {
        return Results.BadRequest("No tenant context available");
    }

    try
    {
        var deleted = await repo.DeleteByIdAsync(id);
        return deleted ? Results.Ok() : Results.NotFound();
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error deleting product");
        return Results.Problem("An error occurred while deleting the product");
    }
});

#endregion

#region Tenant Endpoints

app.MapGet("/api/tenants", async (ITenantService tenantService) =>
{
    var tenants = await tenantService.GetAllAsync();
    return Results.Ok(tenants);
});

app.MapPost("/api/tenants", async (
    [FromBody] TenantCreateRequest tenantRequest, 
    ITenantService tenantService,
    ILogger<Program> logger) =>
{
    // Validate input
    var validationContext = new ValidationContext(tenantRequest);
    var validationResults = new List<ValidationResult>();
    if (!Validator.TryValidateObject(tenantRequest, validationContext, validationResults, true))
    {
        return Results.BadRequest(validationResults);
    }

    try
    {
        var tenant = new Tenant
        {
            Name = tenantRequest.Name,
            Code = tenantRequest.Code,
            Description = tenantRequest.Description ?? "",
            IsProvisioned = true,
            TenantType = TenantType.Shared,
            Contact = new ContactInfo
                {
                    AdminName = tenantRequest.AdminName,
                    AdminEmail = tenantRequest.AdminEmail
                }
        };

        var tenantId = await tenantService.CreateAsync(tenant);
        logger.LogInformation($"Tenant created: {tenant.Name} (ID: {tenantId})");
        return Results.Created($"/api/tenants/{tenantId}", new { TenantId = tenantId });
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error creating tenant");
        return Results.Problem("An error occurred while creating the tenant");
    }
});

#endregion

app.Run();

#region Request Models

public class ProductCreateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public double Price { get; set; }
}

public class ProductUpdateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero")]
    public double Price { get; set; }
}

public class TenantCreateRequest
{
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(50, MinimumLength = 2)]
    public string Code { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    [EmailAddress]
    public string AdminEmail { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string AdminName { get; set; } = string.Empty;
}

#endregion

#region Product Model

public class ProductDocument : TenantBaseDocument
{
    public string Name { get; set; } = string.Empty;
    public double Price { get; set; }
}

#endregion