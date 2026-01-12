namespace QimErp.Shared.Common.Entities.Helpers;

public sealed class TransactionPostNote
{
    public DateTime? ApprovedDate { get; set; }
    public string? ApprovedNote { get; set; }
    public string? RecommendedBy { get; set; }
    public DateTime? RecommendedDate { get; set; }
    public string? RecommendedNote { get; set; }
    public bool IsPosted { get; set; }
    public string? By { get; set; }
    public DateTime? PostedDate { get; set; }
    public string? Memo { get; set; } = string.Empty;
    public string? ApprovedBy { get; private set; }

    public static TransactionPostNote Create()
    {
        return new TransactionPostNote();
    }

    public TransactionPostNote MarkAsPosted(string? postedBy,
        DateTime postedDate,
        string? postedMemo = null)
    {
        IsPosted = true;
        By = postedBy;
        PostedDate = postedDate;
        Memo = postedMemo;
        return this;
    }

    public TransactionPostNote ClearPosting()
    {
        IsPosted = false;
        By = null;
        PostedDate = null;
        Memo = null;
        return this;
    }

    public TransactionPostNote WithPostedMemo(string? postedMemo)
    {
        Memo = postedMemo;
        return this;
    }

    public TransactionPostNote Approve(string approvedBy, DateTime approvedDate, string? note = null)
    {
        ApprovedBy = approvedBy;
        ApprovedDate = approvedDate;
        ApprovedNote = note;
        return this;
    }

    public TransactionPostNote Recommend(string recommendedBy, DateTime recommendedDate, string? note = null)
    {
        RecommendedBy = recommendedBy;
        RecommendedDate = recommendedDate;
        RecommendedNote = note;
        return this;
    }


}