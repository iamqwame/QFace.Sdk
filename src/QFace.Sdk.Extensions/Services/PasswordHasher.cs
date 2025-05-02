using System.Security.Cryptography;
using System.Text;

namespace QFace.Sdk.Extensions.Services;

/// <summary>
/// Implementation of password hasher using HMACSHA512
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    /// <summary>
    /// Creates a hash and salt for a password
    /// </summary>
    public (string Hash, string Salt) HashPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
            throw new ArgumentException("Password cannot be empty", nameof(password));

        using var hmac = new HMACSHA512();
        var salt = Convert.ToBase64String(hmac.Key);
        var hash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                
        return (hash, salt);
    }
        
    /// <summary>
    /// Verifies a password against a hash and salt
    /// </summary>
    public bool VerifyPassword(string password, string hash, string salt)
    {
        if (string.IsNullOrEmpty(password))
            return false;
                
        if (string.IsNullOrEmpty(hash) || string.IsNullOrEmpty(salt))
            return false;
                
        try
        {
            var saltBytes = Convert.FromBase64String(salt);

            using var hmac = new HMACSHA512(saltBytes);
            var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            return computedHash == hash;
        }
        catch
        {
            return false;
        }
    }
}