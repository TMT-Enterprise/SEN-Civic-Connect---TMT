namespace CivicConnect.Data.Entities;

public static class NotificationStatus
{
    public const string Pending = "Pending";
    public const string Sent = "Sent";
    public const string Failed = "Failed";
}

public class Notification
{
    public long NotificationId { get; set; }

    public long RequestId { get; set; }

    public string Channel { get; set; } = string.Empty;

    public string NewStatus { get; set; } = string.Empty;

    public string Status { get; set; } = NotificationStatus.Pending;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset? SentAt { get; set; }

    public string? FailureReason { get; set; }
}