namespace CivicConnect.Requests;

public static class AssignmentRules
{
    public static readonly TimeSpan MinLeadTime = TimeSpan.FromHours(24);

    // Under 24h is rejected; exactly 24h is accepted.
    public static bool IsDueDateAcceptable(DateTimeOffset now, DateTimeOffset dueAt) =>
        dueAt - now >= MinLeadTime;
}