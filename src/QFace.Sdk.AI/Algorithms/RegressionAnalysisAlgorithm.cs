using MathNet.Numerics.LinearRegression;

namespace QFace.Sdk.AI.Algorithms;

/// <summary>
/// Regression analysis algorithm using MathNet.Numerics for linear regression
/// </summary>
public class RegressionAnalysisAlgorithm : IForecastAlgorithm
{
    private readonly ILogger<RegressionAnalysisAlgorithm> _logger;

    /// <summary>
    /// Algorithm name
    /// </summary>
    public string AlgorithmName => "Regression";

    /// <summary>
    /// Initializes a new instance of RegressionAnalysisAlgorithm
    /// </summary>
    public RegressionAnalysisAlgorithm(ILogger<RegressionAnalysisAlgorithm> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public Task<ForecastResult> CalculateForecastAsync(ForecastRequest request, ForecastMethod method, CancellationToken cancellationToken = default)
    {
        if (request.HistoricalData == null || request.HistoricalData.Count < 2)
        {
            throw new ArgumentException("At least 2 historical data points are required for regression analysis");
        }

        var sortedData = request.HistoricalData.OrderBy(d => d.Date).ToList();
        var dataCount = sortedData.Count;

        // Convert dates to numeric values (days since first date)
        var firstDate = sortedData[0].Date;
        var xValues = sortedData.Select(d => (double)(d.Date - firstDate).TotalDays).ToArray();
        var yValues = sortedData.Select(d => (double)d.Value).ToArray();

        // Perform linear regression
        var (intercept, slope) = SimpleRegression.Fit(xValues, yValues);

        // Calculate forecast
        var daysToTarget = (request.TargetDate - firstDate).TotalDays;
        var forecastedValue = (decimal)(intercept + slope * daysToTarget);

        // Calculate R-squared for confidence assessment
        var rSquared = CalculateRSquared(xValues, yValues, intercept, slope);
        var confidenceLevel = request.ConfidenceLevel ?? CalculateConfidenceFromRSquared(rSquared);

        // Calculate confidence intervals
        var residuals = yValues.Select((y, i) => y - (intercept + slope * xValues[i])).ToArray();
        var residualVariance = residuals.Sum(r => r * r) / (dataCount - 2);
        var standardError = Math.Sqrt(residualVariance);
        var zScore = (double)GetZScore(confidenceLevel);
        var margin = (decimal)(standardError * zScore);

        var result = new ForecastResult
        {
            ForecastedValue = forecastedValue,
            TargetDate = request.TargetDate,
            Method = method,
            ConfidenceLevel = confidenceLevel,
            LowerBound = forecastedValue - margin,
            UpperBound = forecastedValue + margin,
            Metadata = new Dictionary<string, object>
            {
                { "RSquared", rSquared },
                { "Slope", slope },
                { "Intercept", intercept }
            },
            Warnings = new List<string>()
        };

        if (rSquared < 0.5)
        {
            result.Warnings.Add("Low R-squared value. Data may not follow a linear trend.");
        }

        if (daysToTarget > 365)
        {
            result.Warnings.Add("Forecast extends more than 1 year into the future. Accuracy may decrease.");
        }

        if (dataCount < 5)
        {
            result.Warnings.Add("Limited historical data. Forecast accuracy may be reduced.");
        }

        _logger.LogInformation("Regression forecast calculated: {Value} for {Date} with {Confidence}% confidence (R²={RSquared})",
            forecastedValue, request.TargetDate, confidenceLevel, rSquared);

        return Task.FromResult(result);
    }

    private double CalculateRSquared(double[] xValues, double[] yValues, double intercept, double slope)
    {
        var yMean = yValues.Average();
        var totalSumSquares = yValues.Sum(y => Math.Pow(y - yMean, 2));
        var residualSumSquares = yValues.Select((y, i) => Math.Pow(y - (intercept + slope * xValues[i]), 2)).Sum();
        
        if (totalSumSquares == 0)
        {
            return 1.0;
        }

        return 1.0 - (residualSumSquares / totalSumSquares);
    }

    private int CalculateConfidenceFromRSquared(double rSquared)
    {
        // Map R-squared to confidence level
        return rSquared switch
        {
            >= 0.9 => 95,
            >= 0.7 => 90,
            >= 0.5 => 80,
            _ => 70
        };
    }

    private decimal GetZScore(int confidenceLevel)
    {
        return confidenceLevel switch
        {
            90 => 1.645m,
            95 => 1.96m,
            99 => 2.576m,
            _ => 1.28m // Default for 80%
        };
    }
}

