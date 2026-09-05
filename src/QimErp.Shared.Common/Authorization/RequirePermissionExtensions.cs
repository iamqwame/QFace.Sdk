using Microsoft.AspNetCore.Authorization;

namespace QimErp.Shared.Common.Authorization;

public static class RequirePermissionExtensions
{
    /// <summary>Multiple codes are any-of: the caller needs only one of them, not all.</summary>
    public static TBuilder RequirePermission<TBuilder>(this TBuilder builder, params string[] codes)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);

        var normalized = (codes ?? [])
            .Where(code => !string.IsNullOrWhiteSpace(code))
            .Select(code => code.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (normalized.Length == 0)
            throw new ArgumentException("At least one permission code is required.", nameof(codes));

        return builder.RequireAuthorization(PermissionPolicyProvider.PolicyNameFor(normalized));
    }
}
