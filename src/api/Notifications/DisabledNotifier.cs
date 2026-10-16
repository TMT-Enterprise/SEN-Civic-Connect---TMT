using CivicConnect.Requests;

namespace CivicConnect.Notifications;

/// <summary>Used while Email:Enabled is false, so rows are logged as Failed instead of silently looking sent.</summary>
internal sealed class DisabledNotifier : INotifier
{
    public Task SendAsync(StatusChangeNotification notification, CancellationToken cancellationToken)
    {
        return Task.FromException(
            new NotificationDeliveryException("Email delivery is not configured (Email:Enabled is false)."));
    }
}