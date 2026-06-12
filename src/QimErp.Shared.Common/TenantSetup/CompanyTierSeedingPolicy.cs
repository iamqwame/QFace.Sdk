namespace QimErp.Shared.Common.TenantSetup;

/// <summary>
/// Seed volume caps per company tier. Pure value — no DI required.
/// Usage: var policy = CompanyTierSeedingPolicy.From(request.CompanyType);
/// </summary>
public readonly record struct CompanyTierSeedingPolicy(
    int MaxLeaveTypeVariants,
    int MaxWorkflowTemplates,
    int MaxOnboardingTemplates,
    bool SeedAdvancedGrades,
    bool SeedRiskAllowance,
    bool SeedActingAllowance)
{
    public static CompanyTierSeedingPolicy From(string? companyType) =>
        (companyType ?? string.Empty).Trim().ToUpperInvariant() switch
        {
            "STARTUP"   => new(5,  5,  2, false, true,  false),
            "SME"       => new(7,  10, 3, true,  true,  true),
            "CORPORATE" => new(10, 20, 5, true,  true,  true),
            "NONPROFIT" => new(5,  8,  2, false, false, false),
            _           => new(7,  10, 3, true,  true,  true)   // default: SME
        };
}
