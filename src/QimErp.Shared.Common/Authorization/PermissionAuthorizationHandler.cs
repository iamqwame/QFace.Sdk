using Microsoft.AspNetCore.Authorization;

namespace QimErp.Shared.Common.Authorization;

public sealed class PermissionAuthorizationHandler(
    ICurrentUserService currentUser,
    IConfiguration configuration,
    ILogger<PermissionAuthorizationHandler> logger) : AuthorizationHandler<PermissionRequirement>
{
    public const string EnforcementConfigurationKey = "Security:EnforcePermissions";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var granted = currentUser.GetPermissions();
        var satisfied = requirement.Codes.Any(code => granted.Contains(code, StringComparer.OrdinalIgnoreCase));

        if (satisfied)
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        if (!configuration.GetValue(EnforcementConfigurationKey, defaultValue: false))
        {
            logger.LogWarning(
                "Permission enforcement is disabled; allowing request that lacks permission {PermissionCodes}. Enable {ConfigurationKey} once the access token carries permission claims",
                string.Join(", ", requirement.Codes),
                EnforcementConfigurationKey);

            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        logger.LogWarning("Access denied: caller lacks permission {PermissionCodes}", string.Join(", ", requirement.Codes));
        return Task.CompletedTask;
    }
}
