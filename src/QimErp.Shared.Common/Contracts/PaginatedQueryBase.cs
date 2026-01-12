namespace QimErp.Shared.Common.Contracts;

/// <summary>
/// Base class for paginated query/command requests
/// </summary>
public abstract class PaginatedQueryBase
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; } = false;
}

