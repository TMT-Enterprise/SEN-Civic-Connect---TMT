using CivicConnect.Data;
using CivicConnect.Data.Entities;

namespace CivicConnect.UnitTests;

internal sealed class FakeNotificationRepository : INotificationRepository
{
    private long _nextId = 1;

    public List<Notification> Rows { get; } = [];

    public string RecipientName { get; set; } = "Ada Lovelace";

    public string RecipientEmail { get; set; } = "ada@example.com";

    public Notification Stage(long requestId, string channel, string newStatus, DateTimeOffset createdAt)
    {
        var row = new Notification
        {
            NotificationId = _nextId++,
            RequestId = requestId,
            Channel = channel,
            NewStatus = newStatus,
            Status = NotificationStatus.Pending,
            CreatedAt = createdAt
        };

        Rows.Add(row);
        return row;
    }

    public Task<NotificationDispatchInfo?> GetDispatchInfoAsync(long notificationId, CancellationToken cancellationToken)
    {
        var row = Rows.SingleOrDefault(r => r.NotificationId == notificationId);
        NotificationDispatchInfo? info = row is null
            ? null
            : new NotificationDispatchInfo(row.NotificationId, row.RequestId, row.NewStatus, RecipientName, RecipientEmail);

        return Task.FromResult(info);
    }

    public Task MarkSentAsync(long notificationId, DateTimeOffset sentAt, CancellationToken cancellationToken)
    {
        var row = Rows.Single(r => r.NotificationId == notificationId);
        if (row.Status == NotificationStatus.Pending)
        {
            row.Status = NotificationStatus.Sent;
            row.SentAt = sentAt;
        }

        return Task.CompletedTask;
    }

    public Task MarkFailedAsync(long notificationId, string reason, CancellationToken cancellationToken)
    {
        var row = Rows.Single(r => r.NotificationId == notificationId);
        if (row.Status == NotificationStatus.Pending)
        {
            row.Status = NotificationStatus.Failed;
            row.FailureReason = reason;
        }

        return Task.CompletedTask;
    }
}