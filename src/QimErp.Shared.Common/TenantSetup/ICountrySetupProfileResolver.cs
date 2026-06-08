namespace QimErp.Shared.Common.TenantSetup;

public interface ICountrySetupProfileResolver
{
    ICountrySetupProfile Resolve(string? countryCode);
}
