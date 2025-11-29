namespace QFace.Sdk.AI.Algorithms;

/// <summary>
/// Manual forecast algorithm that uses assumptions from the request
/// </summary>
public class ManualForecastAlgorithm : IForecastAlgorithm
{
    private readonly ILogger<ManualForecastAlgorithm> _logger;

    /// <summary>
    /// Algorithm name
    /// </summary>
    public string AlgorithmName => "Manual";

    /// <summary>
    /// Initializes a new instance of ManualForecastAlgorithm
    /// </summary>
    public ManualForecastAlgorithm(ILogger<ManualForecastAlgorithm> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ForecastResult> CalculateForecastAsync(ForecastRequest request, ForecastMethod method, CancellationToken cancellationToken = default)
    {
        if (request.Assumptions == null || !request.Assumptions.ContainsKey("value"))
        {
            throw new ArgumentException("Manual forecast requires 'value' in assumptions");
        }

        var forecastedValue = Convert.ToDecimal(request.Assumptions["value"]);
        var confidenceLevel = request.ConfidenceLevel ?? 100; // Manual forecasts typically have high confidence

        var result = new ForecastResult
        {
            ForecastedValue = forecastedValue,
            TargetDate = request.TargetDate,
            Method = method,
            ConfidenceLevel = confidenceLevel,
            Metadata = new Dictionary<string, object>(request.Assumptions),
            Warnings = new List<string>
            {
                "This is a manual forecast based on assumptions. No statistical analysis was performed."
            }
        };

        // If bounds are provided in assumptions, use them
        if (request.Assumptions.ContainsKey("lowerBound"))
        {
            result.LowerBound = Convert.ToDecimal(request.Assumptions["lowerBound"]);
        }

        if (request.Assumptions.ContainsKey("upperBound"))
        {
            result.UpperBound = Convert.ToDecimal(request.Assumptions["upperBound"]);
        }

        _logger.LogInformation("Manual forecast calculated: {Value} for {Date}",
            forecastedValue, request.TargetDate);

        return Task.FromResult(result);
    }
}

