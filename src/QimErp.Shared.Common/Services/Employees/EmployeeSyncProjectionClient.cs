using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using QimErp.Shared.Common.Contracts;
using QimErp.Shared.Common.Options;
using QimErp.Shared.Common.Services.Auth;

namespace QimErp.Shared.Common.Services.Employees;

public sealed class EmployeeSyncProjectionClient(
    IHttpClientFactory httpClientFactory,
    IConfiguration configuration,
    IOptions<InternalApiOptions> internalApiOptions,
    ICurrentUserService currentUserService,
    ILogger<EmployeeSyncProjectionClient> logger) : IEmployeeSyncProjectionClient
{
    public const string HttpClientName = nameof(EmployeeSyncProjectionClient);

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<EmployeeSyncProjection?> GetAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var baseUrl = configuration["People:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            logger.LogError("People:BaseUrl is not configured — cannot load employee sync projection {EmployeeId}", employeeId);
            return null;
        }

        var apiKey = internalApiOptions.Value.ExpectedApiKey;
        if (string.IsNullOrEmpty(apiKey))
        {
            logger.LogError("InternalApi:ExpectedApiKey is not configured — cannot call People sync projection");
            return null;
        }

        var tenantId = currentUserService.GetTenantId();
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            logger.LogError("TenantId is not set — cannot call People sync projection for {EmployeeId}", employeeId);
            return null;
        }

        var client = httpClientFactory.CreateClient(HttpClientName);
        client.BaseAddress = new Uri(baseUrl.TrimEnd('/') + "/");
        client.DefaultRequestHeaders.Remove("X-Internal-Api-Key");
        client.DefaultRequestHeaders.Add("X-Internal-Api-Key", apiKey);

        var path =
            $"internal/employees/{employeeId:D}/sync-projection?tenantId={Uri.EscapeDataString(tenantId.Trim())}";
        var response = await client.GetAsync(path, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<EmployeeSyncProjection>(JsonOptions, cancellationToken);
    }
}
