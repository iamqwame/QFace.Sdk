using Microsoft.AspNetCore.Authorization;

namespace QimErp.Shared.Common.Authorization;

public sealed record PermissionRequirement(string Code) : IAuthorizationRequirement
{
    public const char CodeSeparator = '|';

    // Any-of: holding any single code satisfies the requirement.
    public IReadOnlyList<string> Codes { get; } =
        (Code ?? string.Empty)
            .Split(CodeSeparator, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
}
