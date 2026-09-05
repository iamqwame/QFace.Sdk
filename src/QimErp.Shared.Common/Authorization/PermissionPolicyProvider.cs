using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace QimErp.Shared.Common.Authorization;

public sealed class PermissionPolicyProvider(IOptions<AuthorizationOptions> options) : IAuthorizationPolicyProvider
{
    public const string PolicyPrefix = "perm:";

    private readonly DefaultAuthorizationPolicyProvider _fallback = new(options);

    public static string PolicyNameFor(IEnumerable<string> codes) =>
        PolicyPrefix + string.Join(PermissionRequirement.CodeSeparator, codes);

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        if (string.IsNullOrEmpty(policyName) ||
            !policyName.StartsWith(PolicyPrefix, StringComparison.OrdinalIgnoreCase))
            return _fallback.GetPolicyAsync(policyName);

        var codes = policyName[PolicyPrefix.Length..];
        if (string.IsNullOrWhiteSpace(codes))
            return _fallback.GetPolicyAsync(policyName);

        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .AddRequirements(new PermissionRequirement(codes))
            .Build();

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
