namespace QimErp.Shared.Common.Sync;

public enum SyncType
{
    Employee,
    AdminData,
    TenantConfig,
    /// <summary>GL Core → modules with local COA/cost-center/fiscal read models.</summary>
    GlReference,
    /// <summary>CoreHR assignment changes → Payroll (extensible).</summary>
    AssignmentChanged,
    /// <summary>GL journal posted → Budget Planning actuals sync.</summary>
    JournalEntryPosted,
    /// <summary>IAM master tenant reference data → module local copies (currency, FX, calendar, holidays).</summary>
    TenantReference,
    /// <summary>AR/POS invoice shipped → Inventory stock deduction.</summary>
    StockIssue,
    /// <summary>AR customer create/update/delete → Inventory local customer cache.</summary>
    Customer,
    /// <summary>AP vendor create/update/delete → Inventory local vendor cache.</summary>
    Vendor,
}
