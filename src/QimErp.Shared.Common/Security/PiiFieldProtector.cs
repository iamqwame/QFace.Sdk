using System.Security.Cryptography;
using System.Text;

namespace QimErp.Shared.Common.Security;

/// <summary>DATA-25: AES-GCM protector for PII/Sensitive columns at rest.</summary>
public sealed class PiiFieldProtector(byte[] key)
{
    private readonly byte[] _key = key ?? throw new ArgumentNullException(nameof(key));

    public string Protect(string plaintext)
    {
        ArgumentNullException.ThrowIfNull(plaintext);
        var nonce = RandomNumberGenerator.GetBytes(12);
        var plainBytes = Encoding.UTF8.GetBytes(plaintext);
        var cipherBytes = new byte[plainBytes.Length];
        var tag = new byte[16];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Encrypt(nonce, plainBytes, cipherBytes, tag);
        return Convert.ToBase64String(nonce.Concat(tag).Concat(cipherBytes).ToArray());
    }

    public string Unprotect(string ciphertext)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(ciphertext);
        var payload = Convert.FromBase64String(ciphertext);
        if (payload.Length < 28)
            throw new CryptographicException("Invalid protected PII payload.");
        var nonce = payload[..12];
        var tag = payload[12..28];
        var cipherBytes = payload[28..];
        var plainBytes = new byte[cipherBytes.Length];
        using var aes = new AesGcm(_key, tag.Length);
        aes.Decrypt(nonce, cipherBytes, tag, plainBytes);
        return Encoding.UTF8.GetString(plainBytes);
    }
}
