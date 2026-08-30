namespace QimErp.Shared.Common.Events;

/// <summary>
/// Published when AR approves/ships an invoice. Inventory consumes via <see cref="Activities.Inventory.IStockIssueSyncWorkflow"/>.
/// </summary>
public class InvoiceShippedEvent : DomainEvent
{
    public Guid InvoiceId { get; set; }
    public string InvoiceCode { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime InvoiceDate { get; set; }
    public long? WarehouseId { get; set; }
    public List<InvoiceShippedLineData> Lines { get; set; } = [];

    public InvoiceShippedEvent()
    {
    }

    public InvoiceShippedEvent(
        Guid invoiceId,
        string invoiceCode,
        string customerId,
        string customerName,
        DateTime invoiceDate,
        string tenantId,
        long? warehouseId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        InvoiceId = invoiceId;
        InvoiceCode = invoiceCode;
        CustomerId = customerId;
        CustomerName = customerName;
        InvoiceDate = invoiceDate;
        WarehouseId = warehouseId;
    }

    public static InvoiceShippedEvent Create(
        Guid invoiceId,
        string invoiceCode,
        string customerId,
        string customerName,
        DateTime invoiceDate,
        string tenantId,
        IEnumerable<InvoiceShippedLineData> lines,
        long? warehouseId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new InvoiceShippedEvent(
            invoiceId, invoiceCode, customerId, customerName, invoiceDate, tenantId,
            warehouseId, userEmail, triggeredBy, userName)
        {
            Lines = lines.ToList()
        };
    }
}

public class InvoiceShippedLineData
{
    public Guid LineId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    // A fractional quantity breaks any consumer still deserializing this as int.
    public decimal Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal UnitCost { get; set; }
}

/// <summary>
/// Published when AR creates/confirms a sale order. Inventory consumes via
/// <see cref="Activities.Inventory.IStockReservationSyncWorkflow"/>.
/// </summary>
public class SaleOrderReservedEvent : DomainEvent
{
    public Guid SaleOrderId { get; set; }
    public string SaleOrderCode { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public long? WarehouseId { get; set; }
    public List<SaleOrderReservedLineData> Lines { get; set; } = [];

    public SaleOrderReservedEvent()
    {
    }

    public SaleOrderReservedEvent(
        Guid saleOrderId,
        string saleOrderCode,
        string customerId,
        string customerName,
        DateTime orderDate,
        string tenantId,
        long? warehouseId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        SaleOrderId = saleOrderId;
        SaleOrderCode = saleOrderCode;
        CustomerId = customerId;
        CustomerName = customerName;
        OrderDate = orderDate;
        WarehouseId = warehouseId;
    }

    public static SaleOrderReservedEvent Create(
        Guid saleOrderId,
        string saleOrderCode,
        string customerId,
        string customerName,
        DateTime orderDate,
        string tenantId,
        IEnumerable<SaleOrderReservedLineData> lines,
        long? warehouseId = null,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new SaleOrderReservedEvent(
            saleOrderId, saleOrderCode, customerId, customerName, orderDate, tenantId,
            warehouseId, userEmail, triggeredBy, userName)
        {
            Lines = lines.ToList()
        };
    }
}

public class SaleOrderReservedLineData
{
    public Guid LineId { get; set; }
    public string ProductId { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
}

/// <summary>
/// Published when AR creates, updates, or deletes a customer. Inventory consumes via
/// <see cref="Activities.Inventory.ICustomerSyncWorkflow"/>.
/// </summary>
public class CustomerChangedEvent : DomainEvent
{
    public Guid CustomerId { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Company { get; set; }
    public string? ImageUrl { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string? Type { get; set; }
    public string? TypeId { get; set; }
    public string? Group { get; set; }
    public string? GroupId { get; set; }
    public List<string> Tags { get; set; } = [];
    public string Status { get; set; } = "Active";
    public bool TaxExempt { get; set; }
    public string? PreferredCommunication { get; set; }
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; }

    /// <summary>Tenant installed modules — set by CustomerSyncWorkflow before fan-out.</summary>
    public List<string>? SyncSelectedModules { get; set; }

    public CustomerChangedEvent()
    {
    }

    public CustomerChangedEvent(
        Guid customerId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
        : base(tenantId, userEmail, triggeredBy, userName)
    {
        CustomerId = customerId;
        Code = code;
        Name = name;
    }

    public static CustomerChangedEvent Create(
        Guid customerId,
        string code,
        string name,
        string tenantId,
        string? userEmail = null,
        string? triggeredBy = null,
        string? userName = null)
    {
        return new CustomerChangedEvent(customerId, code, name, tenantId, userEmail, triggeredBy, userName);
    }
}
