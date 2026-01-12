namespace QimErp.Shared.Common.Middlewares;

public class RequestLoggingOptions
{
    public bool Enabled { get; set; } = true;
    public bool LogRequestBody { get; set; } = true;
    public bool LogRequestHeaders { get; set; } = true;
    public bool LogQueryString { get; set; } = true;
    public int MaxBodyLength { get; set; } = 4096; // 4KB limit
    public string[] ExcludedPaths { get; set; } = { "/health", "/metrics", "/favicon.ico" };
    public string[] SensitiveHeaders { get; set; } = { "Authorization", "Cookie", "X-API-Key", "X-Auth-Token" };
    public LogLevel LogLevel { get; set; } = LogLevel.Information;
}

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;
    private readonly RequestLoggingOptions _options;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger, RequestLoggingOptions? options = null)
    {
        _next = next;
        _logger = logger;
        _options = options ?? new RequestLoggingOptions();
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

            // Log request details
            await LogRequestAsync(context);
            
            // Continue with the request pipeline
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing request {Method} {Path}", 
                context.Request.Method, context.Request.Path);
            throw;
        }
    }

    private bool ShouldExcludePath(PathString path)
    {
        return _options.ExcludedPaths.Any(excludedPath => 
            path.StartsWithSegments(excludedPath, StringComparison.OrdinalIgnoreCase));
    }

    private async Task LogRequestAsync(HttpContext context)
    {
        var request = context.Request;
        
        // Log basic request information
        var logMessage = $"Request: {request.Method} {request.Path}";
        
        if (_options.LogQueryString && !string.IsNullOrEmpty(request.QueryString.Value))
        {
            logMessage += $" {request.QueryString}";
        }
        
        logMessage += $" from {context.Connection.RemoteIpAddress}";
        
        _logger.Log(_options.LogLevel, logMessage);

        // Log headers (excluding sensitive ones)
        if (_options.LogRequestHeaders)
        {
            var headers = request.Headers
                .Where(h => !IsSensitiveHeader(h.Key))
                .ToDictionary(h => h.Key, h => h.Value.ToString());
            
            if (headers.Any())
            {
                _logger.LogDebug("Request Headers: {@Headers}", headers);
            }
        }

        // Log request body for POST, PUT, PATCH requests
        if (_options.LogRequestBody && ShouldLogRequestBody(request.Method))
        {
            var body = await GetRequestBodyAsync(request);
            if (!string.IsNullOrEmpty(body))
            {
                // Truncate body if it's too long
                if (body.Length > _options.MaxBodyLength)
                {
                    body = body[.._options.MaxBodyLength] + "... [truncated]";
                }
                
                _logger.Log(_options.LogLevel, "Request Body: {Body}", body);
            }
        }
    }

    private static bool ShouldLogRequestBody(string method)
    {
        return method.Equals("POST", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PUT", StringComparison.OrdinalIgnoreCase) ||
               method.Equals("PATCH", StringComparison.OrdinalIgnoreCase);
    }

    private bool IsSensitiveHeader(string headerName)
    {
        return _options.SensitiveHeaders.Any(h => headerName.Equals(h, StringComparison.OrdinalIgnoreCase));
    }

    private async Task<string> GetRequestBodyAsync(HttpRequest request)
    {
        try
        {
            // Enable buffering so we can read the body multiple times
            request.EnableBuffering();

            using var reader = new StreamReader(
                request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            
            // Reset the request body position so it can be read again by the actual handler
            request.Body.Position = 0;

            // Try to format JSON for better readability
            if (IsJsonContent(request.ContentType) && !string.IsNullOrEmpty(body))
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
            _logger.LogWarning(ex, "Failed to read request body");
            return string.Empty;
        }
    }

    private static bool IsJsonContent(string? contentType)
    {
        return !string.IsNullOrEmpty(contentType) && 
               contentType.Contains("application/json", StringComparison.OrdinalIgnoreCase);
    }
}

// Extension method for easy registration
public static class RequestLoggingMiddlewareExtensions
{
    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, Action<RequestLoggingOptions>? configureOptions = null)
    {
        var options = new RequestLoggingOptions();
        configureOptions?.Invoke(options);
        
        return builder.UseMiddleware<RequestLoggingMiddleware>(options);
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, IConfiguration configuration)
    {
        // var options = new RequestLoggingOptions();
        // configuration.GetSection("Logging:RequestLogging").Bind(options);
        //
        // return builder.UseMiddleware<RequestLoggingMiddleware>(options);

        return builder;
    }

    public static IApplicationBuilder UseRequestLogging(this IApplicationBuilder builder, IConfiguration configuration, Action<RequestLoggingOptions>? configureOptions = null)
    {
        // var options = new RequestLoggingOptions();
        // configuration.GetSection("Logging:RequestLogging").Bind(options);
        // configureOptions?.Invoke(options);
        //
        // return builder.UseMiddleware<RequestLoggingMiddleware>(options);
        return builder;
    }
} 
