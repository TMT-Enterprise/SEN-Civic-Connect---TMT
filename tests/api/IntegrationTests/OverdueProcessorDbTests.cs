using CivicConnect.Data;
using CivicConnect.Requests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;

namespace CivicConnect.IntegrationTests;

[Collection(ApiCollection.Name)]
public sealed class OverdueProcessorDbTests(ApiFactory factory)
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }

    // Postgres stores microseconds; truncate so "exactly at the due time" really is equal.
    private static DateTimeOffset Now()
    {
        var ticks = DateTimeOffset.UtcNow.UtcTicks;
        return new DateTimeOffset(ticks - (ticks % 10), TimeSpan.Zero);
    }

    private async Task<long> InsertRequestAsync(string status, DateTimeOffset dueAt)
    {
        var requesterId = await factory.ScalarAsync<long>(
            "insert into requester (name, email, password_hash) values ('Overdue Test', @e, 'x') returning requester_id",
            ("e", $"overdue-{Guid.NewGuid():N}@example.com"));

        return await factory.ScalarAsync<long>(
            """
            insert into service_request
                (requester_id, category_id, current_status_id, description, submitted_at, due_at)
            values
                (@r, (select min(category_id) from service_category),
                 (select status_id from request_status where name = @s), 'overdue test', @sub, @due)
            returning request_id
            """,
            ("r", requesterId), ("s", status), ("sub", dueAt.AddDays(-2)), ("due", dueAt));
    }

    private Task<object?> OverdueAtAsync(long id) =>
        factory.ScalarAsync<object>("select overdue_at from service_request where request_id = @id", ("id", id));

    private Task<string?> StatusAsync(long id) =>
        factory.ScalarAsync<string>(
            """
            select rs.name from service_request sr
            join request_status rs on rs.status_id = sr.current_status_id
            where sr.request_id = @id
            """,
            ("id", id));

    private OverdueProcessor ProcessorAt(IServiceScope scope, DateTimeOffset now) => new(
        scope.ServiceProvider.GetRequiredService<IOverdueRepository>(),
        new RequestStatusTransitionValidator(),
        new FixedClock(now),
        NullLogger<OverdueProcessor>.Instance);

    [Fact(DisplayName = "S2-10 / FR-012: past-due open request is flagged with its status preserved; closed and not-yet-due are not")]
    public async Task Flags_past_due_open_requests_and_leaves_the_rest()
    {
        var now = Now();
        var pastDue = await InsertRequestAsync("In Progress", now.AddHours(-1));
        var notYetDue = await InsertRequestAsync("In Progress", now.AddHours(1));
        var closed = await InsertRequestAsync("Closed", now.AddHours(-1));

        using var scope = factory.Services.CreateScope();
        await ProcessorAt(scope, now).RunOnceAsync(CancellationToken.None);

        Assert.NotNull(await OverdueAtAsync(pastDue));
        Assert.Equal("In Progress", await StatusAsync(pastDue));
        Assert.Null(await OverdueAtAsync(notYetDue));
        Assert.Null(await OverdueAtAsync(closed));
    }

    [Fact(DisplayName = "S2-10 / FR-012: nothing is flagged at the due time, one tick later it is, and a re-run changes nothing")]
    public async Task Boundary_and_idempotence_against_the_real_table()
    {
        var due = Now().AddMinutes(-5);
        var id = await InsertRequestAsync("Assigned", due);

        using var scope = factory.Services.CreateScope();

        await ProcessorAt(scope, due).RunOnceAsync(CancellationToken.None);
        Assert.Null(await OverdueAtAsync(id)); // at the due time

        await ProcessorAt(scope, due.AddSeconds(1)).RunOnceAsync(CancellationToken.None);
        var firstFlag = await OverdueAtAsync(id);
        Assert.NotNull(firstFlag); // just after

        await ProcessorAt(scope, due.AddMinutes(10)).RunOnceAsync(CancellationToken.None);
        Assert.Equal(firstFlag, await OverdueAtAsync(id)); // re-run keeps the original flag time
        Assert.Equal("Assigned", await StatusAsync(id));
    }
}