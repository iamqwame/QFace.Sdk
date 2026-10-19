using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Ids-only employee sync activities (DATA-11). Implementers fetch
/// <see cref="Contracts.EmployeeSyncProjection"/> from People at activity time.
/// Activity names: <c>SyncEmployeeCreatedV2:{Suffix}</c>,
/// <c>SyncEmployeeUpdatedV2:{Suffix}</c>, <c>SyncEmployeeTerminatedV2:{Suffix}</c>.
/// Bulk import uses <c>SyncEmployeesCreatedBulkV2:{Suffix}</c> with
/// <see cref="List{T}"/> of <see cref="EmployeeChangedEventV2"/> (same naming pattern as
/// V1 <c>SyncEmployeesCreatedBulk:{Suffix}</c>; not a member here — register on the
/// concrete activity class).
/// </summary>
public interface IEmployeeSyncActivityV2
{
    [Activity]
    Task SyncEmployeeCreatedV2Async(EmployeeChangedEventV2 employee);

    [Activity]
    Task SyncEmployeeUpdatedV2Async(EmployeeChangedEventV2 employee);

    [Activity]
    Task SyncEmployeeTerminatedV2Async(EmployeeChangedEventV2 employee);
}
