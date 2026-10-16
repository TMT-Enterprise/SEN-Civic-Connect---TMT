using CivicConnect.Data.Entities;

namespace CivicConnect.Data;

public sealed record NotificationDispatchInfo(
    long NotificationId,
    long RequestId,
    string NewStatus,
    string RecipientName,
    string RecipientEmail);

public interface INotificationRepository
{
    /// <summary>
    /// Adds a Pending row to the current unit of work. Nothing is written until the caller's
    /// SaveChanges, so the row commits or rolls back together with the status change.
    /// </summary>
    Notification Stage(long requestId, string channel, string newStatus, DateTimeOffset createdAt);

    Task<NotificationDispatchInfo?> GetDispatchInfoAsync(long notificationId, CancellationToken cancellationToken);

    Task MarkSentAsync(long notificationId, DateTimeOffset sentAt, CancellationToken cancellationToken);

    Task MarkFailedAsync(long notificationId, string reason, CancellationToken cancellationToken);
}