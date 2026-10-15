namespace CivicConnect.Requests.Requests;

public record ServiceRequestDto(
    long RequestId,
    long RequesterId,
    long CategoryId,
    string Status,
    string Description,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ClosedAt);