using QimErp.Shared.Common.Services.LegalHold;

namespace QimErp.Shared.Common.Extensions;

public static class LegalHoldGateServiceCollectionExtensions
{
    public static IServiceCollection AddLegalHoldGateClient(this IServiceCollection services)
    {
        services.AddHttpClient(LegalHoldGateClient.HttpClientName);
        services.AddScoped<ILegalHoldGate, LegalHoldGateClient>();
        return services;
    }
}
