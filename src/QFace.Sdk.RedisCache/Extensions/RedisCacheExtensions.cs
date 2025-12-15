using Microsoft.Extensions.Http;
using System.Net.Http;
using QFace.Sdk.RedisCache.Models;
using QFace.Sdk.RedisCache.Services;
using QFace.Sdk.RedisCache.Services.Providers;

namespace QFace.Sdk.RedisCache.Extensions;

/// <summary>
/// Extension methods for registering Redis Cache services
/// </summary>
public static class RedisCacheExtensions
{
    /// <summary>
    /// Adds Redis Cache services to the service collection
    /// </summary>
    public static IServiceCollection AddRedisCache(
        this IServiceCollection services, 
        IConfiguration configuration)
    {
        // Register options
        services.Configure<RedisCacheOptions>(configuration.GetSection("RedisCache"));
        
        // Register HTTP client factory for Upstash provider
        services.AddHttpClient("UpstashRedis");
        
        // Register provider based on configuration
        services.AddSingleton<IRedisProvider>(sp => CreateRedisProvider(sp));
        
        // Register main service
        services.AddScoped<IRedisCacheService, RedisCacheService>();
        
        return services;
    }
    
    private static IRedisProvider CreateRedisProvider(IServiceProvider serviceProvider)
    {
        var options = serviceProvider.GetRequiredService<IOptions<RedisCacheOptions>>().Value;
        var loggerFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        var logger = loggerFactory.CreateLogger("RedisCacheExtensions");
        
        logger.LogInformation("Creating Redis provider: {Provider}", options.Provider);
        
        return options.Provider switch
        {
            RedisProvider.Upstash => new UpstashRedisProvider(
                options.Upstash,
                serviceProvider.GetRequiredService<IHttpClientFactory>(),
                loggerFactory.CreateLogger<UpstashRedisProvider>()
            ),
            RedisProvider.StackExchange => new StackExchangeRedisProvider(
                options.StackExchange,
                loggerFactory.CreateLogger<StackExchangeRedisProvider>()
            ),
            _ => throw new ArgumentOutOfRangeException(
                nameof(options.Provider), 
                $"Unknown Redis provider: {options.Provider}")
        };
    }
}

