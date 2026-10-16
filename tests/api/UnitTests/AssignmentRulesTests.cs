using CivicConnect.Requests;

namespace CivicConnect.UnitTests;

public class AssignmentRulesTests
{
    private static readonly DateTimeOffset Now =
        new(2026, 10, 15, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void ExactlyTwentyFourHours_IsAccepted() =>
        Assert.True(AssignmentRules.IsDueDateAcceptable(Now, Now.AddHours(24)));

    [Fact]
    public void OneTickUnderTwentyFourHours_IsRejected() =>
        Assert.False(AssignmentRules.IsDueDateAcceptable(Now, Now.AddHours(24).AddTicks(-1)));

    [Fact]
    public void OneSecondUnderTwentyFourHours_IsRejected() =>
        Assert.False(AssignmentRules.IsDueDateAcceptable(Now, Now.AddHours(24).AddSeconds(-1)));

    [Fact]
    public void OverTwentyFourHours_IsAccepted() =>
        Assert.True(AssignmentRules.IsDueDateAcceptable(Now, Now.AddHours(24).AddSeconds(1)));

    [Fact]
    public void DueDateInThePast_IsRejected() =>
        Assert.False(AssignmentRules.IsDueDateAcceptable(Now, Now.AddHours(-1)));
}