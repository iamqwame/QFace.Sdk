namespace QFace.Sdk.AI.Models;

/// <summary>
/// Request for forecasting calculations
/// </summary>
public class ForecastRequest
{
    /// <summary>
    /// Historical data points to use for forecasting
    /// </summary>
    public List<HistoricalDataPoint> HistoricalData { get; set; } = new();
    
    /// <summary>
    /// Target date for the forecast
    /// </summary>
    public DateTime TargetDate { get; set; }
    
    /// <summary>
    /// Forecasting method to use
    /// </summary>
    public ForecastMethod Method { get; set; } = ForecastMethod.Regression;
    
    /// <summary>
    /// Optional assumptions for manual forecasting
    /// </summary>
    public Dictionary<string, object>? Assumptions { get; set; }
    
    /// <summary>
    /// Confidence level (0-100)
    /// </summary>
    public int? ConfidenceLevel { get; set; }
}

/// <summary>
/// Result of a forecasting calculation
/// </summary>
public class ForecastResult
{
    /// <summary>
    /// Forecasted value
    /// </summary>
    public decimal ForecastedValue { get; set; }
    
    /// <summary>
    /// Target date for the forecast
    /// </summary>
    public DateTime TargetDate { get; set; }
    
    /// <summary>
    /// Method used for forecasting
    /// </summary>
    public ForecastMethod Method { get; set; } = ForecastMethod.Regression;
    
    /// <summary>
    /// Confidence level (0-100)
    /// </summary>
    public int ConfidenceLevel { get; set; }
    
    /// <summary>
    /// Lower bound of confidence interval
    /// </summary>
    public decimal? LowerBound { get; set; }
    
    /// <summary>
    /// Upper bound of confidence interval
    /// </summary>
    public decimal? UpperBound { get; set; }
    
    /// <summary>
    /// Optional metadata about the forecast
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
    
    /// <summary>
    /// Optional warnings about the forecast
    /// </summary>
    public List<string>? Warnings { get; set; }
}

