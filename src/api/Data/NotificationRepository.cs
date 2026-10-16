using CivicConnect.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Data;

public sealed class NotificationRepository(AppDbContext db) : INotificationRepository
{
    public Notification Stage(long requestId, string channel, string newStatus, DateTimeOffset createdAt)
    {
        var notification = new Notification
        {
            RequestId = requestId,
            Channel = channel,
            NewStatus = newStatus,
            Status = NotificationStatus.Pending,
            CreatedAt = createdAt
        };

        db.Notifications.Add(notification);
        return notification;
    }

    public Task<NotificationDispatchInfo?> GetDispatchInfoAsync(long notificationId, CancellationToken cancellationToken)
    {
        var query =
            from n in db.Notifications.AsNoTracking()
            join r in db.ServiceRequests.AsNoTracking() on n.RequestId equals r.RequestId
            join q in db.Requesters.AsNoTracking() on r.RequesterId equals q.RequesterId
            where n.NotificationId == notificationId
            select new NotificationDispatchInfo(n.NotificationId, n.RequestId, n.NewStatus, q.Name, q.Email);

        return query.FirstOrDefaultAsync(cancellationToken);
    }

    public async Task MarkSentAsync(long notificationId, DateTimeOffset sentAt, CancellationToken cancellationToken)
    {
        // Only a Pending row can move on, so a Failed or Sent row is never overwritten.
        await db.Notifications
            .Where(n => n.NotificationId == notificationId && n.Status == NotificationStatus.Pending)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(n => n.Status, NotificationStatus.Sent)
                    .SetProperty(n => n.SentAt, (DateTimeOffset?)sentAt),
                cancellationToken);
    }

    public async Task MarkFailedAsync(long notificationId, string reason, CancellationToken cancellationToken)
    {
        await db.Notifications
            .Where(n => n.NotificationId == notificationId && n.Status == NotificationStatus.Pending)
            .ExecuteUpdateAsync(
                s => s
                    .SetProperty(n => n.Status, NotificationStatus.Failed)
                    .SetProperty(n => n.FailureReason, reason),
                cancellationToken);
    }
}