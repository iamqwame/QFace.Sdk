namespace QimErp.Shared.Common.Entities;

public class Import : GuidAuditableEntity
{
    public string ImportType { get; set; } = string.Empty;
    public ImportStatus Status { get; set; }
    public string? FileName { get; set; }
    public long? FileSize { get; set; }
    public string? ContentType { get; set; }
    public int TotalRows { get; set; }
    public int ProcessedRows { get; set; }
    public int SuccessfulImports { get; set; }
    public int FailedImports { get; set; }
    public double Percentage { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime LastUpdatedAt { get; set; }
    public string? ErrorMessage { get; set; }
    
    // Batch tracking fields
    public int BatchesQueued { get; set; }
    public int BatchesSaved { get; set; }
    public int BatchesFailed { get; set; }

    public Import()
    {
    }

    public static Import Create(
        string importType,
        string? fileName,
        long? fileSize,
        string? contentType,
        string tenantId,
        string userId,
        string userEmail,
        string? userName = null)
    {
        if (string.IsNullOrWhiteSpace(importType))
            throw new ArgumentException("Import type cannot be null or empty", nameof(importType));

        var import = new Import
        {
            Id = CreateId(),
            ImportType = importType,
            Status = ImportStatus.NotStarted,
            FileName = fileName,
            FileSize = fileSize,
            ContentType = contentType,
            TotalRows = 0,
            ProcessedRows = 0,
            SuccessfulImports = 0,
            FailedImports = 0,
            Percentage = 0,
            StartedAt = DateTime.UtcNow,
            LastUpdatedAt = DateTime.UtcNow,
            TenantId = tenantId
        }.OnCreate(userId, userEmail, userName);
       
        

        return (Import)import;
    }

    public void Start(int totalRows = 0)
    {
        Status = ImportStatus.InProgress;
        TotalRows = totalRows;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateTotalRows(int totalRows)
    {
        TotalRows = totalRows;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateProgress(int processedRows, int successfulImports, int failedImports)
    {
        ProcessedRows = processedRows;
        SuccessfulImports = successfulImports;
        FailedImports = failedImports;
        Percentage = TotalRows > 0 ? (double)processedRows / TotalRows * 100 : 0;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = ImportStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Fail(string errorMessage)
    {
        Status = ImportStatus.Failed;
        ErrorMessage = errorMessage;
        CompletedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        Status = ImportStatus.Cancelled;
        CompletedAt = DateTime.UtcNow;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void StartBatchSaving(int totalBatches)
    {
        Status = ImportStatus.SavingBatches;
        BatchesQueued = totalBatches;
        BatchesSaved = 0;
        BatchesFailed = 0;
        LastUpdatedAt = DateTime.UtcNow;
    }

    public void UpdateBatchSaveProgress(int batchesSaved, int batchesFailed)
    {
        BatchesSaved = batchesSaved;
        BatchesFailed = batchesFailed;
        LastUpdatedAt = DateTime.UtcNow;
    }
}

public enum ImportStatus
{
    NotStarted = 0,
    InProgress = 1,
    Completed = 2,
    Failed = 3,
    Cancelled = 4,
    SavingBatches = 5
}

