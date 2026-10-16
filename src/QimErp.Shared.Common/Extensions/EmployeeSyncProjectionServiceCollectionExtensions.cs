using QimErp.Shared.Common.Services.Employees;

namespace QimErp.Shared.Common.Extensions;

public static class EmployeeSyncProjectionServiceCollectionExtensions
{
    /// <summary>
    /// Registers the People sync-projection HTTP client (DATA-11 owner fetch).
    /// Does not touch outbox / DbContext registration.
    /// </summary>
    public static IServiceCollection AddEmployeeSyncProjectionClient(this IServiceCollection services)
    {
        services.AddHttpClient(EmployeeSyncProjectionClient.HttpClientName);
        services.AddScoped<IEmployeeSyncProjectionClient, EmployeeSyncProjectionClient>();
        return services;
    }
}