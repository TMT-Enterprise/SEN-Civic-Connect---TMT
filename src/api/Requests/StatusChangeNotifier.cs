using CivicConnect.Data;
using CivicConnect.Data.Entities;

using Microsoft.Extensions.Logging;

namespace CivicConnect.Requests;

public sealed class StatusChangeNotifier(
    INotificationRepository repository,
    INotifier notifier,
    TimeProvider clock,
    ILogger<StatusChangeNotifier> logger)
{
    public const string EmailChannel = "Email";

    private const int MaxReasonLength = 500;

    /// <summary>Returns null, and writes nothing, when the status did not change.</summary>
    public Notification? Stage(long requestId, string? previousStatus, string newStatus)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(newStatus);

        if (string.Equals(previousStatus, newStatus, StringComparison.Ordinal))
        {
            return null;
        }

        return repository.Stage(requestId, EmailChannel, newStatus, clock.GetUtcNow());
    }

    /// <summary>Never throws: a delivery problem is recorded on the row, not raised to the caller.</summary>
    public async Task DispatchAsync(Notification? staged, CancellationToken cancellationToken)
    {
        if (staged is null)
        {
            return;
        }

        try
        {
            var info = await repository.GetDispatchInfoAsync(staged.NotificationId, cancellationToken)
                ?? throw new NotificationDeliveryException("The notification or its requester could not be found.");

            await notifier.SendAsync(
                new StatusChangeNotification(
                    info.NotificationId,
                    info.RequestId,
                    info.NewStatus,
                    info.RecipientName,
                    info.RecipientEmail),
                cancellationToken);

            await repository.MarkSentAsync(staged.NotificationId, clock.GetUtcNow(), cancellationToken);
        }
        catch (Exception ex)
        {
            await RecordFailureAsync(staged.NotificationId, ex);
        }
    }

    private async Task RecordFailureAsync(long notificationId, Exception ex)
    {
        // Our own exception messages carry no personal data. For anything else only the type name is kept,
        // because provider errors can echo the recipient's address (R-15).
        var reason = ex is NotificationDeliveryException ? ex.Message : ex.GetType().Name;
        if (reason.Length > MaxReasonLength)
        {
            reason = reason[..MaxReasonLength];
        }

        logger.LogWarning("Notification {NotificationId} was not delivered: {Reason}", notificationId, reason);

        try
        {
            await repository.MarkFailedAsync(notificationId, reason, CancellationToken.None);
        }
        catch (Exception markEx)
        {
            logger.LogError(
                "Notification {NotificationId} could not be marked Failed ({ErrorType}); it stays Pending.",
                notificationId,
                markEx.GetType().Name);
        }
    }
}