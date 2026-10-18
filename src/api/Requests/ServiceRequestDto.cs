public record ServiceRequestDto(
    long RequestId,
    long RequesterId,
    long CategoryId,
    string Category,
    string Status,
    string Description,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? DueAt,
    DateTimeOffset UpdatedAt,
    DateTimeOffset? ClosedAt);