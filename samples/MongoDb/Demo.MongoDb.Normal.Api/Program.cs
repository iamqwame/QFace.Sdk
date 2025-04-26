using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.MongoDb;
using QFace.Sdk.MongoDb.Repositories;
using QFace.Sdk.MongoDb.Models;

var builder = WebApplication.CreateBuilder(args);

// 1. Setup MongoDB
builder.Services.AddMongoDb(builder.Configuration);

// 2. Add Repository for ProductDocument
builder.Services.AddMongoRepository<ProductDocument>();

// 3. Setup basic ASP.NET services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Swagger for API testing
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

#region Sample Minimal APIs for Products

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

app.Run();



public class ProductDocument : BaseDocument
{
    public string Name { get; set; }
    public double Price { get; set; }
}