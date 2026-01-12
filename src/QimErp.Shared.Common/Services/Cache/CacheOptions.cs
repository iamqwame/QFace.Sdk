namespace QimErp.Shared.Common.Services.Cache;

public class CacheOptions
{
    public const string SectionName = "Cache";
    
    /// <summary>
    /// Enable or disable caching. When disabled, no Redis connections will be attempted.
    /// </summary>
    public bool Enabled { get; set; } = true;
    
    /// <summary>
    /// Redis connection string. If not provided, falls back to ConnectionStrings.Redis
    /// </summary>
    public string? ConnectionString { get; set; }
}

