using System.ComponentModel.DataAnnotations;

namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// Configures <see cref="OutboxProcessorBackgroundService{TContext}"/>. Binds from the
/// "OutboxProcessor" configuration section; falls back to these defaults when the section
/// is absent.
/// </summary>
public class OutboxProcessorOptions
{
    public const string SectionName = "OutboxProcessor";

    [Range(1, 3600, ErrorMessage = "PollingIntervalSeconds must be between 1 and 3600")]
    public int PollingIntervalSeconds { get; set; } = 30;

    [Range(1, 1000, ErrorMessage = "BatchSize must be between 1 and 1000")]
    public int BatchSize { get; set; } = 50;
}
