using System.Diagnostics.CodeAnalysis;
namespace fcs.Campaign.Application.Audit;

[ExcludeFromCodeCoverage]

public sealed record AuditLogRequestedEvent(
    Guid EventId,
    DateTime OccurredAt,
    string ServiceName,
    string Action,
    string EntityName,
    string? EntityId,
    Guid? ActorId,
    string? ActorType,
    string? CorrelationId = null,
    string? IpAddress = null,
    string? UserAgent = null,
    IReadOnlyDictionary<string, object?>? Metadata = null)
{
    private const string CampaignServiceName = "fcs-campaigns";

    public static AuditLogRequestedEvent Create(
        string action,
        string entityName,
        Guid? actorId = null,
        string? actorType = null,
        string? entityId = null,
        IReadOnlyDictionary<string, object?>? metadata = null)
    {
        return new AuditLogRequestedEvent(
            Guid.NewGuid(),
            DateTime.UtcNow,
            CampaignServiceName,
            action,
            entityName,
            entityId,
            actorId,
            actorType,
            Metadata: metadata);
    }
}
