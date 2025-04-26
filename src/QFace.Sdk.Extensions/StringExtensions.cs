using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;

namespace QFace.Sdk.Extensions;

/// <summary>
/// Extension methods for string operations
/// </summary>
public static class StringExtensions
{
    /// <summary>
    /// Checks if a string is null, empty, or consists only of white-space characters.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is null, empty, or whitespace; otherwise false.</returns>
    public static bool IsEmpty(this string? str)
    {
        return string.IsNullOrEmpty(str) || string.IsNullOrWhiteSpace(str);
    }
    
    /// <summary>
    /// Checks if a string is not null, empty, or whitespace.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <returns>True if the string is not null, empty, or whitespace; otherwise false.</returns>
    public static bool IsNotEmpty(this string? str)
    {
        return !IsEmpty(str);
    }
    
    /// <summary>
    /// Converts a string to a Guid.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A Guid parsed from the string.</returns>
    /// <exception cref="ArgumentException">Thrown when the string cannot be parsed as a Guid.</exception>
    public static Guid ToGuid(this string str)
    {
        return Guid.Parse(str);
    }
    
    /// <summary>
    /// Safely attempts to convert a string to a Guid.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <param name="result">When this method returns, contains the Guid value if conversion succeeded, or Guid.Empty if failed.</param>
    /// <returns>True if conversion succeeded; otherwise, false.</returns>
    public static bool TryToGuid(this string? str, out Guid result)
    {
        if (str.IsEmpty())
        {
            result = Guid.Empty;
            return false;
        }
        
        return Guid.TryParse(str, out result);
    }
    
    /// <summary>
    /// Truncates a string to a specified maximum length.
    /// </summary>
    /// <param name="str">The string to truncate.</param>
    /// <param name="maxLength">The maximum length.</param>
    /// <param name="suffix">An optional suffix to append when truncation occurs.</param>
    /// <returns>The truncated string.</returns>
    public static string Truncate(this string? str, int maxLength, string suffix = "...")
    {
        if (str == null) return string.Empty;
        if (str.Length <= maxLength) return str;
        
        return str.Substring(0, maxLength) + suffix;
    }
    
    /// <summary>
    /// Converts a string to title case (capitalizes the first letter of each word).
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The string in title case.</returns>
    public static string ToTitleCase(this string? str)
    {
        if (str.IsEmpty()) return string.Empty;
        
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(str.ToLower());
    }
    
    /// <summary>
    /// Strips HTML tags from a string.
    /// </summary>
    /// <param name="html">The HTML string.</param>
    /// <returns>The string with HTML tags removed.</returns>
    public static string StripHtml(this string? html)
    {
        if (html.IsEmpty()) return string.Empty;
        
        return Regex.Replace(html, "<.*?>", string.Empty);
    }
    
    /// <summary>
    /// Converts a string to a URL-friendly slug.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>A URL-friendly version of the string.</returns>
    public static string ToSlug(this string? str)
    {
        if (str.IsEmpty()) return string.Empty;
        
        // Convert to lowercase and normalize
        str = str.ToLowerInvariant();
        str = RemoveDiacritics(str);
        
        // Replace spaces and non-alphanumeric characters with hyphens
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", "-");
        str = Regex.Replace(str, @"-+", "-");
        
        // Trim hyphens from start and end
        return str.Trim('-');
    }
    
    /// <summary>
    /// Removes diacritics (accents) from a string.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>The text without diacritics.</returns>
    private static string RemoveDiacritics(string text)
    {
        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
    
    /// <summary>
    /// Generates an MD5 hash of a string.
    /// </summary>
    /// <param name="str">The string to hash.</param>
    /// <returns>The MD5 hash as a hexadecimal string.</returns>
    public static string ToMd5Hash(this string? str)
    {
        if (str.IsEmpty()) return string.Empty;
        
        using var md5 = MD5.Create();
        var inputBytes = Encoding.UTF8.GetBytes(str);
        var hashBytes = md5.ComputeHash(inputBytes);
        
        var sb = new StringBuilder();
        foreach (var hashByte in hashBytes)
        {
            sb.Append(hashByte.ToString("x2"));
        }
        
        return sb.ToString();
    }
    
    /// <summary>
    /// Extracts the first paragraph from a string.
    /// </summary>
    /// <param name="text">The text to process.</param>
    /// <returns>The first paragraph of the text.</returns>
    public static string FirstParagraph(this string? text)
    {
        if (text.IsEmpty()) return string.Empty;
        
        var match = Regex.Match(text, @"^\s*(.+?)(\r\n|\n|$)");
        return match.Success ? match.Groups[1].Value.Trim() : text;
    }
    
    /// <summary>
    /// Checks if a string is a valid email address.
    /// </summary>
    /// <param name="email">The email address to validate.</param>
    /// <returns>True if the string is a valid email address; otherwise, false.</returns>
    public static bool IsValidEmail(this string? email)
    {
        if (email.IsEmpty()) return false;
        
        // Simple regex for basic email validation
        var pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern);
    }
    
    /// <summary>
    /// Converts a camelCase or PascalCase string to snake_case.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The string in snake_case.</returns>
    public static string ToSnakeCase(this string? str)
    {
        if (str.IsEmpty()) return string.Empty;
        
        return Regex.Replace(str, "([a-z])([A-Z])", "$1_$2").ToLower();
    }
    
    /// <summary>
    /// Converts a snake_case string to PascalCase.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The string in PascalCase.</returns>
    public static string ToPascalCase(this string? str)
    {
        if (str.IsEmpty()) return string.Empty;
        
        return string.Join("", str.Split('_')
            .Select(word => char.ToUpper(word[0]) + word.Substring(1).ToLower()));
    }
    
    /// <summary>
    /// Converts a snake_case string to camelCase.
    /// </summary>
    /// <param name="str">The string to convert.</param>
    /// <returns>The string in camelCase.</returns>
    public static string ToCamelCase(this string? str)
    {
        if (str.IsEmpty()) return string.Empty;
        
        var pascal = ToPascalCase(str);
        return char.ToLower(pascal[0]) + pascal.Substring(1);
    }
    
    /// <summary>
    /// Checks if a string matches a wildcard pattern.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <param name="pattern">The wildcard pattern (using * and ? as wildcards).</param>
    /// <param name="ignoreCase">Whether to ignore case when matching.</param>
    /// <returns>True if the string matches the pattern; otherwise, false.</returns>
    public static bool MatchesWildcard(this string? str, string pattern, bool ignoreCase = true)
    {
        if (str == null) return false;
        if (pattern == null) return false;
        
        // Convert wildcard to regex
        pattern = Regex.Escape(pattern).Replace("\\*", ".*").Replace("\\?", ".");
        
        var regexOptions = RegexOptions.Singleline;
        if (ignoreCase)
        {
            regexOptions |= RegexOptions.IgnoreCase;
        }
        
        return Regex.IsMatch(str, $"^{pattern}$", regexOptions);
    }
    
    /// <summary>
    /// Checks if a string is a valid URL.
    /// </summary>
    /// <param name="url">The URL to validate.</param>
    /// <returns>True if the string is a valid URL; otherwise, false.</returns>
    public static bool IsValidUrl(this string? url)
    {
        if (url.IsEmpty()) return false;
        
        return Uri.TryCreate(url, UriKind.Absolute, out var uriResult) 
               && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
    }
    
    /// <summary>
    /// Checks if an object is not found (null).
    /// </summary>
    /// <param name="obj">The object to check.</param>
    /// <returns>True if the object is null; otherwise, false.</returns>
    public static bool IsNotFound(this object? obj)
    {
        return obj == null;
    }
    
    /// <summary>
    /// Returns a default value if the string is empty.
    /// </summary>
    /// <param name="str">The string to check.</param>
    /// <param name="defaultValue">The default value to return if the string is empty.</param>
    /// <returns>The original string if not empty; otherwise, the default value.</returns>
    public static string DefaultIfEmpty(this string? str, string defaultValue)
    {
        return str.IsEmpty() ? defaultValue : str;
    }

    /// <summary>
    /// Checks if a list should fetch a specific metric.
    /// </summary>
    /// <param name="metrics">The list of metrics to check.</param>
    /// <param name="metricName">The name of the metric.</param>
    /// <returns>True if the metric should be fetched; otherwise, false.</returns>
    public static bool ShouldFetchMetrics(this List<string>? metrics, string metricName)
    {
        return metrics == null || metrics.Contains(metricName);
    }
    
}

