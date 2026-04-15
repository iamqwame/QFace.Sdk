namespace QimErp.Shared.Common;

/// <summary>
/// Shared names for tracing/metrics registration (avoid open-generic static member access).
/// </summary>
public static class ObservabilityTelemetry
{
    public const string MediatRActivitySourceName = "QimErp.MediatR";
}
