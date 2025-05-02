using Serilog.Events;

namespace QFace.Sdk.Logging;

/// <summary>
/// Configuration options for QFace logging with Graylog
/// </summary>
public class LoggingOptions
{
    /// <summary>
    /// Graylog server URL
    /// </summary>
    public string Url { get; set; } = "localhost";
    public string Using { get; set; } = "Graylog";
    
    /// <summary>
    /// Graylog UDP port
    /// </summary>
    public int Port { get; set; } = 12201;
    
    /// <summary>
    /// Application name for logging identification
    /// </summary>
    public string Facility { get; set; } = "QFace.Sdk.Logging";
    
    /// <summary>
    /// Minimum level of logs to capture
    /// </summary>
    public LogEventLevel MinimumLevel { get; set; } = LogEventLevel.Information;
    
    /// <summary>
    /// Whether to include console logging alongside Graylog
    /// </summary>
    public bool IncludeConsole { get; set; } = true;
}