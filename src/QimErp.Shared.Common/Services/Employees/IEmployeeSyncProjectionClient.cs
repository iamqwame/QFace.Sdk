using QimErp.Shared.Common.Contracts;

namespace QimErp.Shared.Common.Services.Employees;

/// <summary>
/// Fetches the People-owned employee projection for local mirror upserts (DATA-11).
/// Calls <c>GET /internal/employees/{id}/sync-projection</c> with <c>X-Internal-Api-Key</c>.
/// </summary>
public interface IEmployeeSyncProjectionClient
{
    Task<EmployeeSyncProjection?> GetAsync(Guid employeeId, CancellationToken cancellationToken = default);
}
