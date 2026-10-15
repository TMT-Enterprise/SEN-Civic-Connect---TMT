using CivicConnect.Data;
using CivicConnect.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Requests.Requests;

public class RequestsService
{
    private readonly AppDbContext _db;

    public RequestsService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<SubmitRequestResult> SubmitAsync(
        long requesterId,
        SubmitRequestCommand command,
        CancellationToken cancellationToken = default)
    {
        // Validate category exists and is active
        var category = await _db.ServiceCategories
            .FirstOrDefaultAsync(c => c.CategoryId == command.CategoryId && c.IsActive, cancellationToken)
            ?? throw new ArgumentException($"Category {command.CategoryId} does not exist or is inactive.");

        // Get the Submitted status
        var submittedStatus = await _db.RequestStatuses
            .FirstOrDefaultAsync(s => s.Name == "Submitted", cancellationToken)
            ?? throw new InvalidOperationException("Submitted status not found in database.");

        var now = DateTimeOffset.UtcNow;

        var request = new ServiceRequest
        {
            RequesterId = requesterId,
            CategoryId = command.CategoryId,
            CurrentStatusId = submittedStatus.StatusId,
            Description = command.Description,
            SubmittedAt = now,
            UpdatedAt = now
        };

        _db.ServiceRequests.Add(request);
        await _db.SaveChangesAsync(cancellationToken);

        return new SubmitRequestResult(
            request.RequestId,
            submittedStatus.Name,
            request.SubmittedAt);
    }

    public async Task<List<ServiceRequestDto>> GetRequesterHistoryAsync(
        long requesterId,
        CancellationToken cancellationToken = default)
    {
        var requests = await _db.ServiceRequests
            .Include(r => r.CurrentStatus)
            .Where(r => r.RequesterId == requesterId)
            .OrderByDescending(r => r.SubmittedAt)
            .ToListAsync(cancellationToken);

        return requests.Select(r => ToDto(r)).ToList();
    }

    public async Task<ServiceRequestDto?> GetByIdAsync(
        long requestId,
        long requesterId,
        CancellationToken cancellationToken = default)
    {
        var request = await _db.ServiceRequests
            .Include(r => r.CurrentStatus)
            .FirstOrDefaultAsync(
                r => r.RequestId == requestId && r.RequesterId == requesterId,
                cancellationToken);

        return request is null ? null : ToDto(request);
    }

    private static ServiceRequestDto ToDto(ServiceRequest r)
    {
        var status = DeriveStatus(r);
        return new ServiceRequestDto(
            r.RequestId,
            r.RequesterId,
            r.CategoryId,
            status,
            r.Description,
            r.SubmittedAt,
            r.DueAt,
            r.UpdatedAt,
            r.ClosedAt);
    }

    private static string DeriveStatus(ServiceRequest r)
    {
        // Overdue is derived: if due date has passed and request is not closed/resolved
        if (r.DueAt.HasValue
            && r.DueAt.Value < DateTimeOffset.UtcNow
            && r.CurrentStatus?.Name != "Closed"
            && r.CurrentStatus?.Name != "Resolved")
        {
            return "Overdue";
        }

        return r.CurrentStatus?.Name ?? "Unknown";
    }
}