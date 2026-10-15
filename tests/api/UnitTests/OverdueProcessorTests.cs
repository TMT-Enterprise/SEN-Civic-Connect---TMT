using CivicConnect.Data;
using CivicConnect.Requests;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace CivicConnect.UnitTests;

public class OverdueProcessorTests
{
    private static readonly DateTimeOffset Due = DateTimeOffset.Parse("2026-10-20T13:00:00Z");

    private static OverdueCandidate Request(long id, string status, DateTimeOffset due, DateTimeOffset? overdueAt = null) =>
        new() { RequestId = id, StatusName = status, DueAt = due, OverdueAt = overdueAt };

    private static OverdueProcessor ProcessorAt(FakeOverdueRepository repository, DateTimeOffset now) =>
        new(repository, new RequestStatusTransitionValidator(), new FixedTimeProvider(now), NullLogger<OverdueProcessor>.Instance);

    // ---- Clock-based test at and after the due time (FR-012)

    [Theory(DisplayName = "S2-10 / FR-012: flagged only once the clock is past the due time")]
    [InlineData(-3600, false)] // an hour before
    [InlineData(0, false)]     // exactly at the due time
    [InlineData(1, true)]      // one second after
    [InlineData(86400, true)]  // a day after
    public async Task Request_is_flagged_only_after_the_due_time(int secondsAfterDue, bool expectFlagged)
    {
        var repository = new FakeOverdueRepository { Requests = { Request(1, "Assigned", Due) } };

        var result = await ProcessorAt(repository, Due.AddSeconds(secondsAfterDue)).RunOnceAsync(CancellationToken.None);

        Assert.Equal(expectFlagged, repository.Flagged.ContainsKey(1));
        Assert.Equal(expectFlagged ? 1 : 0, result.Flagged);
    }

    // ---- Underlying status preserved

    [Theory(DisplayName = "S2-10 / FR-012 / ADR-004: flagging leaves the underlying status unchanged")]
    [InlineData("Submitted")]
    [InlineData("Assigned")]
    [InlineData("In Progress")]
    public async Task Flagging_preserves_the_underlying_status(string status)
    {
        var repository = new FakeOverdueRepository { Requests = { Request(1, status, Due) } };
        var now = Due.AddMinutes(5);

        await ProcessorAt(repository, now).RunOnceAsync(CancellationToken.None);

        var stored = repository.Requests.Single();
        Assert.Equal(status, stored.StatusName);
        Assert.Equal(now, stored.OverdueAt);
    }

    // ---- Goes through the TransitionValidator

    [Fact(DisplayName = "S2-10 / ADR-004: closed and already-flagged requests are rejected by the validator")]
    public async Task Closed_and_already_flagged_requests_are_not_flagged()
    {
        var earlier = Due.AddMinutes(1);
        var repository = new FakeOverdueRepository
        {
            Requests =
            {
                Request(1, "Closed", Due),
                Request(2, "In Progress", Due, overdueAt: earlier)
            }
        };

        var result = await ProcessorAt(repository, Due.AddHours(1)).RunOnceAsync(CancellationToken.None);

        Assert.Empty(repository.Flagged);
        Assert.Equal(new OverdueRunResult(2, 0, 2), result);
        Assert.Equal(earlier, repository.Requests[1].OverdueAt); // original flag time untouched
    }

    [Fact(DisplayName = "S2-10: a status name the enum does not know is skipped, not guessed")]
    public async Task Unrecognised_status_is_skipped()
    {
        var repository = new FakeOverdueRepository { Requests = { Request(1, "Resolved", Due) } };

        var result = await ProcessorAt(repository, Due.AddHours(1)).RunOnceAsync(CancellationToken.None);

        Assert.Empty(repository.Flagged);
        Assert.Equal(1, result.Skipped);
    }

    [Fact(DisplayName = "S2-10: a request closed between select and update is not flagged")]
    public async Task Request_closed_concurrently_is_not_flagged()
    {
        var repository = new FakeOverdueRepository
        {
            Requests = { Request(1, "In Progress", Due) },
            RejectMarking = true
        };

        var result = await ProcessorAt(repository, Due.AddHours(1)).RunOnceAsync(CancellationToken.None);

        Assert.Equal(new OverdueRunResult(1, 0, 1), result);
    }

    // ---- Catch-up (R-13)

    [Fact(DisplayName = "S2-10 / R-13: one run flags everything missed, across several batches")]
    public async Task One_run_catches_up_on_everything_that_was_missed()
    {
        var repository = new FakeOverdueRepository();
        const int count = (OverdueProcessor.BatchSize * 2) + 50;
        for (var id = 1; id <= count; id++)
        {
            repository.Requests.Add(Request(id, "Assigned", Due.AddDays(-(id % 10))));
        }

        var result = await ProcessorAt(repository, Due.AddDays(30)).RunOnceAsync(CancellationToken.None);

        Assert.Equal(count, result.Flagged);
        Assert.Equal(count, repository.Flagged.Count);
    }

    [Fact(DisplayName = "S2-10 / R-13: the start-up run flags requests that fell due while the host was asleep")]
    public async Task Start_up_run_catches_up_on_requests_missed_while_the_host_was_asleep()
    {
        var now = DateTimeOffset.UtcNow;
        var done = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var repository = new FakeOverdueRepository
        {
            Requests = { Request(1, "Assigned", now.AddDays(-3)), Request(2, "Submitted", now.AddDays(-10)) }
        };
        repository.OnMarked = () =>
        {
            if (repository.Flagged.Count == 2)
            {
                done.TrySetResult();
            }
        };

        await using var provider = new ServiceCollection()
            .AddLogging()
            .AddSingleton<IOverdueRepository>(repository)
            .AddSingleton<TimeProvider>(new FixedTimeProvider(now))
            .AddSingleton<RequestStatusTransitionValidator>()
            .AddScoped<OverdueProcessor>()
            .BuildServiceProvider();

        var service = new OverdueBackgroundService(
            provider.GetRequiredService<IServiceScopeFactory>(),
            Options.Create(new OverdueOptions { IntervalSeconds = 3600 }), // only the start-up run happens
            NullLogger<OverdueBackgroundService>.Instance);

        await service.StartAsync(CancellationToken.None);
        await done.Task.WaitAsync(TimeSpan.FromSeconds(10));
        await service.StopAsync(CancellationToken.None);

        Assert.Equal(2, repository.Flagged.Count);
    }
}