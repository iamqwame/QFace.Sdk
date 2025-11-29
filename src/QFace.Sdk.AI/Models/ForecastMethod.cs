namespace QFace.Sdk.AI.Models;

/// <summary>
/// Enumeration of available forecasting methods
/// </summary>
public enum ForecastMethod
{
    /// <summary>
    /// Trend analysis - simple linear trend calculation
    /// </summary>
    Trend = 0,
    
    /// <summary>
    /// Regression analysis - linear regression using MathNet.Numerics
    /// </summary>
    Regression = 1,
    
    /// <summary>
    /// Machine Learning - ML.NET time series forecasting
    /// </summary>
    ML = 2,
    
    /// <summary>
    /// Manual forecast - uses assumptions from request
    /// </summary>
    Manual = 3
}

