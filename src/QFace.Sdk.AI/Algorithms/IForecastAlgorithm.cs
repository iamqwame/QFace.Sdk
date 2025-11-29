namespace QFace.Sdk.AI.Algorithms;

/// <summary>
/// Interface for forecasting algorithms
/// </summary>
public interface IForecastAlgorithm
{
    /// <summary>
    /// Algorithm name (e.g., "Trend", "Regression", "ML", "Manual")
    /// </summary>
    string AlgorithmName { get; }
    
    /// <summary>
    /// Calculates a forecast based on historical data
    /// </summary>
    /// <param name="request">Forecast request with historical data</param>
    /// <param name="method">Forecasting method to use</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Forecast result</returns>
    Task<ForecastResult> CalculateForecastAsync(ForecastRequest request, ForecastMethod method, CancellationToken cancellationToken = default);
}

