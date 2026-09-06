using System.Security.Claims;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.MultiTenancy;

namespace QimErp.Shared.Common.Middlewares;

public class CompanyContextMiddleware
{
    public const string CompanyIdHeader = "X-Company-Id";
    public const string CompanyScopeHeader = "X-Company-Scope";

    public const string CompanyScopeClaim = "companyScope";
    public const string CompanyIdsClaim = "companyIds";
    public const string DefaultCompanyIdClaim = "defaultCompanyId";

    private const string ScopeAll = "all";
    private const string ScopeList = "list";

    private readonly RequestDelegate _next;

    public CompanyContextMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(
        HttpContext context,
        ICompanyContext companyContext,
        ICurrentUserService currentUserService,
        IOptions<SystemOptions> systemOptions,
        ILogger<CompanyContextMiddleware> logger)
    {
        if (systemOptions.Value.Company.ForceInactive)
        {
            await RunWithScope(context, companyContext, CompanyScope.Inactive);
            return;
        }

        var claims = currentUserService.GetClaims().ToList();
        var claimScope = FindClaim(claims, CompanyScopeClaim);

        if (string.IsNullOrWhiteSpace(claimScope))
        {
            await RunWithScope(context, companyContext, CompanyScope.Inactive);
            return;
        }

        var defaultCompanyId = CompanyIdNormalizer.NormalizeOrNull(FindClaim(claims, DefaultCompanyIdClaim));
        var headerCompanyId = CompanyIdNormalizer.NormalizeOrNull(context.Request.Headers[CompanyIdHeader].FirstOrDefault());
        var headerScope = context.Request.Headers[CompanyScopeHeader].FirstOrDefault()?.Trim();
        var headerWantsAll = ScopeAll.Equals(headerScope, StringComparison.OrdinalIgnoreCase);

        if (ScopeAll.Equals(claimScope.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            var allScope = headerCompanyId is null
                ? CompanyScope.AllCompanies(defaultCompanyId)
                : CompanyScope.ForCompanies([headerCompanyId], headerCompanyId);

            await RunWithScope(context, companyContext, allScope);
            return;
        }

        if (!ScopeList.Equals(claimScope.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            logger.LogWarning("Unrecognized {Claim} claim value '{Value}'; company scope left inactive.",
                CompanyScopeClaim, claimScope);
            await RunWithScope(context, companyContext, CompanyScope.Inactive);
            return;
        }

        var allowed = ParseCompanyIds(FindClaim(claims, CompanyIdsClaim));

        if (allowed.Count == 0)
        {
            logger.LogError(
                "{Claim} claim is 'list' but {IdsClaim} is empty. Failing closed — only tenant-shared rows are visible.",
                CompanyScopeClaim, CompanyIdsClaim);
            await RunWithScope(context, companyContext, CompanyScope.ForCompanies([], null));
            return;
        }

        // The header may only NARROW the signed claim. An out-of-claim id is a 403, never a
        // silent narrow-to-nothing that returns an empty page.
        if (headerCompanyId is not null)
        {
            if (!allowed.Contains(headerCompanyId, StringComparer.Ordinal))
            {
                logger.LogWarning(
                    "Rejected {Header} '{CompanyId}' — not present in the signed {IdsClaim} claim.",
                    CompanyIdHeader, headerCompanyId, CompanyIdsClaim);

                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                await context.Response.WriteAsJsonAsync(new
                {
                    error = "company_scope_denied",
                    detail = $"The requested company is not in this token's allowed company list. " +
                             $"Remove the {CompanyIdHeader} header or request access to that company."
                });
                return;
            }

            await RunWithScope(context, companyContext, CompanyScope.ForCompanies([headerCompanyId], headerCompanyId));
            return;
        }

        var active = headerWantsAll
            ? null
            : defaultCompanyId is not null && allowed.Contains(defaultCompanyId, StringComparer.Ordinal)
                ? defaultCompanyId
                : null;

        await RunWithScope(context, companyContext, CompanyScope.ForCompanies(allowed, active));
    }

    private async Task RunWithScope(HttpContext context, ICompanyContext companyContext, CompanyScope scope)
    {
        companyContext.SetScope(scope);
        try
        {
            await _next(context);
        }
        finally
        {
            companyContext.Clear();
        }
    }

    private static List<string> ParseCompanyIds(string? csv)
    {
        if (string.IsNullOrWhiteSpace(csv))
            return [];

        return csv
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(CompanyIdNormalizer.NormalizeOrNull)
            .Where(id => id is not null)
            .Select(id => id!)
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string? FindClaim(IEnumerable<Claim> claims, string type)
        => claims.FirstOrDefault(c => c.Type.Equals(type, StringComparison.OrdinalIgnoreCase))?.Value;
}

public static class CompanyContextMiddlewareExtensions
{
    public static IApplicationBuilder UseCompanyContext(this IApplicationBuilder app)
    {
        return app.UseMiddleware<CompanyContextMiddleware>();
    }
}
