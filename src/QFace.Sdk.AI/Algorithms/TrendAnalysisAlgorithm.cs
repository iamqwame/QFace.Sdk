namespace QFace.Sdk.AI.Algorithms;

/// <summary>
/// Simple trend analysis algorithm using linear trend calculation
/// </summary>
public class TrendAnalysisAlgorithm : IForecastAlgorithm
{
    private readonly ILogger<TrendAnalysisAlgorithm> _logger;

    /// <summary>
    /// Algorithm name
    /// </summary>
    public string AlgorithmName => "Trend";

    /// <summary>
    /// Initializes a new instance of TrendAnalysisAlgorithm
    /// </summary>
    public TrendAnalysisAlgorithm(ILogger<TrendAnalysisAlgorithm> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ForecastResult> CalculateForecastAsync(ForecastRequest request, ForecastMethod method, CancellationToken cancellationToken = default)
    {
        if (request.HistoricalData == null || request.HistoricalData.Count < 2)
        {
            throw new ArgumentException("At least 2 historical data points are required for trend analysis");
        }

        var sortedData = request.HistoricalData.OrderBy(d => d.Date).ToList();
        var dataCount = sortedData.Count;

        // Calculate average growth rate
        var totalGrowth = 0.0m;
        var growthCount = 0;

        for (int i = 1; i < dataCount; i++)
        {
            var daysDiff = (sortedData[i].Date - sortedData[i - 1].Date).TotalDays;
            if (daysDiff > 0)
            {
                var growthRate = (sortedData[i].Value - sortedData[i - 1].Value) / (decimal)daysDiff;
                totalGrowth += growthRate;
                growthCount++;
            }
        }

        if (growthCount == 0)
        {
            throw new InvalidOperationException("Cannot calculate trend: no valid time intervals found");
        }

        var averageGrowthRate = totalGrowth / growthCount;
        var lastDataPoint = sortedData[dataCount - 1];
        var daysToTarget = (request.TargetDate - lastDataPoint.Date).TotalDays;

        var forecastedValue = lastDataPoint.Value + (averageGrowthRate * (decimal)daysToTarget);

        // Calculate confidence bounds (simple approach)
        var confidenceLevel = request.ConfidenceLevel ?? 80;
        var variance = CalculateVariance(sortedData);
        var standardDeviation = (decimal)Math.Sqrt((double)variance);
        var zScore = GetZScore(confidenceLevel);
        var margin = standardDeviation * zScore;

        var result = new ForecastResult
        {
            ForecastedValue = forecastedValue,
            TargetDate = request.TargetDate,
            Method = method,
            ConfidenceLevel = confidenceLevel,
            LowerBound = forecastedValue - margin,
            UpperBound = forecastedValue + margin,
            Warnings = new List<string>()
        };

        if (daysToTarget > 365)
        {
            result.Warnings.Add("Forecast extends more than 1 year into the future. Accuracy may decrease.");
        }

        if (dataCount < 5)
        {
            result.Warnings.Add("Limited historical data. Forecast accuracy may be reduced.");
        }

        _logger.LogInformation("Trend forecast calculated: {Value} for {Date} with {Confidence}% confidence",
            forecastedValue, request.TargetDate, confidenceLevel);

        return Task.FromResult(result);
    }

    private decimal CalculateVariance(List<HistoricalDataPoint> data)
    {
        if (data.Count < 2)
        {
            return 0;
        }

        var mean = data.Average(d => d.Value);
        var sumSquaredDiff = data.Sum(d => (d.Value - mean) * (d.Value - mean));
        return sumSquaredDiff / (data.Count - 1);
    }

    private decimal GetZScore(int confidenceLevel)
    {
        // Approximate Z-scores for common confidence levels
        return confidenceLevel switch
        {
            90 => 1.645m,
            95 => 1.96m,
            99 => 2.576m,
            _ => 1.28m // Default for 80%
        };
    }
}

