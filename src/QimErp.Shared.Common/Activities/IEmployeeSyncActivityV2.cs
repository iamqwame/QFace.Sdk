using Temporalio.Activities;

namespace QimErp.Shared.Common.Activities;

/// <summary>
/// Ids-only employee sync activities (DATA-11). Implementers fetch
/// <see cref="Contracts.EmployeeSyncProjection"/> from People at activity time.
/// Activity names: <c>SyncEmployeeCreatedV2:{Suffix}</c>, etc.
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
