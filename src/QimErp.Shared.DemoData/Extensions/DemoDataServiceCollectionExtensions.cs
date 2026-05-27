using Microsoft.Extensions.DependencyInjection;
using QimErp.Shared.DemoData.Industry;
using QimErp.Shared.DemoData.Industry.Profiles;

namespace QimErp.Shared.DemoData.Extensions;

public static class DemoDataServiceCollectionExtensions
{
    public static IServiceCollection AddDemoData(this IServiceCollection services)
    {
        services.AddSingleton<IIndustryProfileResolver>(_ => new IndustryRegistry(BuildAllProfiles()));
        services.AddTransient<IDemoFaker>(_ => new DemoFaker(seed: Random.Shared.Next()));
        return services;
    }

    private static IEnumerable<IIndustryProfile> BuildAllProfiles()
    {
        yield return new BankingIndustryProfile();
        yield return new ConstructionIndustryProfile();
        yield return new SoftwareIndustryProfile();
        yield return new HealthcareIndustryProfile();
        yield return new ManufacturingIndustryProfile();
        yield return new ServiceIndustryProfile();
        yield return new EducationIndustryProfile();
        yield return new ECommerceIndustryProfile();
        yield return new TelecommunicationsIndustryProfile();
        yield return new NonProfitIndustryProfile();
        yield return new SalonBeautyIndustryProfile();
    }
}
