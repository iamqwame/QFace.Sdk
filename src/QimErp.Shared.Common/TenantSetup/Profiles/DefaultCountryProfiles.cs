namespace QimErp.Shared.Common.TenantSetup.Profiles;

/// <summary>
/// Nigeria stub — inherits Ghana statutory defaults.
/// TODO: Override GetPensionConfig (NSITF/PenCom rates), GetIncomeTaxBrackets (PAYE bands),
///       GetLeaveTypes (Employment Act 1990), and GetPublicHolidays when properly implemented.
/// </summary>
public sealed class NigeriaCountryProfile : GhanaCountryProfile
{
    public override string CountryCode => "NG";
    public override string CountryName => "Nigeria";
    public override string Currency => "NGN";
    public override string CurrencySymbol => "₦";
}

/// <summary>
/// Kenya stub — inherits Ghana statutory defaults.
/// TODO: Override GetPensionConfig (NSSF rates), GetIncomeTaxBrackets (KRA PAYE bands),
///       GetLeaveTypes (Employment Act 2007), and GetPublicHolidays when properly implemented.
/// </summary>
public sealed class KenyaCountryProfile : GhanaCountryProfile
{
    public override string CountryCode => "KE";
    public override string CountryName => "Kenya";
    public override string Currency => "KES";
    public override string CurrencySymbol => "KSh";
}

/// <summary>
/// Tanzania stub — inherits Ghana statutory defaults.
/// TODO: Override GetPensionConfig (NSSF/PPF rates), GetIncomeTaxBrackets (TRA PAYE bands),
///       GetLeaveTypes (Employment and Labour Relations Act 2004), and GetPublicHolidays when properly implemented.
/// </summary>
public sealed class TanzaniaCountryProfile : GhanaCountryProfile
{
    public override string CountryCode => "TZ";
    public override string CountryName => "Tanzania";
    public override string Currency => "TZS";
    public override string CurrencySymbol => "TSh";
}
