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
}
