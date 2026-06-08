using QimErp.Shared.Common.TenantSetup.Profiles;

namespace QimErp.Shared.Common.TenantSetup;

public static class CountrySetupServiceCollectionExtensions
{
    public static IServiceCollection AddCountrySetupProfiles(this IServiceCollection services)
    {
        services.AddSingleton<ICountrySetupProfile, GhanaCountryProfile>();
        services.AddSingleton<ICountrySetupProfile, NigeriaCountryProfile>();
        services.AddSingleton<ICountrySetupProfile, KenyaCountryProfile>();
        services.AddSingleton<ICountrySetupProfile, TanzaniaCountryProfile>();
        services.AddSingleton<ICountrySetupProfileResolver, CountrySetupProfileResolver>();
        return services;
    }
}
