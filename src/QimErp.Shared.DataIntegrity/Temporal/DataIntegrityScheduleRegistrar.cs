using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using QFace.Sdk.Temporal.Options;
using QimErp.Shared.DataIntegrity.Discovery;
using Temporalio.Api.Enums.V1;
using Temporalio.Client;
using Temporalio.Client.Schedules;
using Temporalio.Exceptions;

namespace QimErp.Shared.DataIntegrity.Temporal;

internal sealed class DataIntegrityScheduleRegistrar(
    IServiceProvider services,
    IConfiguration configuration,
    IOptions<DataIntegrityOptions> options,
    DataIntegrityContextSet contextSet,
    IntegrityLinkCatalog catalog,
    ILogger<DataIntegrityScheduleRegistrar> logger) : BackgroundService
{
    private static readonly TimeSpan[] RetryDelays =
    [
        TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(2), TimeSpan.FromMinutes(4), TimeSpan.FromMinutes(8), TimeSpan.FromMinutes(16)
    ];

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await Task.Yield();
        var settings = options.Value;
        await Task.Delay(TimeSpan.FromSeconds(settings.ScheduleStartupDelaySeconds), stoppingToken);
        LogDiscoverySummary();

        var scheduleId = IntegrityNaming.ScheduleId(contextSet.ModuleKey, TaskQueueSuffix());
        for (var attempt = 0; ; attempt++)
        {
            try
            {
                var client = services.GetService<ITemporalClient>();
                if (client is null)
                {
                    logger.LogInformation(
                        "data_integrity_schedule_skipped module={Module} scheduleId={ScheduleId} reason={Reason}",
                        contextSet.ModuleKey, scheduleId, "no_temporal_client");
                    return;
                }

                await ReconcileAsync(client, scheduleId, settings, stoppingToken);
                break;
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                if (attempt >= RetryDelays.Length)
                {
                    logger.LogError(ex,
                        "data_integrity_schedule_failed module={Module} scheduleId={ScheduleId} attempts={Attempts}",
                        contextSet.ModuleKey, scheduleId, attempt + 1);
                    return;
                }

                logger.LogWarning(ex,
                    "data_integrity_schedule_retry module={Module} scheduleId={ScheduleId} attempt={Attempt} retryInMinutes={RetryInMinutes}",
                    contextSet.ModuleKey, scheduleId, attempt + 1, RetryDelays[attempt].TotalMinutes);
                await Task.Delay(RetryDelays[attempt], stoppingToken);
            }
        }
    }

    private string TaskQueueSuffix() => configuration[$"{TemporalOptions.SectionName}:TaskQueueSuffix"] ?? string.Empty;

    private async Task ReconcileAsync(ITemporalClient client, string scheduleId, DataIntegrityOptions settings, CancellationToken cancellationToken)
    {
        var suffix = TaskQueueSuffix();
        var temporal = new TemporalOptions { TaskQueueSuffix = suffix };
        var taskQueue = temporal.WithTaskQueueSuffix(IntegrityNaming.TaskQueue(contextSet.ModuleKey));
        var fingerprint = string.Join('|',
            "v1", settings.Cron, settings.JitterMinutes, taskQueue, IntegrityNaming.WorkflowType, contextSet.ModuleKey, settings.MaxRunMinutes);
        var input = new RunIntegrityChecksInput(contextSet.ModuleKey, settings.MaxRunMinutes);

        var action = ScheduleActionStartWorkflow.Create(
            (DataIntegrityWorkflow wf) => wf.RunAsync(input),
            new WorkflowOptions(IntegrityNaming.WorkflowId(contextSet.ModuleKey, suffix), taskQueue)
            {
                ExecutionTimeout = TimeSpan.FromMinutes(settings.MaxRunMinutes * 3 + 60)
            });

        var schedule = new Schedule(action, new ScheduleSpec
        {
            CronExpressions = [settings.Cron],
            TimeZoneName = "UTC",
            Jitter = TimeSpan.FromMinutes(settings.JitterMinutes)
        })
        {
            Policy = new SchedulePolicy { Overlap = ScheduleOverlapPolicy.Skip, CatchupWindow = TimeSpan.FromHours(2) },
            State = new ScheduleState { Paused = !settings.Enabled, Note = fingerprint }
        };

        var rpc = new RpcOptions { CancellationToken = cancellationToken };
        try
        {
            await client.CreateScheduleAsync(scheduleId, schedule, new ScheduleOptions { TriggerImmediately = false, Rpc = rpc });
            logger.LogInformation(
                "data_integrity_schedule_created module={Module} scheduleId={ScheduleId} cron={Cron} taskQueue={TaskQueue} paused={Paused}",
                contextSet.ModuleKey, scheduleId, settings.Cron, taskQueue, !settings.Enabled);
            return;
        }
        catch (ScheduleAlreadyRunningException)
        {
        }

        var handle = client.GetScheduleHandle(scheduleId);
        var description = await handle.DescribeAsync(rpc);
        var state = description.Schedule.State;
        if (state.Note == fingerprint && state.Paused == !settings.Enabled)
        {
            logger.LogInformation(
                "data_integrity_schedule_current module={Module} scheduleId={ScheduleId}", contextSet.ModuleKey, scheduleId);
            return;
        }

        await handle.UpdateAsync(_ => new ScheduleUpdate(schedule), rpc);
        logger.LogInformation(
            "data_integrity_schedule_updated module={Module} scheduleId={ScheduleId} cron={Cron} taskQueue={TaskQueue} paused={Paused}",
            contextSet.ModuleKey, scheduleId, settings.Cron, taskQueue, !settings.Enabled);
    }

    private void LogDiscoverySummary()
    {
        foreach (var registration in contextSet.Contexts)
        {
            var contextKey = IntegrityNaming.ContextKey(registration.ContextType);
            try
            {
                using var scope = services.CreateScope();
                if (scope.ServiceProvider.GetService(registration.ContextType) is not DbContext context)
                    throw new DataIntegrityConfigurationException([$"{registration.ContextType.Name} is not registered in the service provider."]);

                if (!context.Database.IsRelational())
                {
                    logger.LogWarning(
                        "data_integrity_discovery_skipped module={Module} context={Context} reason={Reason}",
                        contextSet.ModuleKey, contextKey, "unsupported_provider");
                    continue;
                }

                var result = catalog.Get(registration, context);
                IntegrityLinkCatalog.WarnUnusedOverrides(logger, contextSet.ModuleKey, result);
                logger.LogInformation(
                    "data_integrity_discovered module={Module} context={Context} links={Links} unindexed={Unindexed} unresolved={Unresolved} ambiguous={Ambiguous} excluded={Excluded}",
                    contextSet.ModuleKey, contextKey, result.CheckedLinks, result.Unindexed, result.Unresolved, result.Ambiguous, result.Excluded);
            }
            catch (DataIntegrityConfigurationException ex)
            {
                logger.LogError(ex, "data_integrity_configuration_invalid module={Module} context={Context}", contextSet.ModuleKey, contextKey);
            }
            catch (Exception ex)
            {
                logger.LogWarning(ex, "data_integrity_discovery_failed module={Module} context={Context}", contextSet.ModuleKey, contextKey);
            }
        }
    }
}
