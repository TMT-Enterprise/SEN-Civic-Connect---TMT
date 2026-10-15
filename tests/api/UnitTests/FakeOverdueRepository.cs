using CivicConnect.Data;

namespace CivicConnect.UnitTests;

internal sealed class FakeOverdueRepository : IOverdueRepository
{
    public List<OverdueCandidate> Requests { get; } = [];

    public Dictionary<long, DateTimeOffset> Flagged { get; } = [];

    public bool RejectMarking { get; set; }

    public Action? OnMarked { get; set; }

    public Task<IReadOnlyList<OverdueCandidate>> FindCandidatesAsync(
        DateTimeOffset now, long afterRequestId, int batchSize, CancellationToken cancellationToken)
    {
        IReadOnlyList<OverdueCandidate> batch = Requests
            .Where(r => r.RequestId > afterRequestId)
            .OrderBy(r => r.RequestId)
            .Take(batchSize)
            .ToList();

        return Task.FromResult(batch);
    }

    public Task<bool> MarkOverdueAsync(long requestId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        if (RejectMarking)
        {
            return Task.FromResult(false);
        }

        Requests.Single(r => r.RequestId == requestId).OverdueAt = now;
        Flagged[requestId] = now;
        OnMarked?.Invoke();

        return Task.FromResult(true);
    }
}