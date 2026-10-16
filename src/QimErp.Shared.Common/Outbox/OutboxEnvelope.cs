namespace QimErp.Shared.Common.Outbox;

/// <summary>
/// Wraps an <see cref="Entities.OutboxMessage.Payload"/> with the tenant (and, when the write
/// was company-scoped, the company) the message belongs to.
/// <see cref="OutboxProcessorBackgroundService{TContext}"/> reads <see cref="TenantId"/> and
/// <see cref="CompanyId"/> before touching <see cref="Data"/> so ambient tenant/company context
/// is seeded before dispatch — redelivery must enforce the same company isolation the original
/// save did, never a cross-company "Inactive" scope.
/// </summary>
public sealed record OutboxEnvelope(string TenantId, string Data, string CompanyId = "")
{
    public static string Wrap(object message, string tenantId, string? companyId = null)
    {
        var data = JsonSerializer.Serialize(message, message.GetType());
        return JsonSerializer.Serialize(new OutboxEnvelope(tenantId ?? string.Empty, data, companyId ?? string.Empty));
    }

    public static OutboxEnvelope? Unwrap(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        return JsonSerializer.Deserialize<OutboxEnvelope>(payload);
    }
}
