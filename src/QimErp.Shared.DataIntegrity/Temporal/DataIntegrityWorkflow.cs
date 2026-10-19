using Temporalio.Common;
using Temporalio.Workflows;

namespace QimErp.Shared.DataIntegrity.Temporal;

[Workflow("DataIntegrityWorkflow")]
public sealed class DataIntegrityWorkflow
{
    [WorkflowRun]
    public Task<DataIntegrityRunSummary> RunAsync(RunIntegrityChecksInput input)
        => Workflow.ExecuteActivityAsync(
            (DataIntegrityActivities a) => a.RunIntegrityChecksAsync(input),
            new ActivityOptions
            {
                StartToCloseTimeout = TimeSpan.FromMinutes(input.MaxRunMinutes + 15),
                HeartbeatTimeout = TimeSpan.FromMinutes(2),
                RetryPolicy = new RetryPolicy
                {
                    InitialInterval = TimeSpan.FromMinutes(5),
                    BackoffCoefficient = 2.0f,
                    MaximumInterval = TimeSpan.FromMinutes(30),
                    MaximumAttempts = 3,
                    NonRetryableErrorTypes = [nameof(DataIntegrityConfigurationException)]
                }
            });
}
