using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace QFace.Sdk.Extensions;

/// <summary>
/// Extension methods for HTTP-related operations
/// </summary>
public static class HttpExtensions
{
    /// <summary>
    /// Gets the client IP address from an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The client IP address.</returns>
    public static string GetClientIpAddress(this HttpContext context)
    {
        string ip = context.Request.Headers["X-Forwarded-For"];
        
        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Request.Headers["REMOTE_ADDR"];
        }
        
        if (string.IsNullOrEmpty(ip))
        {
            ip = context.Connection.RemoteIpAddress?.ToString();
        }
        
        return ip ?? "0.0.0.0";
    }
    
    /// <summary>
    /// Gets a query string value from an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">The query string key.</param>
    /// <returns>The value associated with the key, or null if not found.</returns>
    public static string? GetQueryString(this HttpContext context, string key)
    {
        if (context.Request.Query.TryGetValue(key, out StringValues values))
        {
            return values.FirstOrDefault();
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets a query string value from an HTTP context and parses it as the specified type.
    /// </summary>
    /// <typeparam name="T">The type to parse the value as.</typeparam>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">The query string key.</param>
    /// <param name="defaultValue">The default value to return if the key is not found or the value cannot be parsed.</param>
    /// <returns>The parsed value, or the default value if parsing fails.</returns>
    public static T GetQueryString<T>(this HttpContext context, string key, T defaultValue)
    {
        var value = context.GetQueryString(key);
        if (string.IsNullOrEmpty(value)) return defaultValue;
        
        try
        {
            return (T)Convert.ChangeType(value, typeof(T));
        }
        catch
        {
            return defaultValue;
        }
    }
    
    /// <summary>
    /// Gets a header value from an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">The header key.</param>
    /// <returns>The value associated with the key, or null if not found.</returns>
    public static string? GetHeaderValue(this HttpContext context, string key)
    {
        if (context.Request.Headers.TryGetValue(key, out StringValues values))
        {
            return values.FirstOrDefault();
        }
        
        return null;
    }
    
    /// <summary>
    /// Gets the base URL from an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <returns>The base URL.</returns>
    public static string GetBaseUrl(this HttpContext context)
    {
        var request = context.Request;
        var host = request.Host.ToUriComponent();
        var pathBase = request.PathBase.ToUriComponent();
        var scheme = request.Scheme;
        
        return $"{scheme}://{host}{pathBase}";
    }
    
    /// <summary>
    /// Gets the current URL from an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="includeQueryString">Whether to include the query string.</param>
    /// <returns>The current URL.</returns>
    public static string GetCurrentUrl(this HttpContext context, bool includeQueryString = true)
    {
        var request = context.Request;
        var host = request.Host.ToUriComponent();
        var pathBase = request.PathBase.ToUriComponent();
        var path = request.Path.ToUriComponent();
        var queryString = includeQueryString ? request.QueryString.ToUriComponent() : string.Empty;
        var scheme = request.Scheme;
        
        return $"{scheme}://{host}{pathBase}{path}{queryString}";
    }
    
    /// <summary>
    /// Sets a response cookie in an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">The cookie key.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="expiresDays">The number of days until the cookie expires.</param>
    /// <param name="isEssential">Whether the cookie is essential for the site's functionality.</param>
    /// <param name="isSecure">Whether the cookie should only be sent over HTTPS.</param>
    /// <param name="httpOnly">Whether the cookie is accessible only through HTTP.</param>
    public static void SetCookie(this HttpContext context, string key, string value, int expiresDays = 30, 
        bool isEssential = true, bool isSecure = true, bool httpOnly = true)
    {
        var options = new CookieOptions
        {
            Expires = DateTime.UtcNow.AddDays(expiresDays),
            IsEssential = isEssential,
            Secure = isSecure,
            HttpOnly = httpOnly,
            SameSite = SameSiteMode.Lax
        };
        
        context.Response.Cookies.Append(key, value, options);
    }
    
    /// <summary>
    /// Gets a cookie value from an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">The cookie key.</param>
    /// <returns>The cookie value, or null if not found.</returns>
    public static string? GetCookie(this HttpContext context, string key)
    {
        return context.Request.Cookies[key];
    }
    
    /// <summary>
    /// Deletes a cookie from an HTTP context.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="key">The cookie key.</param>
    public static void DeleteCookie(this HttpContext context, string key)
    {
        context.Response.Cookies.Delete(key);
    }
    
    /// <summary>
    /// Redirects to another URL.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="url">The URL to redirect to.</param>
    /// <param name="permanent">Whether the redirect is permanent (301 vs 302).</param>
    public static void Redirect(this HttpContext context, string url, bool permanent = false)
    {
        context.Response.Redirect(url, permanent);
    }
    
    /// <summary>
    /// Writes a string to the HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="content">The content to write.</param>
    /// <param name="contentType">The content type (default is "text/plain").</param>
    /// <param name="statusCode">The HTTP status code (default is 200).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteStringAsync(this HttpContext context, string content, 
        string contentType = "text/plain", int statusCode = 200)
    {
        context.Response.ContentType = contentType;
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(content);
    }
    
    /// <summary>
    /// Writes JSON to the HTTP response.
    /// </summary>
    /// <typeparam name="T">The type of object to serialize.</typeparam>
    /// <param name="context">The HTTP context.</param>
    /// <param name="obj">The object to serialize and write.</param>
    /// <param name="statusCode">The HTTP status code (default is 200).</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static async Task WriteJsonAsync<T>(this HttpContext context, T obj, int statusCode = 200)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsync(obj.Serialize());
    }
    
    /// <summary>
    /// Returns a "404 Not Found" response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="message">The error message (default is "Resource not found").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task NotFoundAsync(this HttpContext context, string message = "Resource not found")
    {
        return context.WriteJsonAsync(new { error = message }, 404);
    }
    
    /// <summary>
    /// Returns a "400 Bad Request" response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="message">The error message (default is "Invalid request").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task BadRequestAsync(this HttpContext context, string message = "Invalid request")
    {
        return context.WriteJsonAsync(new { error = message }, 400);
    }
    
    /// <summary>
    /// Returns a "500 Internal Server Error" response.
    /// </summary>
    /// <param name="context">The HTTP context.</param>
    /// <param name="message">The error message (default is "An error occurred").</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    public static Task ServerErrorAsync(this HttpContext context, string message = "An error occurred")
    {
        return context.WriteJsonAsync(new { error = message }, 500);
    }
}