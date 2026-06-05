using fcs.Campaign.Application.Abstractions.Messaging;

namespace fcs.Campaign.Application.Audit;

public static class AuditMessagePublisherExtensions
{
    public static void PublishAuditLogFireAndForget(this IMessagePublisher messagePublisher, AuditLogRequestedEvent auditEvent)
    {
        _ = Task.Run(async () =>
        {
            await messagePublisher.PublishAsync(auditEvent, CancellationToken.None);
        }, CancellationToken.None);
    }
}
