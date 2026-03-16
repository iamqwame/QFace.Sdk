using QimErp.Shared.Common.Database;

namespace QimErp.Shared.Common.Extensions;

/// <summary>
/// Extends AddDbContextWithOutbox to auto-wire the Temporal bridge
/// when "Temporal:Address" is present in configuration.
///
/// This file adds no new public API — it just hooks into the existing
/// AddDbContextWithOutbox call so every module WebApi gets Temporal for free.
///
/// Add a single call at the bottom of AddDbContextWithOutbox:
///
///   if (configuration != null)
///       services.AddTemporalWorkflow(configuration);
///
/// That single line is the only change required in SharedServiceCollectionExtensions.cs.
/// The extension itself lives in TemporalServiceCollectionExtensions.cs.
/// </summary>
public static class DbContextWithOutboxTemporalExtension
{
    /// <summary>
    /// Convenience wrapper — calls AddDbContextWithOutbox then conditionally
    /// wires Temporal.  Use this in any module WebApi Program.cs to get both
    /// in one call:
    ///
    ///   services.AddDbContextWithOutboxAndTemporal&lt;HrApplicationDbContext&gt;(
    ///       connectionString, configuration);
    ///
    /// If Temporal:Address is absent in config the call is a no-op for Temporal.
    /// </summary>
    public static IServiceCollection AddDbContextWithOutboxAndTemporal<TContext>(
        this IServiceCollection services,
        string connectionString,
        IConfiguration configuration)
        where TContext : ApplicationDbContext<TContext>
    {
        services.AddDbContextWithOutbox<TContext>(connectionString, configuration);
        services.AddTemporalWorkflow(configuration);
        return services;
    }
}
