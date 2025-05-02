# QFace Logging SDK Documentation

The QFace Logging SDK provides a simple and robust solution for adding structured logging to your .NET applications. Built on top of Serilog with Graylog integration, it offers a consistent logging approach with minimal configuration.

## Table of Contents

1. [Installation](#installation)
2. [Configuration](#configuration)
3. [Basic Usage](#basic-usage)
4. [Configuration Options](#configuration-options)
5. [Error Handling](#error-handling)
6. [Best Practices](#best-practices)
7. [Troubleshooting](#troubleshooting)

## Installation

Install the package via NuGet:

```bash
dotnet add package QFace.Sdk.Logging
```

Add the following namespaces to your files:

```csharp
using QFace.Sdk.Logging;
using Serilog;
```

## Configuration

### appsettings.json

Add the following configuration to your `appsettings.json` file:

```json
{
  "Logs": {
    "Url": "graylog-server.example.com",
    "Port": 12201,
    "Facility": "YourAppName",
    "MinimumLevel": "Information",
    "IncludeConsole": true,
    "Using": "Graylog"
  }
}
```

## Basic Usage

### Integrating with Host Builder

The simplest way to add QFace Logging to your application is through the host builder extension method in your `Program.cs` or `Startup.cs`:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .AddQFaceLogging()  // Uses "Logs" section from appsettings.json
    .ConfigureServices((hostContext, services) =>
    {
        // Register your services here
    })
    .Build();
```

### Using Logger in Your Services

After configuring QFace Logging, you can inject and use the Serilog logger in your services:

```csharp
public class OrderService
{
    private readonly ILogger _logger;

    public OrderService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task<Order> CreateOrderAsync(Order order)
    {
        try
        {
            _logger.Information("Creating new order {OrderId} for customer {CustomerId}",
                order.Id, order.CustomerId);

            // Process the order...

            _logger.Information("Successfully created order {OrderId}", order.Id);
            return order;
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Failed to create order for customer {CustomerId}", order.CustomerId);
            throw;
        }
    }
}
```

### Using Context Enrichment

Serilog supports enriching log events with contextual information:

```csharp
// Add properties to all subsequent log events in the context
using (LogContext.PushProperty("TransactionId", transactionId))
{
    _logger.Information("Beginning transaction");

    // All log events within this scope will have the TransactionId property
    _logger.Information("Processing items");

    _logger.Information("Transaction complete");
}
```

### Structured Logging

Always use structured logging with named parameters rather than string concatenation:

```csharp
// DO THIS: Structured logging with named parameters
_logger.Information("User {UserId} performed {Action} on {Resource}",
    userId, "update", "product");

// DON'T DO THIS: String concatenation
_logger.Information($"User {userId} performed update on product");  // Avoid this!
```

## Configuration Options

### LoggingOptions Properties

The following options can be configured either in `appsettings.json` or programmatically:

| Property       | Description                                                     | Default                   |
| -------------- | --------------------------------------------------------------- | ------------------------- |
| Url            | Graylog server URL                                              | "localhost"               |
| Port           | Graylog UDP port                                                | 12201                     |
| Facility       | Application name for logging identification                     | "QFace.Sdk.Logging"       |
| MinimumLevel   | Minimum level of logs to capture                                | LogEventLevel.Information |
| IncludeConsole | Whether to include console logging alongside Graylog            | true                      |
| Using          | Logging provider to use (currently only "Graylog" is supported) | "Graylog"                 |

### Programmatic Configuration

You can also configure QFace Logging programmatically:

```csharp
var host = Host.CreateDefaultBuilder(args)
    .AddQFaceLogging(new LoggingOptions
    {
        Url = "graylog.mycompany.com",
        Port = 12201,
        Facility = "MyQFaceApp",
        MinimumLevel = LogEventLevel.Debug,
        IncludeConsole = true
    })
    .ConfigureServices((hostContext, services) =>
    {
        // Register your services here
    })
    .Build();
```

## Error Handling

The QFace Logging SDK automatically handles errors during configuration and falls back to console logging if Graylog configuration fails. This ensures that your application will still log important events even if the Graylog server is unreachable.

### Fallback Mechanism

When Graylog is unavailable or misconfigured, the SDK will:

1. Log an error message to the console
2. Configure a fallback console logger
3. Continue application execution without throwing exceptions

```csharp
// This happens automatically in the SDK
try
{
    // Configure Graylog logging...
}
catch (Exception ex)
{
    // Fallback to console logging
    loggerConfig
        .MinimumLevel.Information()
        .WriteTo.Console(outputTemplate:
            "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} (Fallback){NewLine}{Exception}");

    // Log the configuration error to console
    Console.Error.WriteLine($"Error configuring QFace logging: {ex.Message}");
}
```

## Best Practices

### Log Levels

Use appropriate log levels for different scenarios:

| Level       | Usage                                                             |
| ----------- | ----------------------------------------------------------------- |
| Verbose     | Detailed debugging information (development only)                 |
| Debug       | Diagnostic information useful for debugging                       |
| Information | General information about application flow                        |
| Warning     | Potential issues or unexpected states                             |
| Error       | Problems that affect functionality but don't stop the application |
| Fatal       | Critical errors that cause the application to crash               |

### Log Level Examples

```csharp
// Verbose: Detailed tracing information
_logger.Verbose("Database connection established with connection string: {ConnectionString}", connectionString);

// Debug: Diagnostic information
_logger.Debug("Query parameters: {@QueryParams}", queryParams);

// Information: Noteworthy events in normal operation
_logger.Information("User {UserId} logged in from {IpAddress}", userId, ipAddress);

// Warning: Potential issues
_logger.Warning("API rate limit at {Percent}% of quota", usagePercent);

// Error: Problems that affect functionality
_logger.Error(exception, "Failed to process payment for order {OrderId}", orderId);

// Fatal: Application crashes or severe failures
_logger.Fatal(exception, "Unhandled exception in background service - shutting down");
```

### Contextual Information

Always include relevant context in log messages:

```csharp
// Include relevant entity IDs
_logger.Information("Processing order {OrderId} for customer {CustomerId}", orderId, customerId);

// For complex objects, use @ to destructure them
_logger.Debug("Processing payment with details: {@PaymentDetails}", paymentDetails);

// Include operation timing information
var sw = Stopwatch.StartNew();
// ... perform operation ...
sw.Stop();
_logger.Information("Operation completed in {ElapsedMs}ms", sw.ElapsedMilliseconds);
```

### Correlation IDs

Use correlation IDs to track requests across multiple services:

```csharp
// In ASP.NET Core middleware
app.Use(async (context, next) =>
{
    var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
        ?? Guid.NewGuid().ToString();

    using (LogContext.PushProperty("CorrelationId", correlationId))
    {
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        await next();
    }
});
```

### Sensitive Information

Never log sensitive information:

```csharp
// DON'T DO THIS: Logging sensitive information
_logger.Information("User {Email} logged in with password {Password}", email, password);

// DO THIS: Log non-sensitive information
_logger.Information("User {UserId} logged in successfully", userId);
```

## Troubleshooting

### Common Issues

#### UDP Not Reaching Graylog

If logs are not appearing in Graylog:

1. Check if UDP port 12201 (or your configured port) is open on the Graylog server
2. Verify that the Graylog server has an input configured for GELF UDP on the specified port
3. Check network connectivity between your app and the Graylog server

#### High CPU or Memory Usage

Logging at too low a level (e.g., Verbose or Debug) in production can lead to performance issues:

1. Set MinimumLevel to Information or higher in production
2. Use conditional compilation for verbose logging in development:

```csharp
#if DEBUG
    loggerConfig.MinimumLevel.Debug();
#else
    loggerConfig.MinimumLevel.Information();
#endif
```

#### Missing Log Events

If some log events are missing:

1. Check the minimum log level configuration
2. Verify that the events are being logged at an appropriate level
3. Check for package conflicts with other logging libraries

### Checking Configuration

To verify your logging configuration is correct:

```csharp
public void VerifyLoggingConfiguration(IConfiguration configuration)
{
    var options = new LoggingOptions();
    configuration.GetSection("Logs").Bind(options);

    Console.WriteLine($"Graylog URL: {options.Url}");
    Console.WriteLine($"Graylog Port: {options.Port}");
    Console.WriteLine($"Facility: {options.Facility}");
    Console.WriteLine($"MinimumLevel: {options.MinimumLevel}");
    Console.WriteLine($"Using: {options.Using}");
    Console.WriteLine($"IncludeConsole: {options.IncludeConsole}");
}
```

### Testing Logging Configuration

Add a simple test method to verify logging is working:

```csharp
public static void TestLogging(ILogger logger)
{
    logger.Information("This is a test information message");
    logger.Warning("This is a test warning message");
    logger.Error("This is a test error message");

    try
    {
        throw new Exception("Test exception");
    }
    catch (Exception ex)
    {
        logger.Error(ex, "Caught test exception");
    }
}
```

## Complete Example

Here's a complete example of using QFace.Sdk.Logging in an ASP.NET Core application:

```csharp
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QFace.Sdk.Logging;
using Serilog;
using Serilog.Context;
using System;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            Log.Information("Starting web application");
            CreateHostBuilder(args).Build().Run();
        }
        catch (Exception ex)
        {
            Log.Fatal(ex, "Application terminated unexpectedly");
        }
        finally
        {
            Log.CloseAndFlush();
        }
    }

    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .AddQFaceLogging()
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
            });
}

public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddControllers();

        // Register other services
        services.AddTransient<IOrderService, OrderService>();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        // Add correlation ID middleware
        app.Use(async (context, next) =>
        {
            var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
                ?? Guid.NewGuid().ToString();

            using (LogContext.PushProperty("CorrelationId", correlationId))
            {
                context.Response.Headers.Add("X-Correlation-ID", correlationId);
                await next();
            }
        });

        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }

        app.UseRouting();
        app.UseAuthorization();

        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllers();
        });
    }
}

public class OrderController
{
    private readonly IOrderService _orderService;
    private readonly ILogger _logger;

    public OrderController(IOrderService orderService, ILogger logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(string id)
    {
        _logger.Information("Retrieving order {OrderId}", id);

        try
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                _logger.Warning("Order {OrderId} not found", id);
                return NotFound();
            }

            _logger.Information("Successfully retrieved order {OrderId}", id);
            return Ok(order);
        }
        catch (Exception ex)
        {
            _logger.Error(ex, "Error retrieving order {OrderId}", id);
            return StatusCode(500, "An error occurred while retrieving the order");
        }
    }
}
```
