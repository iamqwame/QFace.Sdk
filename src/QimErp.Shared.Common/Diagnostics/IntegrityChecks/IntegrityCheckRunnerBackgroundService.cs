using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using QFace.Sdk.Temporal.Interceptors;
using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Diagnostics.IntegrityChecks;

/// <summary>
/// Runs every registered <see cref="IIntegrityCheck"/> on a timer and logs findings as the
/// DATA-06 "data_integrity_found" signal. Mirrors the poll / scope / process / sleep shape of
/// <see cref="Outbox.OutboxProcessorBackgroundService{TContext}"/>.
/// </summary>
public sealed class IntegrityCheckRunnerBackgroundService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<IntegrityCheckRunnerBackgroundService> _logger;
    private readonly IntegrityCheckRunnerOptions _options;

    public IntegrityCheckRunnerBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<IntegrityCheckRunnerBackgroundService> logger,
        IOptions<IntegrityCheckRunnerOptions> options)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _options = options.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation(
            "Integrity check runner started. IntervalSeconds={IntervalSeconds}", _options.PollingIntervalSeconds);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await RunAllChecksAsync(stoppingToken);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Integrity check runner sweep failed");
            }

            try
            {
                await Task.Delay(TimeSpan.FromSeconds(_options.PollingIntervalSeconds), stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        _logger.LogInformation("Integrity check runner stopped");
    }

    /// <summary>
    /// Runs every registered <see cref="IIntegrityCheck"/> once. Public so tests can drive a sweep
    /// deterministically without running the hosted-service loop.
    /// </summary>
    public async Task<IReadOnlyList<IntegrityCheckResult>> RunAllChecksAsync(CancellationToken cancellationToken)
    {
        int checkCount;
        using (var listScope = _scopeFactory.CreateScope())
        {
            checkCount = listScope.ServiceProvider.GetServices<IIntegrityCheck>().Count();
        }

        var results = new List<IntegrityCheckResult>(checkCount);
        for (var i = 0; i < checkCount; i++)
        {
            var result = await RunSingleCheckAsync(i, cancellationToken);
            if (result is null)
                continue;

            results.Add(result);

            if (result.HasFindings)
                _logger.LogWarning(
                    "data_integrity_found check={Check} count={Count} sampleIds={SampleIds}",
                    result.CheckName, result.Count, result.SampleIds);
            else
                _logger.LogInformation("Integrity check {Check} found no issues", result.CheckName);
        }

        return results;
    }

    private async Task<IntegrityCheckResult?> RunSingleCheckAsync(int checkIndex, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var userContextService = scope.ServiceProvider.GetService<ICurrentUserService>();
        var tenantScopeSetter = scope.ServiceProvider.GetService<ITenantScopeSetter>();
        var companyScopeSetter = scope.ServiceProvider.GetService<ICompanyScopeSetter>();

        var checks = scope.ServiceProvider.GetServices<IIntegrityCheck>().ToList();
        if (checkIndex >= checks.Count)
            return null;

        var check = checks[checkIndex];
        try
        {
            return await check.RunAsync(cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Integrity check {Check} failed to run", check.Name);
            return null;
        }
        finally
        {
            userContextService?.ClearContext();
            tenantScopeSetter?.ClearTenantScope();
            companyScopeSetter?.ClearCompanyScope();
        }
    }
}
