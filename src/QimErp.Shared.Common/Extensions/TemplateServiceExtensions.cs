using QFace.Sdk.BlobStorage.Extensions;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for registering template services.
/// Requires AddBlobStorageServices to be called first. Registers ITemplateService.
/// </summary>
public static class TemplateServiceExtensions
{
    /// <summary>
    /// Registers ITemplateService. Requires AddBlobStorageServices and IDistributedCacheService (Redis) to be registered.
    /// </summary>
    public static IServiceCollection AddTemplateServices(this IServiceCollection services, IConfiguration configuration)
    {
        if (!services.Any(d => d.ServiceType == typeof(BlobStorageServicesMarker)))
            throw new InvalidOperationException("AddBlobStorageServices must be called before AddTemplateServices.");

        services.AddScoped<ITemplateService, TemplateService>();
        return services;
    }
}
