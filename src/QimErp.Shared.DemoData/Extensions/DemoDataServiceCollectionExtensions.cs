using Microsoft.Extensions.DependencyInjection;
using QimErp.Shared.DemoData.Bogus;
using QimErp.Shared.DemoData.Industry;
using QimErp.Shared.DemoData.Industry.Profiles;

namespace QimErp.Shared.DemoData.Extensions;

public static class DemoDataServiceCollectionExtensions
{
    /// <summary>
    /// Registers demo-data services: industry profile resolver and a transient
    /// <see cref="IDemoFaker"/> factory. Call from each service that needs to generate
    /// demo data (IAM admin endpoints, CoreHr bulk-seed activities).
    /// </summary>
    public static IServiceCollection AddDemoData(this IServiceCollection services)
    {
        services.AddSingleton<IIndustryProfileResolver>(_ => new IndustryRegistry(BuildAllProfiles()));
        services.AddTransient<IDemoFaker>(_ => new DemoFaker(seed: Random.Shared.Next()));
        return services;
    }

    /// <summary>
    /// All concrete industry profiles known to v1. Each profile lifts its baseline
    /// org units, job titles, and distributions from the matching IAM seed file
    /// (QimErp.IAM.Seeding.Demo/Constants/IndustryData/{Industry}IndustryData.cs).
    /// </summary>
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
    }
}
