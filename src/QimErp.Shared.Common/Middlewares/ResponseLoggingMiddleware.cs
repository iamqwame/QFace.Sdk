namespace QimErp.Shared.Common.Middlewares;

public class ResponseLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public bool LogResponseBody { get; set; } = true;
    public bool LogResponseHeaders { get; set; } = true;
    public int MaxBodyLength { get; set; } = 4096; // 4KB limit
    public string[] ExcludedPaths { get; set; } = { "/health", "/metrics", "/favicon.ico" };
    public int[] ExcludedStatusCodes { get; set; } = { 404, 405 }; // Don't log 404s and 405s
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}

public class ResponseLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ResponseLoggingMiddleware> _logger;
    private readonly ResponseLoggingOptions _options;

    public ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger, ResponseLoggingOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new ResponseLoggingOptions();
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            // Check if logging is disabled
            if (!_options.Enabled)
            {
                await _next(context);
                return;
            }

            // Check if path should be excluded
            if (ShouldExcludePath(context.Request.Path))
            {
                await _next(context);
                return;
            }

            // Capture the original response body stream
            var originalBodyStream = context.Response.Body;

            using var memoryStream = new MemoryStream();
            context.Response.Body = memoryStream;

            // Continue with the request pipeline
            await _next(context);

            // Log response details
            await LogResponseAsync(context, memoryStream, originalBodyStream);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing response for {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            throw;
        }
    }

    private bool ShouldExcludePath(PathString path)
    {
        return _options.ExcludedPaths.Any(excludedPath => 
            path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase));
    }

    private bool ShouldExcludeStatusCode(int statusCode)
    {
        return _options.ExcludedStatusCodes.Contains(statusCode);
    }

    private async Task LogResponseAsync(HttpContext context, MemoryStream memoryStream, Stream originalBodyStream)
    {
        var response = context.Response;
        var request = context.Request;

        // Check if status code should be excluded
        if (ShouldExcludeStatusCode(response.StatusCode))
        {
            // Still need to copy the response back to the original stream
            memoryStream.Position = 0;
            await memoryStream.CopyToAsync(originalBodyStream);
            return;
        }

        // Log basic response information
        var logMessage = $"Response: {response.StatusCode} for {request.Method} {request.Path}";
        _logger.Log(_options.LogLevel, logMessage);

        // Log response headers
        if (_options.LogResponseHeaders)
        {
            var headers = response.Headers.ToDictionary(h => h.Key, h => h.Value.ToString());
            if (headers.Any())
            {
                _logger.LogDebug("Response Headers: {@Headers}", headers);
            }
        }

        // Log response body
        if (_options.LogResponseBody && ShouldLogResponseBody(response.ContentType))
        {
            var body = await GetResponseBodyAsync(memoryStream);
            if (!string.IsNullOrEmpty(body))
            {
                // Truncate body if it's too long
                if (body.Length > _options.MaxBodyLength)
                {
                    body = body[.._options.MaxBodyLength] + "... [truncated]";
                }

                _logger.Log(_options.LogLevel, "Response Body: {Body}", body);
            }
        }

        // Copy the response back to the original stream
        memoryStream.Position = 0;
        await memoryStream.CopyToAsync(originalBodyStream);
    }

    private static bool ShouldLogResponseBody(string? contentType)
    {
        if (string.IsNullOrEmpty(contentType))
            return false;

        return contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("text/", StringComparison.OrdinalIgnoreCase) ||
               contentType.Contains("application/xml", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<string> GetResponseBodyAsync(MemoryStream memoryStream)
    {
        try
        {
            memoryStream.Position = 0;
            using var reader = new StreamReader(memoryStream, Encoding.UTF8, leaveOpen: true);
            var body = await reader.ReadToEndAsync();

            // Try to format JSON for better readability
            if (!string.IsNullOrEmpty(body))
            {
                try
                {
                    var jsonDocument = JsonDocument.Parse(body);
                    return JsonSerializer.Serialize(jsonDocument, new JsonSerializerOptions 
                    { 
                        WriteIndented = true 
                    });
                }
                catch
                {
                    // If JSON parsing fails, return the original body
                    return body;
                }
            }

            return body;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to read response body");
            return string.Empty;
        }
    }
}

// Extension method for easy registration
public static class ResponseLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseResponseLogging(this IApplicationBuilder builder, Action<ResponseLoggingOptions>? configureOptions = null)
    {
        var options = new ResponseLoggingOptions();
        configureOptions?.Invoke(options);
        
        return builder.UseMiddleware<ResponseLoggingMiddleware>(options);
    }

    public static IApplicationBuilder UseResponseLogging(this IApplicationBuilder builder, IConfiguration configuration)
    {
        var options = new ResponseLoggingOptions();
        configuration.GetSection("Logging:ResponseLogging").Bind(options);
        
        return builder.UseMiddleware<ResponseLoggingMiddleware>(options);
    }

    public static IApplicationBuilder UseResponseLogging(this IApplicationBuilder builder, IConfiguration configuration, Action<ResponseLoggingOptions>? configureOptions = null)
    {
        var options = new ResponseLoggingOptions();
        configuration.GetSection("Logging:ResponseLogging").Bind(options);
        configureOptions?.Invoke(options);
        
        return builder.UseMiddleware<ResponseLoggingMiddleware>(options);
    }
} 