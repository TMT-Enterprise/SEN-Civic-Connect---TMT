using CivicConnect.Requests;

namespace CivicConnect.UnitTests;

public class OverdueFlagValidatorTests
{
    private static readonly DateTimeOffset Due = DateTimeOffset.Parse("2026-10-20T13:00:00Z");

    private readonly RequestStatusTransitionValidator _validator = new();

    // Every status x time (before / at / after due) x already-flagged combination.
    public static IEnumerable<object[]> Matrix()
    {
        foreach (var status in Enum.GetValues<RequestStatus>())
        {
            foreach (var secondsAfterDue in new[] { -60, 0, 1 })
            {
                foreach (var alreadyFlagged in new[] { false, true })
                {
                    var expected = status is not (RequestStatus.Resolved or RequestStatus.Closed) && !alreadyFlagged && secondsAfterDue > 0;
                    yield return [status, secondsAfterDue, alreadyFlagged, expected];
                }
            }
        }
    }

    [Theory(DisplayName = "S2-10 / FR-012 / ADR-004: overdue-flag decision matrix")]
    [MemberData(nameof(Matrix))]
    public void ValidateOverdueFlag_follows_the_FR012_rules(
        RequestStatus status, int secondsAfterDue, bool alreadyFlagged, bool expected)
    {
        var result = _validator.ValidateOverdueFlag(status, Due, Due.AddSeconds(secondsAfterDue), alreadyFlagged);

        Assert.Equal(expected, result.IsAllowed);
        Assert.Equal(expected, result.Error is null);
    }

    [Fact(DisplayName = "S2-10 / FR-012: a request with no due date is never flagged")]
    public void ValidateOverdueFlag_rejects_a_missing_due_date()
    {
        var result = _validator.ValidateOverdueFlag(RequestStatus.Assigned, null, Due, alreadyFlagged: false);

        Assert.False(result.IsAllowed);
    }

    [Fact(DisplayName = "S2-10 / ADR-004: Overdue is a flag, never a status that can be requested")]
    public void Overdue_is_not_a_request_status() =>
        Assert.DoesNotContain("Overdue", Enum.GetNames<RequestStatus>());
}