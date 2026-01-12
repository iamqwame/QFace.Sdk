namespace QimErp.Shared.Common.Contracts;

public class BulkOperationResponse
{
    public int SuccessCount { get; set; }
    public int FailureCount { get; set; }
    public List<string> Errors { get; set; } = [];
    public List<string> Warnings { get; set; } = [];
    public string? Message { get; set; }
}