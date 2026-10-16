namespace CivicConnect.Requests;

public sealed record StatusChangeNotification(
    long NotificationId,
    long RequestId,
    string NewStatus,
    string RecipientName,
    string RecipientEmail);

public interface INotifier
{
    Task SendAsync(StatusChangeNotification notification, CancellationToken cancellationToken);
}

public sealed class NotificationDeliveryException : Exception
{
    public NotificationDeliveryException(string message)
        : base(message)
    {
    }

    public NotificationDeliveryException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}