using Microsoft.AspNetCore.Mvc;
using QFace.Sdk.RedisCache.Extensions;
using QFace.Sdk.RedisCache.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Register Redis Cache
builder.Services.AddRedisCache(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();

app.MapControllers();

// Minimal API endpoints for cache operations
app.MapGet("/cache/{key}", async (
    string key,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var value = await cache.GetAsync<string>(key);
        return Results.Ok(new { key, value });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/cache/{key}", async (
    string key,
    [FromBody] CacheRequest request,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var expiration = request.ExpirationMinutes.HasValue 
            ? TimeSpan.FromMinutes(request.ExpirationMinutes.Value) 
            : (TimeSpan?)null;
        
        await cache.SetAsync(key, request.Value, expiration);
        return Results.Ok(new { message = $"Key '{key}' cached successfully", expiration });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapDelete("/cache/{key}", async (
    string key,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var removed = await cache.RemoveAsync(key);
        return Results.Ok(new { key, removed, message = removed ? "Key removed" : "Key not found" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/cache/{key}/exists", async (
    string key,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var exists = await cache.ExistsAsync(key);
        var ttl = await cache.GetTimeToLiveAsync(key);
        return Results.Ok(new { key, exists, ttl = ttl?.TotalSeconds });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// User cache endpoints (demonstrating GetOrSet pattern)
app.MapGet("/users/{id}", async (
    int id,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var user = await cache.GetOrSetAsync(
            $"user:{id}",
            async () =>
            {
                // Simulate database call
                await Task.Delay(100);
                return new User
                {
                    Id = id,
                    Name = $"User {id}",
                    Email = $"user{id}@example.com",
                    CreatedAt = DateTime.UtcNow
                };
            },
            TimeSpan.FromMinutes(30)
        );
        
        return Results.Ok(user);
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Hash operations
app.MapGet("/cache/{key}/hash/{field}", async (
    string key,
    string field,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var value = await cache.HashGetAsync<string>(key, field);
        if (value == null)
            return Results.NotFound(new { message = $"Field '{field}' not found in hash '{key}'" });
        
        return Results.Ok(new { key, field, value });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapPost("/cache/{key}/hash/{field}", async (
    string key,
    string field,
    [FromBody] CacheRequest request,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        await cache.HashSetAsync(key, field, request.Value);
        return Results.Ok(new { message = $"Hash field '{field}' set successfully" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/cache/{key}/hash", async (
    string key,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var hash = await cache.HashGetAllAsync<string>(key);
        return Results.Ok(new { key, hash });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// List operations
app.MapPost("/cache/{key}/list", async (
    string key,
    [FromBody] CacheRequest request,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var length = await cache.ListPushAsync(key, request.Value);
        return Results.Ok(new { key, length, message = "Value added to list" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/cache/{key}/list", async (
    string key,
    [FromServices] IRedisCacheService cache,
    [FromQuery] long start = 0,
    [FromQuery] long stop = -1) =>
{
    try
    {
        var items = await cache.ListGetRangeAsync<string>(key, start, stop);
        return Results.Ok(new { key, items, count = items.Count });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Set operations
app.MapPost("/cache/{key}/set", async (
    string key,
    [FromBody] CacheRequest request,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var added = await cache.SetAddAsync(key, request.Value);
        return Results.Ok(new { key, added, message = added ? "Value added to set" : "Value already exists" });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/cache/{key}/set", async (
    string key,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var items = await cache.SetGetAllAsync<string>(key);
        return Results.Ok(new { key, items, count = items.Count });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

// Batch operations
app.MapPost("/cache/batch", async (
    [FromBody] BatchCacheRequest request,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var expiration = request.ExpirationMinutes.HasValue 
            ? TimeSpan.FromMinutes(request.ExpirationMinutes.Value) 
            : (TimeSpan?)null;
        
        await cache.SetManyAsync(request.Items, expiration);
        return Results.Ok(new { message = $"Cached {request.Items.Count} items", expiration });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.MapGet("/cache/batch", async (
    [FromQuery] string[] keys,
    [FromServices] IRedisCacheService cache) =>
{
    try
    {
        var items = await cache.GetManyAsync<string>(keys);
        return Results.Ok(new { items, count = items.Count });
    }
    catch (Exception ex)
    {
        return Results.Problem(ex.Message);
    }
});

app.Run();

public class CacheRequest
{
    public string Value { get; set; } = string.Empty;
    public int? ExpirationMinutes { get; set; }
}

public class BatchCacheRequest
{
    public Dictionary<string, string> Items { get; set; } = new();
    public int? ExpirationMinutes { get; set; }
}

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

