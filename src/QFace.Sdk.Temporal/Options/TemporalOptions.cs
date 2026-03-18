using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Hosting;

namespace QFace.Sdk.Temporal.Options;

/// <summary>
/// Unified configuration for Temporal client and worker.
/// Bind from the "Temporal" section in appsettings.json.
///
/// Replaces:
///   Orchestration: Orchestration:TemporalAddress, TemporalNamespace, EnableTemporalTls, TaskQueue
///   Workflow.Consumer: Temporal:Address, Temporal:Namespace
///
/// Example appsettings.json:
/// <code>
/// "Temporal": {
///   "Address":   "localhost:7233",
///   "Namespace": "qimerp",
///   "EnableTls": false,
///   "TaskQueue": "qimerp-workflow-approvals"
/// }
/// </code>
/// </summary>
public sealed class TemporalOptions
{
    public const string SectionName = "Temporal";

    // ── Connection ────────────────────────────────────────────────────────────

    /// <summary>
    /// Temporal frontend address in host:port format.
    /// For Temporal Cloud use the gRPC endpoint (e.g. "acct.tmprl.cloud:7233").
    /// </summary>
    [Required(ErrorMessage = "Temporal:Address is required when Temporal is enabled.")]
    public string Address { get; set; } = "localhost:7233";

    /// <summary>Temporal namespace. Defaults to "default".</summary>
    [Required]
    public string Namespace { get; set; } = "default";

    /// <summary>
    /// Enable TLS for the Temporal connection.
    /// Automatically set to true when Address is non-localhost and EnableTls is not
    /// explicitly configured — see TemporalClientServiceCollectionExtensions.
    /// </summary>
    public bool EnableTls { get; set; } = false;

    /// <summary>
    /// Advanced TLS settings (custom CA, SNI domain, mTLS certs).
    /// Null means use default TLS (system CA, SNI from Address host).
    /// Only relevant when EnableTls is true.
    /// </summary>
    public TemporalTlsOptions? Tls { get; set; }

    // ── Worker ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Default task queue for this app's worker.
    /// Can be overridden per worker in AddTemporalWorker(taskQueue:) calls.
    /// </summary>
    public string TaskQueue { get; set; } = string.Empty;

    /// <summary>
    /// Enable Temporal's worker versioning feature.
    /// When false, WorkerDeploymentOptions are still created (required by Temporalio 1.11+)
    /// but useWorkerVersioning is set to false so all tasks are delivered to this worker.
    /// </summary>
    public bool UseWorkerVersioning { get; set; } = false;

    /// <summary>
    /// Deployment version / build ID used for WorkerDeploymentOptions.
    /// Defaults to "unversioned" when null and UseWorkerVersioning is false.
    /// When UseWorkerVersioning is true, set this to a meaningful version string
    /// (e.g. Git commit SHA, semantic version).
    /// </summary>
    public string? BuildId { get; set; }

    /// <summary>
    /// Maximum concurrent activity task executions per worker.
    /// Null uses the Temporalio default (typically 100).
    /// Tune this for CPU-heavy or I/O-heavy activity workloads.
    /// </summary>
    public int? MaxConcurrentActivityExecutions { get; set; }

    /// <summary>
    /// Maximum concurrent workflow task executions per worker.
    /// Null uses the Temporalio default.
    /// Tune this for workflows with complex history or many simultaneous instances.
    /// </summary>
    public int? MaxConcurrentWorkflowTaskExecutions { get; set; }

    // ── Host resilience ───────────────────────────────────────────────────────

    /// <summary>
    /// Controls host behaviour when the Temporal worker fails to connect at startup.
    ///
    /// StopHost (default for Worker processes):
    ///   Host shuts down. Appropriate for dedicated worker processes where
    ///   Temporal connectivity is mandatory for the process to serve any purpose.
    ///
    /// ContinueWithUnhealthy (default for WebApi+Worker processes like Orchestration):
    ///   Host stays up. Worker is down but the API remains reachable.
    ///   Health check reports Unhealthy. Load balancer stops routing when
    ///   readiness probe fails. Process recovers when Temporal comes back.
    ///
    /// Maps to HostOptions.BackgroundServiceExceptionBehavior.
    /// </summary>
    public WorkerFailureBehavior WorkerFailureBehavior { get; set; } =
        WorkerFailureBehavior.StopHost;
}

/// <summary>
/// Controls what happens to the host when the Temporal worker background service fails.
/// </summary>
public enum WorkerFailureBehavior
{
    /// <summary>Host shuts down. Use for dedicated worker processes.</summary>
    StopHost,

    /// <summary>
    /// Host stays up, worker is unhealthy.
    /// Use for WebApi processes that also host a Temporal worker.
    /// Requires a health check to surface the unhealthy state.
    /// </summary>
    ContinueWithUnhealthy
}
