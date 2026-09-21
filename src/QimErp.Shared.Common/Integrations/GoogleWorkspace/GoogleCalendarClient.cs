using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using QimErp.Shared.Common.Constants;
using QimErp.Shared.Common.Services.Cache;

namespace QimErp.Shared.Common.Integrations.GoogleWorkspace;

public sealed class GoogleCalendarClient(
    ICacheService cache,
    IHttpClientFactory httpClientFactory,
    ILogger<GoogleCalendarClient> logger) : IGoogleCalendarClient
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public Task<TenantGoogleWorkspaceResolvedOptions?> GetTenantOptionsAsync(
        string tenantId,
        CancellationToken cancellationToken = default) =>
        cache.GetAsync<TenantGoogleWorkspaceResolvedOptions>(SharedCacheKeys.TenantGoogleWorkspaceConfig(tenantId));

    public async Task<GoogleCalendarResult> TryUpsertLeaveCalendarEventAsync(
        string tenantId,
        string employeeEmail,
        string subject,
        DateTime startUtc,
        DateTime endUtcExclusive,
        string? transactionId,
        bool clear,
        CancellationToken cancellationToken = default)
    {
        var options = await GetTenantOptionsAsync(tenantId, cancellationToken);
        if (options is null || !options.EnableCalendarBlockOnLeave)
        {
            return new GoogleCalendarResult { Succeeded = true, Skipped = true };
        }

        if (string.IsNullOrWhiteSpace(employeeEmail))
        {
            return new GoogleCalendarResult
            {
                Succeeded = false,
                Error = "Employee email is required for Google calendar block."
            };
        }

        try
        {
            var token = await AcquireDelegatedTokenAsync(options, employeeEmail.Trim(), cancellationToken);
            if (token is null)
            {
                return new GoogleCalendarResult
                {
                    Succeeded = false,
                    Error = "Could not obtain a Google access token."
                };
            }

            using var client = httpClientFactory.CreateClient("google-calendar");
            var eventId = string.IsNullOrWhiteSpace(transactionId)
                ? null
                : $"qimerpleave{transactionId}";

            if (clear)
            {
                if (string.IsNullOrWhiteSpace(eventId))
                    return new GoogleCalendarResult { Succeeded = true, Skipped = true };

                using var deleteRequest = new HttpRequestMessage(
                    HttpMethod.Delete,
                    $"https://www.googleapis.com/calendar/v3/calendars/primary/events/{Uri.EscapeDataString(eventId)}");
                deleteRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var deleteResponse = await client.SendAsync(deleteRequest, cancellationToken);
                if (!deleteResponse.IsSuccessStatusCode && deleteResponse.StatusCode != System.Net.HttpStatusCode.NotFound)
                {
                    return new GoogleCalendarResult
                    {
                        Succeeded = false,
                        Error = $"Google returned {(int)deleteResponse.StatusCode}."
                    };
                }

                return new GoogleCalendarResult { Succeeded = true };
            }

            var body = new Dictionary<string, object?>
            {
                ["summary"] = subject,
                ["transparency"] = "opaque",
                ["status"] = "confirmed",
                ["start"] = new { date = startUtc.ToUniversalTime().ToString("yyyy-MM-dd") },
                ["end"] = new { date = endUtcExclusive.ToUniversalTime().ToString("yyyy-MM-dd") }
            };
            if (!string.IsNullOrWhiteSpace(eventId))
                body["id"] = eventId;

            using var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://www.googleapis.com/calendar/v3/calendars/primary/events")
            {
                Content = JsonContent.Create(body, options: JsonOptions)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            using var response = await client.SendAsync(request, cancellationToken);
            if (response.StatusCode == System.Net.HttpStatusCode.Conflict && !string.IsNullOrWhiteSpace(eventId))
            {
                using var put = new HttpRequestMessage(
                    HttpMethod.Put,
                    $"https://www.googleapis.com/calendar/v3/calendars/primary/events/{Uri.EscapeDataString(eventId)}")
                {
                    Content = JsonContent.Create(body, options: JsonOptions)
                };
                put.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                using var putResponse = await client.SendAsync(put, cancellationToken);
                if (!putResponse.IsSuccessStatusCode)
                {
                    return new GoogleCalendarResult
                    {
                        Succeeded = false,
                        Error = $"Google returned {(int)putResponse.StatusCode}."
                    };
                }

                return new GoogleCalendarResult { Succeeded = true };
            }

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Google calendar event create failed for tenant {TenantId} with status {Status}",
                    tenantId, (int)response.StatusCode);
                return new GoogleCalendarResult
                {
                    Succeeded = false,
                    Error = $"Google returned {(int)response.StatusCode}."
                };
            }

            return new GoogleCalendarResult { Succeeded = true };
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Google calendar upsert failed for tenant {TenantId}", tenantId);
            return new GoogleCalendarResult
            {
                Succeeded = false,
                Error = "Google calendar block update failed."
            };
        }
    }

    private async Task<string?> AcquireDelegatedTokenAsync(
        TenantGoogleWorkspaceResolvedOptions options,
        string userEmail,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.ClientEmail) || string.IsNullOrWhiteSpace(options.PrivateKeyPem))
            return null;

        try
        {
            using var rsa = RSA.Create();
            rsa.ImportFromPem(options.PrivateKeyPem);

            var now = DateTime.UtcNow;
            var credentials = new SigningCredentials(new RsaSecurityKey(rsa), SecurityAlgorithms.RsaSha256)
            {
                CryptoProviderFactory = new CryptoProviderFactory { CacheSignatureProviders = false }
            };

            var token = new JwtSecurityToken(
                issuer: options.ClientEmail,
                audience: "https://oauth2.googleapis.com/token",
                claims:
                [
                    new Claim("scope", "https://www.googleapis.com/auth/calendar.events"),
                    new Claim("sub", userEmail)
                ],
                notBefore: now,
                expires: now.AddMinutes(55),
                signingCredentials: credentials);

            var assertion = new JwtSecurityTokenHandler().WriteToken(token);

            using var tokenClient = httpClientFactory.CreateClient("google-oauth");
            using var content = new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["grant_type"] = "urn:ietf:params:oauth:grant-type:jwt-bearer",
                ["assertion"] = assertion
            });

            using var response = await tokenClient.PostAsync(
                "https://oauth2.googleapis.com/token", content, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: cancellationToken);
            return doc.RootElement.TryGetProperty("access_token", out var access)
                ? access.GetString()
                : null;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Google service-account token acquisition failed");
            return null;
        }
    }
}
