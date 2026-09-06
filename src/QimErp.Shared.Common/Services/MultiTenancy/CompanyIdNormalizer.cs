namespace QimErp.Shared.Common.Services.MultiTenancy;

/// <summary>
/// Aligns JWT/header/claim company ids with DB values: Guid claims may differ only by casing, but
/// PostgreSQL uses case-sensitive string equality, which breaks EF global filters (e.CompanyId == context).
/// </summary>
public static class CompanyIdNormalizer
{
    public static string Normalize(string? companyId)
    {
        return NormalizeOrNull(companyId) ?? string.Empty;
    }

    public static string? NormalizeOrNull(string? companyId)
    {
        if (string.IsNullOrWhiteSpace(companyId))
            return null;

        return Guid.TryParse(companyId.Trim(), out var g) ? g.ToString("D") : companyId.Trim();
    }
}
