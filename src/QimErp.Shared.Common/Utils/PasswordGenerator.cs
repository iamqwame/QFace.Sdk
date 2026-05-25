using System.Security.Cryptography;
using System.Text;

namespace QimErp.Shared.Common.Utils;

public static class PasswordGenerator
{
    private const string Lower   = "abcdefghjkmnpqrstuvwxyz";
    private const string Upper   = "ABCDEFGHJKLMNPQRSTUVWXYZ";
    private const string Digits  = "23456789";
    private const string Symbols = "!@#$%^&*";

    public static string Generate(int length = 16)
    {
        if (length < 8) length = 8;

        var sb = new StringBuilder(length);
        sb.Append(PickOne(Upper));
        sb.Append(PickOne(Lower));
        sb.Append(PickOne(Digits));
        sb.Append(PickOne(Symbols));

        var pool = Lower + Upper + Digits + Symbols;
        for (var i = sb.Length; i < length; i++)
        {
            sb.Append(PickOne(pool));
        }

        var chars = sb.ToString().ToCharArray();
        for (var i = chars.Length - 1; i > 0; i--)
        {
            var j = RandomNumberGenerator.GetInt32(0, i + 1);
            (chars[i], chars[j]) = (chars[j], chars[i]);
        }

        return new string(chars);
    }

    private static char PickOne(string pool) => pool[RandomNumberGenerator.GetInt32(0, pool.Length)];
}
