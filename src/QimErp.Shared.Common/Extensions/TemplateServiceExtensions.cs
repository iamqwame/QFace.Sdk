using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extension methods for registering template services.
/// </summary>
public static class TemplateServiceExtensions
{
    /// <summary>
    /// Registers TemplateStorageOptions and ITemplateService.
    /// Requires AddBlobStorageServices and IDistributedCacheService (Redis) to be registered.
    /// </summary>
    public static IServiceCollection AddTemplateServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<TemplateStorageOptions>(configuration.GetSection(TemplateStorageOptions.SectionName));
        services.AddScoped<ITemplateService, TemplateService>();
        return services;
    }
}
