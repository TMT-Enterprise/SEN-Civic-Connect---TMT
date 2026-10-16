using CivicConnect.Requests;

namespace CivicConnect.UnitTests;

internal sealed class StubNotifier : INotifier
{
    public List<StatusChangeNotification> Sent { get; } = [];

    public Exception? Failure { get; set; }

    public Action? OnSend { get; set; }

    public Task SendAsync(StatusChangeNotification notification, CancellationToken cancellationToken)
    {
        if (Failure is not null)
        {
            return Task.FromException(Failure);
        }

        OnSend?.Invoke();
        Sent.Add(notification);
        return Task.CompletedTask;
    }
}