namespace QFace.Sdk.AI.Models;

/// <summary>
/// Represents a historical data point for forecasting
/// </summary>
public class HistoricalDataPoint
{
    /// <summary>
    /// Date of the data point
    /// </summary>
    public DateTime Date { get; set; }
    
    /// <summary>
    /// Value at this date
    /// </summary>
    public decimal Value { get; set; }
    
    /// <summary>
    /// Optional metadata associated with this data point
    /// </summary>
    public Dictionary<string, object>? Metadata { get; set; }
}

