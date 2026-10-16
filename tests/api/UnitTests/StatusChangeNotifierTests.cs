using CivicConnect.Data.Entities;
using CivicConnect.Requests;

using Microsoft.Extensions.Logging.Abstractions;

namespace CivicConnect.UnitTests;

public class StatusChangeNotifierTests
{
    private static readonly DateTimeOffset Start = DateTimeOffset.Parse("2026-10-16T09:00:00Z");

    private readonly FakeNotificationRepository _repository = new();
    private readonly StubNotifier _notifier = new();
    private readonly ManualClock _clock = new(Start);
    private readonly StatusChangeNotifier _service;

    public StatusChangeNotifierTests()
    {
        _service = new StatusChangeNotifier(_repository, _notifier, _clock, NullLogger<StatusChangeNotifier>.Instance);
    }

    private sealed class ManualClock(DateTimeOffset start) : TimeProvider
    {
        private DateTimeOffset _now = start;

        public override DateTimeOffset GetUtcNow() => _now;

        public void Advance(TimeSpan by) => _now += by;
    }

    [Fact(DisplayName = "S2-09 / FR-006: no notification is staged or sent when the status has not changed")]
    public async Task Unchanged_status_stages_and_sends_nothing()
    {
        // Basis: FR-006 acceptance criterion "No email is sent when the status has not been changed".
        var staged = _service.Stage(7, "Assigned", "Assigned");
        await _service.DispatchAsync(staged, CancellationToken.None);

        Assert.Null(staged);
        Assert.Empty(_repository.Rows);
        Assert.Empty(_notifier.Sent);
    }

    [Fact(DisplayName = "S2-09 / ADR-005 / ASR-2: staging only logs a Pending row; nothing is sent until dispatch (after commit)")]
    public void Staging_records_a_pending_row_and_does_not_send()
    {
        // Basis: ADR-005 and ADR-002 section 6.4: the row joins the transaction, the email goes after commit.
        var staged = _service.Stage(7, "Submitted", "Assigned");

        var row = Assert.Single(_repository.Rows);
        Assert.Same(staged, row);
        Assert.Equal(NotificationStatus.Pending, row.Status);
        Assert.Equal("Assigned", row.NewStatus);
        Assert.Equal(Start, row.CreatedAt);
        Assert.Null(row.SentAt);
        Assert.Empty(_notifier.Sent);
    }

    [Fact(DisplayName = "S2-09 / FR-006: dispatch sends the request id, new status and requester address, then logs sent_at")]
    public async Task Dispatch_sends_to_the_requester_and_marks_the_row_sent()
    {
        _repository.RecipientEmail = "grace@example.com";
        var staged = _service.Stage(42, "Assigned", "In Progress");

        await _service.DispatchAsync(staged, CancellationToken.None);

        var sent = Assert.Single(_notifier.Sent);
        Assert.Equal(42, sent.RequestId);
        Assert.Equal("In Progress", sent.NewStatus);
        Assert.Equal("grace@example.com", sent.RecipientEmail);
        Assert.Equal(NotificationStatus.Sent, _repository.Rows.Single().Status);
        Assert.NotNull(_repository.Rows.Single().SentAt);
    }

    [Fact(DisplayName = "S2-09 / ASR-3: created_at to sent_at is measurable and inside the 60 second target")]
    public async Task Created_at_to_sent_at_measures_delivery_time()
    {
        _notifier.OnSend = () => _clock.Advance(TimeSpan.FromSeconds(2));
        var staged = _service.Stage(1, "Submitted", "Assigned");

        await _service.DispatchAsync(staged, CancellationToken.None);

        var row = _repository.Rows.Single();
        var elapsed = row.SentAt!.Value - row.CreatedAt;
        Assert.Equal(TimeSpan.FromSeconds(2), elapsed);
        Assert.True(elapsed <= TimeSpan.FromSeconds(60));
    }

    [Fact(DisplayName = "S2-09 / ADR-005 / R-14: provider failure does not throw and is logged as Failed without personal data")]
    public async Task Provider_failure_is_recorded_not_raised()
    {
        // Basis: ADR-006 section 9.1: a failed send must not undo or fail the committed status change.
        _notifier.Failure = new InvalidOperationException("550 mailbox ada@example.com rejected");
        var staged = _service.Stage(1, "Submitted", "Assigned");

        await _service.DispatchAsync(staged, CancellationToken.None); // must not throw

        var row = _repository.Rows.Single();
        Assert.Equal(NotificationStatus.Failed, row.Status);
        Assert.Null(row.SentAt);
        Assert.Equal(nameof(InvalidOperationException), row.FailureReason);
        Assert.DoesNotContain("ada@example.com", row.FailureReason);
    }
}