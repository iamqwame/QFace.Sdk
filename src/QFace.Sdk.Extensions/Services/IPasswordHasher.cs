namespace QFace.Sdk.Extensions.Services;


/// Interface for password hashing
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Creates a hash and salt for a password
    /// </summary>
    /// <param name="password">The password to hash</param>
    /// <returns>Hash and salt tuple</returns>
    (string Hash, string Salt) HashPassword(string password);
        
    /// <summary>
    /// Verifies a password against a hash and salt
    /// </summary>
    /// <param name="password">The password to verify</param>
    /// <param name="hash">The stored hash</param>
    /// <param name="salt">The stored salt</param>
    /// <returns>True if password is valid, false otherwise</returns>
    bool VerifyPassword(string password, string hash, string salt);
}