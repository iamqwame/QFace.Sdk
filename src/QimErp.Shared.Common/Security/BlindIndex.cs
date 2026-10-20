using System.Security.Cryptography;
using System.Text;

namespace QimErp.Shared.Common.Security;

/// <summary>DATA-25: HMAC blind index for exact PII lookup without storing a searchable plaintext column.</summary>
public sealed class BlindIndex(byte[] key)
{
    private readonly byte[] _key = key ?? throw new ArgumentNullException(nameof(key));

    public string Compute(string value)
    {
        ArgumentNullException.ThrowIfNull(value);
        var normalised = value.Trim().ToLowerInvariant();
        using var hmac = new HMACSHA256(_key);
        return Convert.ToHexString(hmac.ComputeHash(Encoding.UTF8.GetBytes(normalised)));
    }
}
