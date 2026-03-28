using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Logging;

internal static class UserContextScopeProperties
{
    public static List<KeyValuePair<string, object>> From(ICurrentUserService? user)
    {
        if (user == null)
            return [];
        var userId = user.GetUserId();
        return From(user.GetTenantId(), user.GetUserEmail(), user.GetUserName(), userId);
    }

    public static List<KeyValuePair<string, object>> From(
        string? tenantId,
        string? userEmail,
        string? userName,
        string? userId)
    {
        var items = new List<KeyValuePair<string, object>>(4);
        if (!string.IsNullOrWhiteSpace(tenantId))
            items.Add(new KeyValuePair<string, object>("TenantId", tenantId));
        if (!string.IsNullOrWhiteSpace(userEmail))
            items.Add(new KeyValuePair<string, object>("UserEmail", userEmail));
        if (!string.IsNullOrWhiteSpace(userName))
            items.Add(new KeyValuePair<string, object>("UserName", userName));
        if (!string.IsNullOrWhiteSpace(userId) &&
            !string.Equals(userId, "system", StringComparison.OrdinalIgnoreCase))
            items.Add(new KeyValuePair<string, object>("UserId", userId));
        return items;
    }
}
