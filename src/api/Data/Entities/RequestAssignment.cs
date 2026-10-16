namespace CivicConnect.Data.Entities;

public static class AssignmentDecision
{
    public const string Pending = "Pending";
    public const string Accepted = "Accepted";
    public const string Rejected = "Rejected";
}

public class RequestAssignment
{
    public long AssignmentId { get; set; }
    public long RequestId { get; set; }
    public long EmployeeId { get; set; }
    public long AssignedByEmployeeId { get; set; }
    public DateTimeOffset AssignedAt { get; set; }
    public string Decision { get; set; } = AssignmentDecision.Pending;
    public DateTimeOffset? DecisionAt { get; set; }
    public string? RejectionComment { get; set; }
}