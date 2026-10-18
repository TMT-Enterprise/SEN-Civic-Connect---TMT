using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Data;

public sealed class OverdueRepository(AppDbContext db) : IOverdueRepository
{
    public async Task<IReadOnlyList<OverdueCandidate>> FindCandidatesAsync(
        DateTimeOffset now,
        long afterRequestId,
        int batchSize,
        CancellationToken cancellationToken)
    {
        var rows = await db.Database.SqlQuery<OverdueCandidate>($"""
            SELECT sr.request_id AS "RequestId",
                   rs.name       AS "StatusName",
                   sr.due_at     AS "DueAt",
                   sr.overdue_at AS "OverdueAt"
            FROM service_request sr
            JOIN request_status rs ON rs.status_id = sr.current_status_id
            WHERE sr.overdue_at IS NULL
              AND rs.name NOT IN ('Closed', 'Resolved', 'Withdrawn')
              AND sr.due_at < {now}
              AND sr.request_id > {afterRequestId}
            ORDER BY sr.request_id
            LIMIT {batchSize}
            """).ToListAsync(cancellationToken);

        return rows.OrderBy(r => r.RequestId).ToList();
    }

    public async Task<bool> MarkOverdueAsync(long requestId, DateTimeOffset now, CancellationToken cancellationToken)
    {
        var affected = await db.Database.ExecuteSqlInterpolatedAsync($"""
            UPDATE service_request
            SET overdue_at = {now}
            WHERE request_id = {requestId}
              AND overdue_at IS NULL
              AND due_at < {now}
              AND current_status_id NOT IN (
                SELECT status_id
                FROM request_status
                WHERE name IN ('Closed', 'Resolved', 'Withdrawn')
            )
            """, cancellationToken);

        return affected == 1;
    }
}