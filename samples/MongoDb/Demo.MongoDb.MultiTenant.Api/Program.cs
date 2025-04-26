using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.MongoDb;
using QFace.Sdk.MongoDb.Repositories;
using QFace.Sdk.MongoDb.Models;
using QFace.Sdk.MongoDb.MultiTenant.Core;
using QFace.Sdk.MongoDb.MultiTenant.Middleware;
using QFace.Sdk.MongoDb.MultiTenant.Services;
using QFace.Sdk.MongoDb.MultiTenant.Models;
using QFace.Sdk.MongoDb.MultiTenant.Repositories;

var builder = WebApplication.CreateBuilder(args);

// 1. MongoDB setup
builder.Services.AddMongoDb(builder.Configuration);

// 2. Tenant Management setup
builder.Services.AddSingleton<ITenantAccessor, TenantAccessor>();
builder.Services.AddScoped<ITenantService, TenantService>();
builder.Services.AddScoped<ITenantRepository, TenantRepository>();
builder.Services.AddScoped<ITenantUserRepository, TenantUserRepository>();
builder.Services.AddScoped<ITenantDatabaseManager, TenantDatabaseManager>();

// 3. Mongo repositories
builder.Services.AddMongoRepository<ProductDocument>();
builder.Services.AddMongoRepository<TenantDocument, TenantRepository>();

// 4. ASP.NET services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 5. Tenant resolution middleware config
builder.Services.Configure<TenantResolutionOptions>(builder.Configuration.GetSection("TenantResolution"));

var app = builder.Build();

// Middleware
app.UseMiddleware<TenantResolutionMiddleware>();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

#region Product APIs

app.MapPost("/api/products", async ([FromBody] ProductDocument product, IMongoRepository<ProductDocument> repo) =>
{
    await repo.InsertOneAsync(product);
    return Results.Ok(product);
});

app.MapGet("/api/products", async (IMongoRepository<ProductDocument> repo) =>
{
    var products = await repo.GetAllAsync();
    return Results.Ok(products);
});

app.MapGet("/api/products/{id}", async (string id, IMongoRepository<ProductDocument> repo) =>
{
    var product = await repo.GetByIdAsync(id);
    return product is not null ? Results.Ok(product) : Results.NotFound();
});

app.MapPut("/api/products/{id}", async (string id, [FromBody] ProductDocument update, IMongoRepository<ProductDocument> repo) =>
{
    var existing = await repo.GetByIdAsync(id);
    if (existing == null) return Results.NotFound();

    existing.Name = update.Name;
    existing.Price = update.Price;

    await repo.UpdateAsync(existing);
    return Results.Ok(existing);
});

app.MapDelete("/api/products/{id}", async (string id, IMongoRepository<ProductDocument> repo) =>
{
    var deleted = await repo.DeleteByIdAsync(id);
    return deleted ? Results.Ok() : Results.NotFound();
});

#endregion

#region Tenant APIs

// Get all tenants
app.MapGet("/api/tenants", async (ITenantService tenantService) =>
{
    var tenants = await tenantService.GetAllAsync();
    return Results.Ok(tenants);
});

// Create a new tenant
app.MapPost("/api/tenants", async ([FromBody] TenantDocument tenant, ITenantService tenantService) =>
{
    if (string.IsNullOrWhiteSpace(tenant.Name))
    {
        return Results.BadRequest("Tenant Name is required.");
    }

    var tenantId = await tenantService.CreateAsync(tenant);
    return Results.Ok(new { TenantId = tenantId });
});

#endregion

app.Run();
public class ProductDocument : BaseDocument
{
    public string Name { get; set; }
    public double Price { get; set; }
}