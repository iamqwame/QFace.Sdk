using System.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QimErp.Shared.DataIntegrity.Discovery;
using QimErp.Shared.DataIntegrity.Results;
using QimErp.Shared.DataIntegrity.Sql;
using Temporalio.Activities;
using Temporalio.Exceptions;

namespace QimErp.Shared.DataIntegrity.Temporal;

public sealed class DataIntegrityActivities
{
    internal const string NpgsqlProvider = "Npgsql.EntityFrameworkCore.PostgreSQL";

    private static readonly TimeSpan HeartbeatInterval = TimeSpan.FromSeconds(30);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly DataIntegrityContextSet _contextSet;
    private readonly IntegrityLinkCatalog _catalog;
    private readonly DataIntegrityOptions _options;
    private readonly IReadOnlyList<IIntegrityResultSink> _sinks;
    private readonly IntegrityMetrics _metrics;
    private readonly ILogger<DataIntegrityActivities> _logger;

    internal DataIntegrityActivities(
        IServiceScopeFactory scopeFactory,
        DataIntegrityContextSet contextSet,
        IntegrityLinkCatalog catalog,
        IOptions<DataIntegrityOptions> options,
        IEnumerable<IIntegrityResultSink> sinks,
        IntegrityMetrics metrics,
        ILogger<DataIntegrityActivities> logger)
    {
        _scopeFactory = scopeFactory;
        _contextSet = contextSet;
        _catalog = catalog;
        _options = options.Value;
        _sinks = sinks.ToList();
        _metrics = metrics;
        _logger = logger;
    }

    [Activity(IntegrityNaming.ActivityName)]
    public async Task<DataIntegrityRunSummary> RunIntegrityChecksAsync(RunIntegrityChecksInput input)
    {
        var activity = ActivityExecutionContext.Current;
        var cancellationToken = activity.CancellationToken;

        if (!string.Equals(input.ModuleKey, _contextSet.ModuleKey, StringComparison.Ordinal))
        {
            throw new ApplicationFailureException(
                $"Run requested for module '{input.ModuleKey}' but this worker is registered for '{_contextSet.ModuleKey}'.",
                errorType: "DataIntegrityModuleMismatch",
                nonRetryable: true);
        }

        var progress = activity.Info.HeartbeatDetails.Count > 0
            ? await activity.Info.HeartbeatDetailAtAsync<DataIntegrityProgress>(0)
            : new DataIntegrityProgress(0, 0);

        var run = new RunState(
            activity.Info.WorkflowRunId ?? activity.Info.ActivityId,
            DateTimeOffset.UtcNow,
            Stopwatch.StartNew(),
            TimeSpan.FromMinutes(input.MaxRunMinutes > 0 ? input.MaxRunMinutes : _options.MaxRunMinutes),
            progress);

        using var heartbeatCancellation = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        var heartbeat = HeartbeatLoopAsync(activity, run, heartbeatCancellation.Token);

        try
        {
            var total = new Tally();
            var exhausted = false;

            for (var contextIndex = progress.ContextIndex; contextIndex < _contextSet.Contexts.Count && !exhausted; contextIndex++)
            {
                var start = contextIndex == progress.ContextIndex ? progress.LinkIndex : 0;
                exhausted = await RunContextAsync(contextIndex, start, run, total, cancellationToken);
            }

            if (!exhausted)
                exhausted = await RunCustomChecksAsync(progress, run, total, cancellationToken);

            var duration = run.Clock.Elapsed.TotalSeconds;
            _metrics.RecordRun(_contextSet.ModuleKey, duration, DateTimeOffset.UtcNow);

            var executed = total.Checks - total.Skipped;
            if (executed > 0 && total.Failed == executed)
            {
                throw new ApplicationFailureException(
                    $"All {executed} executed data integrity checks failed for module '{_contextSet.ModuleKey}'.",
                    errorType: "DataIntegrityAllChecksFailed",
                    nonRetryable: true);
            }

            return new DataIntegrityRunSummary(total.Checks, total.WithFindings, total.Failed, total.TimedOut, total.Skipped, total.TotalFindings);
        }
        finally
        {
            await heartbeatCancellation.CancelAsync();
            await heartbeat;
        }
    }

    private async Task<bool> RunContextAsync(int contextIndex, int startIndex, RunState run, Tally total, CancellationToken cancellationToken)
    {
        var registration = _contextSet.Contexts[contextIndex];
        var contextKey = IntegrityNaming.ContextKey(registration.ContextType);
        var info = new IntegrityRunInfo(run.RunId, run.RunAt, _contextSet.ModuleKey, contextKey);
        var tally = new Tally();
        var clock = Stopwatch.StartNew();

        using var scope = _scopeFactory.CreateScope();
        var context = ResolveContext(scope.ServiceProvider, registration);

        if (context.Database.ProviderName != NpgsqlProvider)
        {
            _logger.LogWarning(
                "data_integrity_context_skipped module={Module} context={Context} reason={Reason} runId={RunId}",
                _contextSet.ModuleKey, contextKey, "unsupported_provider", run.RunId);
            tally.Skip(1);
            total.Merge(tally);
            run.Progress = new DataIntegrityProgress(contextIndex + 1, 0);
            return false;
        }

        var scans = _catalog.Get(registration, context).Scans;
        var executor = new IntegrityCheckExecutor(_options, _logger);
        var exhausted = false;

        for (var linkIndex = startIndex; linkIndex < scans.Count; linkIndex++)
        {
            if (run.Clock.Elapsed >= run.Budget)
            {
                var remaining = scans.Skip(linkIndex).Sum(s => s.Checks.Count) + CountRemainingAfter(contextIndex);
                tally.Skip(remaining);
                _logger.LogWarning(
                    "data_integrity_budget_exhausted module={Module} context={Context} skipped={Skipped} maxRunMinutes={MaxRunMinutes} runId={RunId}",
                    _contextSet.ModuleKey, contextKey, remaining, (int)run.Budget.TotalMinutes, run.RunId);
                exhausted = true;
                break;
            }

            run.Progress = new DataIntegrityProgress(contextIndex, linkIndex);
            var outcomes = await executor.RunAsync(context, scans[linkIndex], info, cancellationToken);
            foreach (var outcome in outcomes)
            {
                await WriteAsync(info, outcome, cancellationToken);
                tally.Add(outcome);
            }

            run.Progress = new DataIntegrityProgress(contextIndex, linkIndex + 1);
        }

        if (!exhausted)
            run.Progress = new DataIntegrityProgress(contextIndex + 1, 0);

        LogRun(contextKey, tally, clock.Elapsed.TotalSeconds, run.RunId);
        total.Merge(tally);
        return exhausted;
    }

    private async Task<bool> RunCustomChecksAsync(DataIntegrityProgress progress, RunState run, Tally total, CancellationToken cancellationToken)
    {
        if (_contextSet.Checks.Count == 0)
            return false;

        var customIndex = _contextSet.Contexts.Count;
        var start = progress.ContextIndex == customIndex ? progress.LinkIndex : 0;
        var info = new IntegrityRunInfo(run.RunId, run.RunAt, _contextSet.ModuleKey, IntegrityNaming.CustomContextKey);
        var tally = new Tally();
        var clock = Stopwatch.StartNew();
        var exhausted = false;

        for (var index = start; index < _contextSet.Checks.Count; index++)
        {
            if (run.Clock.Elapsed >= run.Budget)
            {
                var remaining = _contextSet.Checks.Count - index;
                tally.Skip(remaining);
                _logger.LogWarning(
                    "data_integrity_budget_exhausted module={Module} context={Context} skipped={Skipped} maxRunMinutes={MaxRunMinutes} runId={RunId}",
                    _contextSet.ModuleKey, IntegrityNaming.CustomContextKey, remaining, (int)run.Budget.TotalMinutes, run.RunId);
                exhausted = true;
                break;
            }

            run.Progress = new DataIntegrityProgress(customIndex, index);
            var outcome = await RunCustomCheckAsync(_contextSet.Checks[index], info, cancellationToken);
            await WriteAsync(info, outcome, cancellationToken);
            tally.Add(outcome);
            run.Progress = new DataIntegrityProgress(customIndex, index + 1);
        }

        LogRun(IntegrityNaming.CustomContextKey, tally, clock.Elapsed.TotalSeconds, run.RunId);
        total.Merge(tally);
        return exhausted;
    }

    private async Task<IntegrityCheckOutcome> RunCustomCheckAsync(Type checkType, IntegrityRunInfo info, CancellationToken cancellationToken)
    {
        using var scope = _scopeFactory.CreateScope();
        var check = scope.ServiceProvider.GetService(checkType) as IDataIntegrityCheck
                    ?? throw new DataIntegrityConfigurationException([$"{checkType.Name} is not registered in the service provider."]);
        var clock = Stopwatch.StartNew();

        try
        {
            var findings = await check.RunAsync(cancellationToken);
            var count = findings.Sum(f => f.Count);
            return new IntegrityCheckOutcome(
                check.Name,
                IntegrityFindingKind.Custom,
                count > 0 ? IntegrityCheckStatus.Findings : IntegrityCheckStatus.Clean,
                count,
                findings,
                clock.Elapsed.TotalSeconds,
                null);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "data_integrity_check_error check={Check} runId={RunId}", check.Name, info.RunId);
            return new IntegrityCheckOutcome(check.Name, IntegrityFindingKind.Custom, IntegrityCheckStatus.Failed, 0, [], clock.Elapsed.TotalSeconds, "error");
        }
    }

    private int CountRemainingAfter(int contextIndex)
    {
        var remaining = _contextSet.Checks.Count;
        for (var index = contextIndex + 1; index < _contextSet.Contexts.Count; index++)
        {
            var registration = _contextSet.Contexts[index];
            using var scope = _scopeFactory.CreateScope();
            var context = ResolveContext(scope.ServiceProvider, registration);
            remaining += context.Database.ProviderName == NpgsqlProvider
                ? _catalog.Get(registration, context).CheckCount
                : 1;
        }

        return remaining;
    }

    private static DbContext ResolveContext(IServiceProvider services, DataIntegrityContextRegistration registration)
        => services.GetService(registration.ContextType) as DbContext
           ?? throw new DataIntegrityConfigurationException([$"{registration.ContextType.Name} is not registered in the service provider."]);

    private async Task WriteAsync(IntegrityRunInfo info, IntegrityCheckOutcome outcome, CancellationToken cancellationToken)
    {
        foreach (var sink in _sinks)
            await sink.WriteAsync(info, outcome, cancellationToken);
    }

    private void LogRun(string contextKey, Tally tally, double durationSeconds, string runId)
        => _logger.LogInformation(
            "data_integrity_run module={Module} context={Context} checks={Checks} withFindings={WithFindings} failed={Failed} timedOut={TimedOut} skipped={Skipped} totalFindings={TotalFindings} durationSeconds={DurationSeconds} runId={RunId}",
            _contextSet.ModuleKey, contextKey, tally.Checks, tally.WithFindings, tally.Failed, tally.TimedOut, tally.Skipped, tally.TotalFindings,
            Math.Round(durationSeconds, 1), runId);

    private static async Task HeartbeatLoopAsync(ActivityExecutionContext activity, RunState run, CancellationToken cancellationToken)
    {
        try
        {
            while (true)
            {
                await Task.Delay(HeartbeatInterval, cancellationToken);
                activity.Heartbeat(run.Progress);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
    }

    private sealed class RunState(string runId, DateTimeOffset runAt, Stopwatch clock, TimeSpan budget, DataIntegrityProgress progress)
    {
        private DataIntegrityProgress _progress = progress;

        public string RunId { get; } = runId;
        public DateTimeOffset RunAt { get; } = runAt;
        public Stopwatch Clock { get; } = clock;
        public TimeSpan Budget { get; } = budget;

        public DataIntegrityProgress Progress
        {
            get => Volatile.Read(ref _progress);
            set => Volatile.Write(ref _progress, value);
        }
    }

    private sealed class Tally
    {
        public int Checks { get; private set; }
        public int WithFindings { get; private set; }
        public int Failed { get; private set; }
        public int TimedOut { get; private set; }
        public int Skipped { get; private set; }
        public long TotalFindings { get; private set; }

        public void Add(IntegrityCheckOutcome outcome)
        {
            Checks++;
            switch (outcome.Status)
            {
                case IntegrityCheckStatus.Findings:
                    WithFindings++;
                    TotalFindings += outcome.Count;
                    break;
                case IntegrityCheckStatus.Failed:
                    Failed++;
                    break;
                case IntegrityCheckStatus.TimedOut:
                    TimedOut++;
                    break;
                case IntegrityCheckStatus.Skipped:
                    Skipped++;
                    break;
            }
        }

        public void Skip(int count)
        {
            Checks += count;
            Skipped += count;
        }

        public void Merge(Tally other)
        {
            Checks += other.Checks;
            WithFindings += other.WithFindings;
            Failed += other.Failed;
            TimedOut += other.TimedOut;
            Skipped += other.Skipped;
            TotalFindings += other.TotalFindings;
        }
    }
}
