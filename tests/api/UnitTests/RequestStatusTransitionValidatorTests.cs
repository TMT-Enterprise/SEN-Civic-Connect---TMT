using CivicConnect.Requests;

namespace CivicConnect.UnitTests;

public class RequestStatusTransitionValidatorTests
{
    private readonly RequestStatusTransitionValidator _validator = new();

    public static IEnumerable<object[]> TransitionMatrix()
    {
        var statuses = Enum.GetValues<RequestStatus>();

        foreach (var current in statuses)
        {
            foreach (var requested in statuses)
            {
                var expected =
                    (current, requested) switch
                    {
                        (RequestStatus.Submitted, RequestStatus.Assigned) => true,
                        (RequestStatus.Assigned, RequestStatus.InProgress) => true,
                        (RequestStatus.InProgress, RequestStatus.Closed) => true,
                        _ => false
                    };

                yield return [current, requested, expected];
            }
        }
    }

    [Theory]
    [MemberData(nameof(TransitionMatrix))]
    public void Validate_EnforcesEveryStatusPair(
        RequestStatus current,
        RequestStatus requested,
        bool expectedAllowed)
    {
        var result = _validator.Validate(
            current,
            requested,
            completionDescription: "Work completed successfully");

        Assert.Equal(expectedAllowed, result.IsAllowed);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Validate_RejectsClosingWithoutValidCompletionDescription(
        string? description)
    {
        var result = _validator.Validate(
            RequestStatus.InProgress,
            RequestStatus.Closed,
            description);

        Assert.False(result.IsAllowed);
        Assert.NotNull(result.Error);
    }

    [Fact]
    public void Validate_AllowsClosingWithCompletionDescription()
    {
        var result = _validator.Validate(
            RequestStatus.InProgress,
            RequestStatus.Closed,
            "Repair completed and verified.");

        Assert.True(result.IsAllowed);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Validate_RejectsBackwardTransition()
    {
        var result = _validator.Validate(
            RequestStatus.InProgress,
            RequestStatus.Assigned);

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void Validate_RejectsSkippedTransition()
    {
        var result = _validator.Validate(
            RequestStatus.Submitted,
            RequestStatus.Closed,
            "Completed");

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void Validate_RejectsTransitionOutOfClosed()
    {
        var result = _validator.Validate(
            RequestStatus.Closed,
            RequestStatus.InProgress);

        Assert.False(result.IsAllowed);
    }

    [Fact]
    public void Validate_RejectsSameStatus()
    {
        var result = _validator.Validate(
            RequestStatus.Assigned,
            RequestStatus.Assigned);

        Assert.False(result.IsAllowed);
    }
}