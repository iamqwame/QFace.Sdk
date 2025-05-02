using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace QFace.Sdk.Logging;

public static class LoggingExtensions
{
    /// <summary>
    /// Configure Serilog + Graylog sink using a "Logs" section in IConfiguration.
    /// </summary>
    public static IHostBuilder AddQFaceLogging(this IHostBuilder hostBuilder)
    {
        return hostBuilder.UseSerilog((context, loggerConfig) =>
        {
            try
            {
                // Bind the Logs section from appsettings
                var opts = new LoggingOptions();
                ConfigurationBinder.Bind(context.Configuration.GetSection("Logs"), opts);

                if (!opts.Using.Equals("Graylog", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Unsupported logging sink: {opts.Using}");
                }

                loggerConfig
                    .MinimumLevel.Is(opts.MinimumLevel)
                    .Enrich.FromLogContext()
                    .WriteTo.Console()
                    .WriteTo.Graylog(new GraylogSinkOptions
                    {
                        HostnameOrAddress = opts.Url,
                        Port = opts.Port,
                        Facility = opts.Facility,
                        Host = Environment.MachineName,
                        TransportType = TransportType.Udp,
                        MinimumLogEventLevel = opts.MinimumLevel,
                    });
            } catch (Exception ex)
            {
                // Fallback to console logging if Graylog configuration fails
                loggerConfig
                    .MinimumLevel.Information()
                    .WriteTo.Console(outputTemplate: 
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (Fallback){NewLine}{Exception}");
                
                // Log the configuration error to console
                Console.Error.WriteLine($"Error configuring QFace logging: {ex.Message}");
            }
        });}
}