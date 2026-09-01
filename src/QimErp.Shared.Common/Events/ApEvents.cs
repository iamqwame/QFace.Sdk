namespace QimErp.Shared.Common.Events;

/// <summary>
/// Published when AP posts a vendor bill to GL. Project modules consume via
/// <see cref="Activities.Operations.IProjectExpenditureSyncWorkflow"/> for project-cost capture.
/// </summary>
public class ApBillPostedEvent : DomainEvent
{
    public Guid BillId { get; set; }
    public string BillCode { get; set; } = string.Empty;
    public string VendorId { get; set; } = string.Empty;
    public string VendorName { get; set; } = string.Empty;
    public DateTime BillDate { get; set; }
    public string CurrencyCode { get; set; } = string.Empty;
    public decimal GrandTotal { get; set; }
    public List<ApBillPostedLineData> Lines { get; set; } = [];

    public ApBillPostedEvent()
    {
    }

    public ApBillPostedEvent(
        Guid billId,
        string billCode,
        string vendorId,
        string vendorName,
        DateTime billDate,
        string currencyCode,
        decimal grandTotal,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        BillId = billId;
        BillCode = billCode;
        VendorId = vendorId;
        VendorName = vendorName;
        BillDate = billDate;
        CurrencyCode = currencyCode;
        GrandTotal = grandTotal;
    }

    public static ApBillPostedEvent Create(
        Guid billId,
        string billCode,
        string vendorId,
        string vendorName,
        DateTime billDate,
        string currencyCode,
        decimal grandTotal,
        string tenantId,
        IEnumerable<ApBillPostedLineData> lines,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new ApBillPostedEvent(
            billId, billCode, vendorId, vendorName, billDate, currencyCode, grandTotal, tenantId,
            userEmail, triggeredBy, userName)
        {
            Lines = lines.ToList()
        };
    }
}

public class ApBillPostedLineData
{
    public Guid LineId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }
    public Guid? ProjectId { get; set; }
    public string? ProjectNumber { get; set; }
    public Guid? TaskId { get; set; }
    public string? TaskNumber { get; set; }
}

/// <summary>
/// Published when AP creates, updates, or deletes a vendor. Inventory consumes via
/// <see cref="Activities.Inventory.IVendorSyncWorkflow"/>.
/// </summary>
public class VendorChangedEvent : DomainEvent
{
    public Guid VendorId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Type { get; set; }
    public string? TypeId { get; set; }
    public string? Group { get; set; }
    public string? GroupId { get; set; }
    public List<string> Tags { get; set; } = [];
    public string Status { get; set; } = "Active";
    public bool PaymentBlock { get; set; }
    public bool PostingBlock { get; set; }
    public string? TaxDetailsTin { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    /// <summary>Tenant installed modules — set by VendorSyncWorkflow before fan-out.</summary>
    public List<string>? SyncSelectedModules { get; set; }

    public VendorChangedEvent()
    {
    }

    public VendorChangedEvent(
        Guid vendorId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        VendorId = vendorId;
        Code = code;
        Name = name;
    }

    public static VendorChangedEvent Create(
        Guid vendorId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new VendorChangedEvent(vendorId, code, name, tenantId, userEmail, triggeredBy, userName);
    }
}
