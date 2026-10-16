using CivicConnect.Data.Entities;

namespace CivicConnect.Requests;

public sealed record AssignRequestDto(long EmployeeId, DateTimeOffset DueAt);
public sealed record DecideRequestDto(string? Decision, string? RejectionComment);
public sealed record UpdateStatusDto(string? Status, string? CompletionNote);

public sealed record AssignmentDto(
    long AssignmentId,
    long RequestId,
    long EmployeeId,
    DateTimeOffset AssignedAt,
    DateTimeOffset? DueAt,
    string Decision,
    DateTimeOffset? DecisionAt,
    RequestStatus RequestStatus);

public sealed record StatusChangeDto(long RequestId, RequestStatus Status);

public sealed record HistoryEntryDto(
    long HistoryId,
    long RequestId,
    long? EmployeeId,
    string Status,
    DateTimeOffset ChangedAt);

public enum Outcome
{
    Ok,
    NotFound,
    Conflict,
    Invalid,
    Forbidden
}

public sealed record Result<T>(Outcome Outcome, T? Value = default, string? Error = null)
{
    public static Result<T> Ok(T value) => new(Outcome.Ok, value);
    public static Result<T> Fail(Outcome outcome, string error) => new(outcome, default, error);
}