namespace QimErp.Shared.Common.Options;

/// <summary>
/// RabbitMQ configuration. Bind from "RabbitMq" section.
/// Env: RabbitMq__Exchanges__Notify, RabbitMq__Exchanges__WorkflowApprovalRequired, etc.
/// </summary>
public class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    private RabbitMqExchanges? _exchanges;
    public RabbitMqExchanges Exchanges
    {
        get => _exchanges ??= new();
        set => _exchanges = value;
    }
}
