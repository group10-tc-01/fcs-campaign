using Fcg.Campaign.Application.Abstractions.Messaging;

namespace Fcg.Campaign.Application.Audit;

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
