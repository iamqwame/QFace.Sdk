using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Globalization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Logging;
using QimErp.Shared.DataIntegrity.Discovery;
using Temporalio.Exceptions;

namespace QimErp.Shared.DataIntegrity.Sql;

// Cross-tenant by design: read-only aggregate over model-derived identifiers; no request or tenant input reaches this SQL.
internal sealed class IntegrityCheckExecutor(DataIntegrityOptions options, ILogger logger)
{
    public async Task<IReadOnlyList<IntegrityCheckOutcome>> RunAsync(
        DbContext context,
        IntegrityScan scan,
        IntegrityRunInfo run,
        CancellationToken cancellationToken)
    {
        var started = Stopwatch.GetTimestamp();
        var builder = new IntegritySqlBuilder(context.GetService<ISqlGenerationHelper>());
        var accumulator = new Accumulator(options.SampleSize);
        var connection = context.Database.GetDbConnection();

        await context.Database.OpenConnectionAsync(cancellationToken);
        try
        {
            await ScanAsync(connection, builder, scan, accumulator, cancellationToken);
            return accumulator.ToOutcomes(scan, null, null, Stopwatch.GetElapsedTime(started).TotalSeconds);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (DbException ex) when (IsConnectionFailure(ex.SqlState, connection))
        {
            throw ConnectionFailure(scan, ex.SqlState);
        }
        catch (DbException ex)
        {
            var (status, reason) = ex.SqlState switch
            {
                "57014" => (IntegrityCheckStatus.TimedOut, "timeout"),
                "55P03" => (IntegrityCheckStatus.TimedOut, "lock_timeout"),
                "42P01" or "42703" => (IntegrityCheckStatus.Failed, "schema_drift"),
                _ => (IntegrityCheckStatus.Failed, "db_error")
            };

            if (reason == "db_error")
            {
                logger.LogWarning(
                    "data_integrity_check_db_error check={Check} sqlState={SqlState} runId={RunId}",
                    scan.CheckPrefix, ex.SqlState, run.RunId);
            }

            return accumulator.ToOutcomes(scan, status, reason, Stopwatch.GetElapsedTime(started).TotalSeconds);
        }
        catch (Exception ex) when (!IsConnectionFailure(null, connection))
        {
            logger.LogError(
                "data_integrity_check_error check={Check} errorType={ErrorType} runId={RunId}",
                scan.CheckPrefix, ex.GetType().Name, run.RunId);
            return accumulator.ToOutcomes(scan, IntegrityCheckStatus.Failed, "error", Stopwatch.GetElapsedTime(started).TotalSeconds);
        }
        catch (Exception ex) when (ex is not ApplicationFailureException)
        {
            throw ConnectionFailure(scan, null);
        }
        finally
        {
            await context.Database.CloseConnectionAsync();
        }
    }

    /// <summary>Lands in Temporal history: carries only the SQLSTATE and check prefix, never server text or the inner exception.</summary>
    private static ApplicationFailureException ConnectionFailure(IntegrityScan scan, string? sqlState)
        => new(
            $"Data integrity check {scan.CheckPrefix} lost its database connection (sqlState={sqlState ?? "none"}).",
            errorType: "DataIntegrityConnectionFailure",
            nonRetryable: false);

    private static bool IsConnectionFailure(string? sqlState, DbConnection connection)
        => (sqlState is not null && sqlState.StartsWith("08", StringComparison.Ordinal)) || connection.State != ConnectionState.Open;

    private async Task ScanAsync(
        DbConnection connection,
        IntegritySqlBuilder builder,
        IntegrityScan scan,
        Accumulator accumulator,
        CancellationToken cancellationToken)
    {
        var root = scan.Root;
        if (root.Key is null)
        {
            await InReadOnlyTransactionAsync(connection, builder, tx => QueryAsync(connection, tx, builder, scan, BatchRange.None, null, null, accumulator, cancellationToken), cancellationToken);
            return;
        }

        var estimate = await InReadOnlyTransactionAsync(connection, builder, async tx =>
        {
            await using var command = CreateCommand(connection, tx, builder.RowEstimate);
            AddParameter(command, IntegritySqlBuilder.SchemaParameter, root.Table.Schema, DbType.String);
            AddParameter(command, IntegritySqlBuilder.TableParameter, root.Table.Name, DbType.String);
            var value = await command.ExecuteScalarAsync(cancellationToken);
            return value is null or DBNull ? (long?)null : Convert.ToInt64(value, CultureInfo.InvariantCulture);
        }, cancellationToken);

        if (estimate is null || (estimate >= 0 && estimate <= options.BatchThresholdRows))
        {
            await InReadOnlyTransactionAsync(connection, builder, tx => QueryAsync(connection, tx, builder, scan, BatchRange.None, null, null, accumulator, cancellationToken), cancellationToken);
            return;
        }

        object? after = null;
        while (true)
        {
            var finished = await InReadOnlyTransactionAsync(connection, builder, async tx =>
            {
                object? upTo;
                await using (var bound = CreateCommand(connection, tx, builder.BatchUpperBound(root, after is not null)))
                {
                    if (after is not null)
                        AddParameter(bound, IntegritySqlBuilder.AfterParameter, after);
                    AddParameter(bound, IntegritySqlBuilder.BatchSizeParameter, options.BatchSize);
                    upTo = await bound.ExecuteScalarAsync(cancellationToken);
                }

                if (upTo is null or DBNull)
                    return true;

                var range = after is null ? BatchRange.UpTo : BatchRange.AfterAndUpTo;
                await QueryAsync(connection, tx, builder, scan, range, after, upTo, accumulator, cancellationToken);
                after = upTo;
                return false;
            }, cancellationToken);

            if (finished)
                return;
        }
    }

    private async Task InReadOnlyTransactionAsync(
        DbConnection connection,
        IntegritySqlBuilder builder,
        Func<DbTransaction, Task> body,
        CancellationToken cancellationToken)
        => await InReadOnlyTransactionAsync(connection, builder, async tx =>
        {
            await body(tx);
            return true;
        }, cancellationToken);

    private async Task<T> InReadOnlyTransactionAsync<T>(
        DbConnection connection,
        IntegritySqlBuilder builder,
        Func<DbTransaction, Task<T>> body,
        CancellationToken cancellationToken)
    {
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        await using (var readOnly = CreateCommand(connection, transaction, builder.ReadOnly))
            await readOnly.ExecuteNonQueryAsync(cancellationToken);

        await using (var limits = CreateCommand(connection, transaction, builder.SessionLimits))
        {
            AddParameter(limits, IntegritySqlBuilder.StatementTimeoutParameter,
                (options.StatementTimeoutSeconds * 1000).ToString(CultureInfo.InvariantCulture), DbType.String);
            AddParameter(limits, IntegritySqlBuilder.LockTimeoutParameter,
                (options.LockTimeoutSeconds * 1000).ToString(CultureInfo.InvariantCulture), DbType.String);
            await limits.ExecuteNonQueryAsync(cancellationToken);
        }

        var result = await body(transaction);
        await transaction.CommitAsync(cancellationToken);
        return result;
    }

    private async Task QueryAsync(
        DbConnection connection,
        DbTransaction transaction,
        IntegritySqlBuilder builder,
        IntegrityScan scan,
        BatchRange range,
        object? after,
        object? upTo,
        Accumulator accumulator,
        CancellationToken cancellationToken)
    {
        var root = scan.Root;
        var sql = scan switch
        {
            IntegrityLink link => builder.Link(link, range),
            IntegrityVersionCheck version => builder.Unversioned(version, range),
            _ => throw new InvalidOperationException($"Unsupported scan type {scan.GetType().Name}.")
        };

        await using var command = CreateCommand(connection, transaction, sql);
        if (root.Key is not null)
            AddParameter(command, IntegritySqlBuilder.SampleSizeParameter, options.SampleSize);
        if (root.Status is not null)
            AddParameter(command, IntegritySqlBuilder.DeletedRootParameter, root.Status.DeletedValue);
        if (scan is IntegrityLink { Target.Status: { } targetStatus } linkScan
            && linkScan.Checks.Any(c => c.Kind == IntegrityFindingKind.DeletedTarget))
            AddParameter(command, IntegritySqlBuilder.DeletedTargetParameter, targetStatus.DeletedValue);
        if (range == BatchRange.AfterAndUpTo)
            AddParameter(command, IntegritySqlBuilder.AfterParameter, after);
        if (range != BatchRange.None)
            AddParameter(command, IntegritySqlBuilder.UpToParameter, upTo);

        accumulator.BeginBatch();
        await using var reader = await command.ExecuteReaderAsync(cancellationToken);
        var isLink = scan is IntegrityLink;
        while (await reader.ReadAsync(cancellationToken))
        {
            var offset = isLink ? 1 : 0;
            var kind = isLink ? ParseKind(reader.GetString(0)) : IntegrityFindingKind.Unversioned;
            var tenant = reader.IsDBNull(offset) ? string.Empty : reader.GetString(offset);
            var count = Convert.ToInt64(reader.GetValue(offset + 1), CultureInfo.InvariantCulture);
            var sample = reader.IsDBNull(offset + 2) ? null : reader.GetString(offset + 2);
            accumulator.Add(kind, tenant, count, sample);
        }
    }

    private static IntegrityFindingKind ParseKind(string kind) => kind switch
    {
        "orphan" => IntegrityFindingKind.Orphan,
        "tenant_mismatch" => IntegrityFindingKind.TenantMismatch,
        "deleted_target" => IntegrityFindingKind.DeletedTarget,
        _ => throw new InvalidOperationException($"Unexpected integrity kind '{kind}'.")
    };

    private DbCommand CreateCommand(DbConnection connection, DbTransaction transaction, string sql)
    {
        var command = connection.CreateCommand();
        command.Transaction = transaction;
        command.CommandText = sql;
        command.CommandTimeout = options.StatementTimeoutSeconds + 15;
        return command;
    }

    private static void AddParameter(DbCommand command, string name, object? value, DbType? dbType = null)
    {
        var parameter = command.CreateParameter();
        parameter.ParameterName = name;
        parameter.Value = value ?? DBNull.Value;
        if (dbType is not null)
            parameter.DbType = dbType.Value;
        command.Parameters.Add(parameter);
    }

    private sealed class Accumulator(int sampleSize)
    {
        private readonly Dictionary<(IntegrityFindingKind Kind, string TenantId), (long Count, List<string> Samples)> _groups = new();
        private readonly HashSet<(IntegrityFindingKind, string)> _countedInBatch = [];

        public void BeginBatch() => _countedInBatch.Clear();

        public void Add(IntegrityFindingKind kind, string tenantId, long count, string? sampleId)
        {
            var key = (kind, tenantId);
            if (!_groups.TryGetValue(key, out var group))
                group = (0, []);

            if (_countedInBatch.Add(key))
                group.Count += count;

            if (sampleId is not null && group.Samples.Count < sampleSize && !group.Samples.Contains(sampleId, StringComparer.Ordinal))
                group.Samples.Add(sampleId);

            _groups[key] = group;
        }

        public IReadOnlyList<IntegrityCheckOutcome> ToOutcomes(IntegrityScan scan, IntegrityCheckStatus? failedStatus, string? reason, double durationSeconds)
        {
            var outcomes = new List<IntegrityCheckOutcome>(scan.Checks.Count);
            foreach (var check in scan.Checks)
            {
                var findings = _groups
                    .Where(g => g.Key.Kind == check.Kind && g.Value.Count > 0)
                    .OrderByDescending(g => g.Value.Count)
                    .ThenBy(g => g.Key.TenantId, StringComparer.Ordinal)
                    .Select(g => new IntegrityFinding(check.Name, g.Key.TenantId, g.Value.Count, g.Value.Samples.ToArray()))
                    .ToList();
                var count = findings.Sum(f => f.Count);
                var status = failedStatus ?? (count > 0 ? IntegrityCheckStatus.Findings : IntegrityCheckStatus.Clean);
                outcomes.Add(new IntegrityCheckOutcome(check.Name, check.Kind, status, count, findings, durationSeconds, reason));
            }

            return outcomes;
        }
    }
}
