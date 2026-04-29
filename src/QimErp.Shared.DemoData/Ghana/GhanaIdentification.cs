namespace QimErp.Shared.DemoData.Ghana;

/// <summary>
/// Format specs for the four government IDs every employed Ghanaian has — used by
/// GhanaFakerExtensions to populate <c>IdentificationDetails</c> VOs realistically.
/// These are random-looking but format-conformant; they are NOT real numbers.
/// </summary>
public static class GhanaIdentification
{
    /// <summary>Ghana Card (NIA): GHA-XXXXXXXXX-X — 9 digits + checksum digit.</summary>
    public const string GhanaCardPattern = "GHA-#########-#";

    /// <summary>SSNIT social-security number: 1 letter + 12 alphanumeric (commonly 12 digits in practice).</summary>
    public const string SsnitPattern = "L############";

    /// <summary>TIN (Tax Identification Number): P + 10 digits for individuals.</summary>
    public const string TinPattern = "P##########";

    /// <summary>Ghanaian passport: G + 7 digits (e.g. G1234567).</summary>
    public const string PassportPattern = "G#######";

    /// <summary>DVLA driver's licence: 2 letters + 5 digits + 2 letters (varies; this is one common form).</summary>
    public const string DriverLicencePattern = "LL#####LL";
}
