using QimErp.Shared.Common.Services.Retention;

namespace QimErp.Shared.Common.Extensions;

public static class RetentionPolicyClientServiceCollectionExtensions
{
    public static IServiceCollection AddRetentionPolicyClient(this IServiceCollection services)
    {
        services.AddHttpClient(RetentionPolicyClient.HttpClientName);
        services.AddScoped<IRetentionPolicyClient, RetentionPolicyClient>();
        return services;
    }
}
