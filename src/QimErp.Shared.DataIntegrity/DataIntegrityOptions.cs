using System.ComponentModel.DataAnnotations;

namespace QimErp.Shared.DataIntegrity;

public sealed class DataIntegrityOptions
{
    public const string SectionName = "DataIntegrity";

    public bool Enabled { get; set; } = true;

    [Required]
    public string Cron { get; set; } = "0 2 * * *";

    [Range(0, 120)]
    public int JitterMinutes { get; set; } = 15;

    [Range(5, 3600)]
    public int StatementTimeoutSeconds { get; set; } = 120;

    [Range(1, 60)]
    public int LockTimeoutSeconds { get; set; } = 5;

    [Range(1000, int.MaxValue)]
    public int BatchThresholdRows { get; set; } = 250_000;

    [Range(1000, 1_000_000)]
    public int BatchSize { get; set; } = 100_000;

    [Range(1, 50)]
    public int SampleSize { get; set; } = 10;

    [Range(1, 1000)]
    public int MaxTenantsLoggedPerCheck { get; set; } = 50;

    [Range(10, 600)]
    public int MaxRunMinutes { get; set; } = 120;

    [Range(0, 600)]
    public int ScheduleStartupDelaySeconds { get; set; } = 30;

    public bool PrintLinks { get; set; }
}
