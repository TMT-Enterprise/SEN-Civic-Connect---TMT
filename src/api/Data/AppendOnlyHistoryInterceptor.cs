using CivicConnect.Data.Entities;

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace CivicConnect.Data;

/// <summary>Blocks EF from ever updating or deleting request_status_history rows.</summary>
public sealed class AppendOnlyHistoryInterceptor : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        Guard(eventData.Context);
        return result;
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        Guard(eventData.Context);
        return ValueTask.FromResult(result);
    }

    private static void Guard(DbContext? context)
    {
        if (context is null)
        {
            return;
        }

        var illegal = context.ChangeTracker
            .Entries<RequestStatusHistory>()
            .Any(e => e.State is EntityState.Modified or EntityState.Deleted);

        if (illegal)
        {
            throw new InvalidOperationException(
                "request_status_history is insert-only.");
        }
    }
}