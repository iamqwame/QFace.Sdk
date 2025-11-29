using QFace.Sdk.AI.Models;

namespace QFace.Sdk.AI.Services;

/// <summary>
/// Interface for forecasting service
/// </summary>
public interface IForecastingService
{
    /// <summary>
    /// Calculates a forecast based on historical data
    /// </summary>
    /// <param name="request">Forecast request</param>
    /// <param name="method">Forecasting method to use (defaults to request.Method or AIOptions.DefaultForecastMethod)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Forecast result</returns>
    Task<ForecastResult> CalculateForecastAsync(ForecastRequest request, ForecastMethod? method = null, CancellationToken cancellationToken = default);
    
    /// <summary>
    /// Calculates multiple forecasts
    /// </summary>
    /// <param name="requests">List of forecast requests</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of forecast results</returns>
    Task<List<ForecastResult>> CalculateMultipleForecastsAsync(List<ForecastRequest> requests, CancellationToken cancellationToken = default);
}

