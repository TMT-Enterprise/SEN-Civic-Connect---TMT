using CivicConnect.Data;
using CivicConnect.Data.Entities;
using CivicConnect.Requests;

using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CivicConnect.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class NotificationDbTests(ApiFactory factory)
{
    private sealed class RecordingNotifier : INotifier
    {
        public List<StatusChangeNotification> Sent { get; } = [];

        public Task SendAsync(StatusChangeNotification notification, CancellationToken cancellationToken)
        {
            Sent.Add(notification);
            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingNotifier : INotifier
    {
        public Task SendAsync(StatusChangeNotification notification, CancellationToken cancellationToken)
        {
            return Task.FromException(new NotificationDeliveryException("Provider unavailable (test)."));
        }
    }

    private async Task<long> InsertRequestAsync(string status)
    {
        var requesterId = await factory.ScalarAsync<long>(
            "insert into requester (name, email, password_hash) values ('Notify Test', @e, 'x') returning requester_id",
            ("e", $"notify-{Guid.NewGuid():N}@example.com"));

        return await factory.ScalarAsync<long>(
            """
            insert into service_request (requester_id, category_id, current_status_id, description)
            values (@r, (select min(category_id) from service_category),
                    (select status_id from request_status where name = @s), 'notification test')
            returning request_id
            """,
            ("r", requesterId),
            ("s", status));
    }

    private Task<string?> RequestStatusAsync(long requestId) =>
        factory.ScalarAsync<string>(
            """
            select rs.name from service_request sr
            join request_status rs on rs.status_id = sr.current_status_id
            where sr.request_id = @id
            """,
            ("id", requestId));

    private Task<string?> NotificationColumnAsync(string column, long requestId) =>
        factory.ScalarAsync<string>(
            $"select {column}::text from notification where request_id = @id",
            ("id", requestId));

    private static async Task<Notification?> CommitStatusChangeAsync(
        IServiceProvider services, long requestId, string from, string to)
    {
        var db = services.GetRequiredService<AppDbContext>();
        var notifier = services.GetRequiredService<StatusChangeNotifier>();

        await using var transaction = await db.Database.BeginTransactionAsync();

        var request = await db.ServiceRequests.SingleAsync(r => r.RequestId == requestId);
        request.CurrentStatusId = await db.RequestStatuses
            .Where(s => s.Name == to)
            .Select(s => s.StatusId)
            .SingleAsync();

        var staged = notifier.Stage(requestId, from, to);
        await db.SaveChangesAsync();
        await transaction.CommitAsync();

        return staged;
    }

    [Fact(DisplayName = "S2-09 / FR-006 / ASR-3: after commit the email is sent and the row is Sent with sent_at")]
    public async Task Committed_change_is_sent_and_logged()
    {
        var recorder = new RecordingNotifier();
        await using var host = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<INotifier>();
            s.AddSingleton<INotifier>(recorder);
        }));

        var requestId = await InsertRequestAsync("Submitted");
        using var scope = host.Services.CreateScope();

        var staged = await CommitStatusChangeAsync(scope.ServiceProvider, requestId, "Submitted", "Assigned");
        await scope.ServiceProvider.GetRequiredService<StatusChangeNotifier>().DispatchAsync(staged, CancellationToken.None);

        var sent = Assert.Single(recorder.Sent);
        Assert.Equal(requestId, sent.RequestId);
        Assert.Equal("Assigned", sent.NewStatus);
        Assert.Equal("Sent", await NotificationColumnAsync("status", requestId));
        Assert.NotNull(await NotificationColumnAsync("sent_at", requestId));
    }

    [Fact(DisplayName = "S2-09 / ADR-005 / R-14: provider failure leaves the status change committed and the row Failed")]
    public async Task Provider_failure_does_not_roll_back_the_status_change()
    {
        await using var host = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<INotifier>();
            s.AddSingleton<INotifier>(new ThrowingNotifier());
        }));

        var requestId = await InsertRequestAsync("Submitted");
        using var scope = host.Services.CreateScope();

        var staged = await CommitStatusChangeAsync(scope.ServiceProvider, requestId, "Submitted", "Assigned");
        await scope.ServiceProvider.GetRequiredService<StatusChangeNotifier>().DispatchAsync(staged, CancellationToken.None);

        Assert.Equal("Assigned", await RequestStatusAsync(requestId));
        Assert.Equal("Failed", await NotificationColumnAsync("status", requestId));
        Assert.Equal("Provider unavailable (test).", await NotificationColumnAsync("failure_reason", requestId));
        Assert.Null(await NotificationColumnAsync("sent_at", requestId));
    }

    [Fact(DisplayName = "S2-09 / FR-006: an unchanged status writes no notification row")]
    public async Task Unchanged_status_writes_no_notification()
    {
        await using var host = factory.WithWebHostBuilder(b => b.ConfigureTestServices(s =>
        {
            s.RemoveAll<INotifier>();
            s.AddSingleton<INotifier>(new RecordingNotifier());
        }));

        var requestId = await InsertRequestAsync("Assigned");
        using var scope = host.Services.CreateScope();

        var notifier = scope.ServiceProvider.GetRequiredService<StatusChangeNotifier>();
        Assert.Null(notifier.Stage(requestId, "Assigned", "Assigned"));
        Assert.Null(await NotificationColumnAsync("status", requestId));
    }
}