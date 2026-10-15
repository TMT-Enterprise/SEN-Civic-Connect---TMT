namespace CivicConnect.Data;

public sealed class OverdueCandidate
{
    public long RequestId { get; set; }

    public string StatusName { get; set; } = string.Empty;

    public DateTimeOffset DueAt { get; set; }

    public DateTimeOffset? OverdueAt { get; set; }
}

public interface IOverdueRepository
{
    Task<IReadOnlyList<OverdueCandidate>> FindCandidatesAsync(
        DateTimeOffset now,
        long afterRequestId,
        int batchSize,
        CancellationToken cancellationToken);

    Task<bool> MarkOverdueAsync(long requestId, DateTimeOffset now, CancellationToken cancellationToken);
}