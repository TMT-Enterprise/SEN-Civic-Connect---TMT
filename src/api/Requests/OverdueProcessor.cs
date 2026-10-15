using CivicConnect.Data;

using Microsoft.Extensions.Logging;

namespace CivicConnect.Requests;

public sealed record OverdueRunResult(int Examined, int Flagged, int Skipped);

public sealed class OverdueProcessor(
    IOverdueRepository repository,
    RequestStatusTransitionValidator validator,
    TimeProvider clock,
    ILogger<OverdueProcessor> logger)
{
    public const int BatchSize = 200;

    public async Task<OverdueRunResult> RunOnceAsync(CancellationToken cancellationToken)
    {
        var now = clock.GetUtcNow(); // one reading per run, so every decision in it is consistent
        long afterRequestId = 0;
        int examined = 0;
        int flagged = 0;
        int skipped = 0;

        while (true)
        {
            var batch = await repository.FindCandidatesAsync(now, afterRequestId, BatchSize, cancellationToken);
            if (batch.Count == 0)
            {
                break;
            }

            foreach (var candidate in batch)
            {
                examined++;

                if (!RequestStatusNames.TryParse(candidate.StatusName, out var status))
                {
                    logger.LogWarning(
                        "Overdue check skipped request {RequestId}: unrecognised status '{Status}'.",
                        candidate.RequestId,
                        candidate.StatusName);
                    skipped++;
                    continue;
                }

                var decision = validator.ValidateOverdueFlag(
                    status, candidate.DueAt, now, alreadyFlagged: candidate.OverdueAt is not null);

                if (decision.IsAllowed && await repository.MarkOverdueAsync(candidate.RequestId, now, cancellationToken))
                {
                    flagged++;
                }
                else
                {
                    skipped++;
                }
            }

            afterRequestId = batch.Max(c => c.RequestId);
        }

        if (examined > 0)
        {
            logger.LogInformation(
                "Overdue run: examined {Examined}, flagged {Flagged}, skipped {Skipped}.", examined, flagged, skipped);
        }

        return new OverdueRunResult(examined, flagged, skipped);
    }
}