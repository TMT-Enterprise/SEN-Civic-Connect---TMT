namespace CivicConnect.Requests.Requests;

public record SubmitRequestCommand(
    long CategoryId,
    string Description);

public record SubmitRequestResult(
    long RequestId,
    string Status,
    DateTimeOffset SubmittedAt);