using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Sinks.Graylog;
using Serilog.Sinks.Graylog.Core.Transport;

namespace QFace.Sdk.Logging;

/// <summary>
/// 
/// </summary>
public static class LoggingExtensions
    {
        /// <summary>
        /// Configure Serilog + Graylog sink using a "Logs" section in IConfiguration.
        /// Works for both Web API and Console applications.
        /// </summary>
        public static IHostBuilder AddQFaceLogging(this IHostBuilder hostBuilder)
        {
            return hostBuilder.UseSerilog((context, loggerConfig) =>
            {
                ConfigureLogging(context.Configuration, loggerConfig);
            });
        }

        /// <summary>
        /// Configure Serilog + Graylog sink directly with an IConfiguration instance.
        /// Useful for console applications where you need to configure logging before host building.
        /// </summary>
        public static void ConfigureQFaceLogging(this IConfiguration configuration)
        {
            var loggerConfig = new LoggerConfiguration();
            ConfigureLogging(configuration, loggerConfig);
            Log.Logger = loggerConfig.CreateLogger();
        }

        private static void ConfigureLogging(IConfiguration configuration, LoggerConfiguration loggerConfig)
        {
            try
            {
                // Bind the Logs section from appsettings
                var opts = new LoggingOptions();
                configuration.GetSection("Logs").Bind(opts);

                if (!opts.Using.Equals("Graylog", StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException($"Unsupported logging sink: {opts.Using}");
                }

                // Parse transport type from configuration
                var transportType = opts.TransportType?.Equals("Http", StringComparison.OrdinalIgnoreCase) == true
                    ? TransportType.Http
                    : TransportType.Udp;

                var transportTypeName = transportType == TransportType.Http ? "HTTP" : "UDP";
                Console.WriteLine($"[QFace.Logging] Configuring Graylog sink: {opts.Url}:{opts.Port} using {transportTypeName} transport");

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
                        TransportType = transportType,
                        MinimumLogEventLevel = opts.MinimumLevel,
                    });
            }
            catch (Exception ex)
            {
                // Fallback to console logging if Graylog configuration fails
                loggerConfig
                    .MinimumLevel.Information()
                    .WriteTo.Console(outputTemplate:
                        "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (Fallback){NewLine}{Exception}");

                // Log the configuration error to console
                Console.Error.WriteLine($"Error configuring QFace logging: {ex.Message}");
            }
        }
    }