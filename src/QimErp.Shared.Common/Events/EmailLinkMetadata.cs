namespace QimErp.Shared.Common.Events;

public enum EmailLinkPurpose
{
    PasswordReset,
    AccountUnlock,
    AccountActivation,
    SetPasswordInvite
}

public static class EmailLinkMetadata
{
    public const string LookupIdKey = "emailLinkLookupId";
    public const string PurposeKey = "emailLinkPurpose";

    public static string ReplacementKeyFor(EmailLinkPurpose purpose) => purpose switch
    {
        EmailLinkPurpose.PasswordReset or EmailLinkPurpose.AccountActivation => "Link",
        EmailLinkPurpose.AccountUnlock => "UnlockUrl",
        EmailLinkPurpose.SetPasswordInvite => "SetPasswordUrl",
        _ => throw new ArgumentOutOfRangeException(nameof(purpose), purpose, null)
    };

    public static bool TryParsePurpose(string? value, out EmailLinkPurpose purpose) =>
        Enum.TryParse(value, ignoreCase: true, out purpose);
}
