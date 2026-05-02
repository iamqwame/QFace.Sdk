namespace QimErp.Shared.Common.Entities.ValueObjects;

/// <summary>
/// Per-tenant auth provider entry. A tenant has 1..N of these,
/// stored as a JSONB column on the Tenant table. Exactly one is
/// marked <see cref="IsDefault"/> at any time — that's the provider
/// promoted as the primary CTA on the sign-in page.
///
/// Names are normalised to lowercase on entry. Use the
/// <see cref="TenantAuthProviderNames"/> constants to avoid typos.
/// </summary>
public class TenantAuthProvider
{
    public string Name { get; set; } = string.Empty;

    public bool IsDefault { get; set; }

    /// <summary>
    /// Optional structured config slot — unused in v1, reserved for
    /// SAML metadata, OIDC client ids, "enforce 2FA per provider"
    /// flags etc. without another schema bump.
    /// </summary>
    public Dictionary<string, string>? Config { get; set; }

    public TenantAuthProvider() { }

    public TenantAuthProvider(string name, bool isDefault = false)
    {
        Name = (name ?? string.Empty).Trim().ToLowerInvariant();
        IsDefault = isDefault;
    }

    public static TenantAuthProvider Credentials(bool isDefault = true) =>
        new(TenantAuthProviderNames.Credentials, isDefault);

    public static TenantAuthProvider Microsoft(bool isDefault = false) =>
        new(TenantAuthProviderNames.Microsoft, isDefault);

    public static TenantAuthProvider Google(bool isDefault = false) =>
        new(TenantAuthProviderNames.Google, isDefault);
}

/// <summary>
/// Canonical lowercase names for the supported auth providers.
/// Backend, validators, and the wire format all agree on these
/// strings.
/// </summary>
public static class TenantAuthProviderNames
{
    public const string Credentials = "credentials";
    public const string Google = "google";
    public const string Microsoft = "microsoft";
}
