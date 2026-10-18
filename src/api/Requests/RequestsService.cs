using CivicConnect.Data;
using CivicConnect.Data.Entities;

using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Requests.Requests;

public class RequestsService
{
    private readonly AppDbContext _db;
    private readonly TimeProvider _timeProvider;

    public RequestsService(AppDbContext db, TimeProvider timeProvider)
    {
        _db = db;
        _timeProvider = timeProvider;
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
        var requests = await (
            from request in _db.ServiceRequests
            join category in _db.ServiceCategories
                on request.CategoryId equals category.CategoryId
            join status in _db.RequestStatuses
                on request.CurrentStatusId equals status.StatusId
            where request.RequesterId == requesterId
            orderby request.SubmittedAt descending
            select new
            {
                Request = request,
                Category = category.Type,
                Status = status.Name
            })
            .ToListAsync(cancellationToken);

        var now = _timeProvider.GetUtcNow();

        return requests.Select(item =>
        {
            var request = item.Request;

            var status = request.DueAt.HasValue
                && request.DueAt.Value < now
                && item.Status != "Closed"
                && item.Status != "Resolved"
                    ? "Overdue"
                    : item.Status;

            return new ServiceRequestDto(
                request.RequestId,
                request.RequesterId,
                request.CategoryId,
                item.Category,
                status,
                request.Description,
                request.SubmittedAt,
                request.DueAt,
                request.UpdatedAt,
                request.ClosedAt);
        }).ToList();
    }


    
    public async Task<ServiceRequestDto?> GetByIdAsync(
        long requestId,
        long requesterId,
        CancellationToken cancellationToken = default)
    {
        var result = await (
            from request in _db.ServiceRequests
            join category in _db.ServiceCategories
                on request.CategoryId equals category.CategoryId
            where request.RequestId == requestId
                && request.RequesterId == requesterId
            select new
            {
                Request = request,
                Category = category.Type
            })
            .FirstOrDefaultAsync(cancellationToken);

        return result is null
            ? null
            : ToDto(result.Request, _timeProvider.GetUtcNow(), result.Category);
    }


    public Task<RequestSearchResponse> SearchStaffAsync(
        long employeeId,
        RequestSearchFilters filters,
        CancellationToken cancellationToken = default) =>
        SearchAsync(ApplyFilters(
            BuildSearchQuery().Where(r => r.AssignedEmployeeId == employeeId),
            filters), cancellationToken);

    public Task<RequestSearchResponse> SearchAdminAsync(
        RequestSearchFilters filters,
        CancellationToken cancellationToken = default) =>
        SearchAsync(ApplyFilters(BuildSearchQuery(), filters), cancellationToken);

    private IQueryable<ServiceRequest> BuildSearchQuery() =>
        _db.ServiceRequests.AsNoTracking();

    private IQueryable<ServiceRequest> ApplyFilters(
        IQueryable<ServiceRequest> requests,
        RequestSearchFilters filters)
    {
        if (!string.IsNullOrWhiteSpace(filters.Id))
        {
            var pattern = ToLikePattern(filters.Id);
            requests = requests.Where(r =>
                EF.Functions.ILike(r.RequestId.ToString(), pattern, "\\"));
        }

        if (!string.IsNullOrWhiteSpace(filters.Status))
        {
            var pattern = ToLikePattern(filters.Status);
            var now = _timeProvider.GetUtcNow();
            requests = requests.Where(r =>
                (r.DueAt.HasValue
                    && r.DueAt.Value < now
                    && _db.RequestStatuses.Any(s =>
                        s.StatusId == r.CurrentStatusId
                        && s.Name != "Closed"
                        && s.Name != "Resolved")
                    ? EF.Functions.ILike("Overdue", pattern, "\\")
                    : _db.RequestStatuses.Any(s =>
                        s.StatusId == r.CurrentStatusId
                        && EF.Functions.ILike(s.Name, pattern, "\\"))));
        }

        if (filters.DueDateRange is { } dueDateRange)
        {
            requests = requests.Where(r =>
                r.DueAt >= dueDateRange.Start && r.DueAt < dueDateRange.EndExclusive);
        }

        if (!string.IsNullOrWhiteSpace(filters.Category))
        {
            var pattern = ToLikePattern(filters.Category);
            requests = requests.Where(r => _db.ServiceCategories.Any(c =>
                c.CategoryId == r.CategoryId
                && EF.Functions.ILike(c.Type, pattern, "\\")));
        }

        if (!string.IsNullOrWhiteSpace(filters.Assignee))
        {
            var pattern = ToLikePattern(filters.Assignee);
            requests = requests.Where(r => r.AssignedEmployeeId.HasValue
                && _db.Employees.Any(e =>
                    e.EmployeeId == r.AssignedEmployeeId.Value
                    && EF.Functions.ILike(e.Name, pattern, "\\")));
        }

        if (filters.CategoryId is { } categoryId)
        {
            requests = requests.Where(r => r.CategoryId == categoryId);
        }

        if (filters.AssignedEmployeeId is { } assignedEmployeeId)
        {
            requests = requests.Where(r => r.AssignedEmployeeId == assignedEmployeeId);
        }

        if (filters.Unassigned)
        {
            requests = requests.Where(r => r.AssignedEmployeeId == null);
        }

        if (!string.IsNullOrWhiteSpace(filters.Search))
        {
            var search = filters.Search.Trim();
            var pattern = ToLikePattern(search);
            var now = _timeProvider.GetUtcNow();

            var matchesText = requests.Where(r =>
                EF.Functions.ILike(r.RequestId.ToString(), pattern, "\\")
                || (r.DueAt.HasValue
                    && r.DueAt.Value < now
                    && _db.RequestStatuses.Any(s =>
                        s.StatusId == r.CurrentStatusId
                        && s.Name != "Closed"
                        && s.Name != "Resolved")
                    ? EF.Functions.ILike("Overdue", pattern, "\\")
                    : _db.RequestStatuses.Any(s =>
                        s.StatusId == r.CurrentStatusId
                        && EF.Functions.ILike(s.Name, pattern, "\\")))
                || _db.ServiceCategories.Any(c =>
                    c.CategoryId == r.CategoryId
                    && EF.Functions.ILike(c.Type, pattern, "\\"))
                || (r.AssignedEmployeeId.HasValue
                    && _db.Employees.Any(e =>
                        e.EmployeeId == r.AssignedEmployeeId.Value
                        && EF.Functions.ILike(e.Name, pattern, "\\"))));

            if (RequestSearchDateRange.TryParse(search, out var searchDateRange))
            {
                requests = matchesText.Union(requests.Where(r =>
                    r.DueAt >= searchDateRange.Start
                    && r.DueAt < searchDateRange.EndExclusive));
            }
            else
            {
                requests = matchesText;
            }
        }

        return requests;
    }

    private async Task<RequestSearchResponse> SearchAsync(
        IQueryable<ServiceRequest> query,
        CancellationToken cancellationToken)
    {
        var now = _timeProvider.GetUtcNow();
        var requests = await (
            from request in query
            join status in _db.RequestStatuses on request.CurrentStatusId equals status.StatusId
            join category in _db.ServiceCategories on request.CategoryId equals category.CategoryId
            join assignedEmployee in _db.Employees
                on request.AssignedEmployeeId equals (long?)assignedEmployee.EmployeeId into employees
            from assignedEmployee in employees.DefaultIfEmpty()
            orderby request.SubmittedAt descending, request.RequestId descending
            select new RequestSearchRow(
                request.RequestId,
                request.CategoryId,
                request.AssignedEmployeeId,
                status.Name,
                category.Type,
                assignedEmployee == null ? null : assignedEmployee.Name,
                request.Description,
                request.SubmittedAt,
                request.DueAt,
                request.UpdatedAt,
                request.ClosedAt))
            .ToListAsync(cancellationToken);

        var results = requests
            .Select(r => new RequestSearchItemDto(
                r.RequestId,
                r.CategoryId,
                r.Category,
                r.AssignedEmployeeId,
                r.Assignee,
                IsOverdue(r, now) ? "Overdue" : r.Status,
                r.Description,
                r.SubmittedAt,
                r.DueAt,
                r.UpdatedAt,
                r.ClosedAt))
            .ToList();

        return new RequestSearchResponse(
            results,
            results.Count == 0 ? "No requests matched your filters." : null);
    }

    private static bool IsOverdue(RequestSearchRow request, DateTimeOffset now) =>
        request.DueAt.HasValue
        && request.DueAt.Value < now
        && request.Status is not ("Closed" or "Resolved");

    private static string ToLikePattern(string value) =>
        $"%{value.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";

    private static ServiceRequestDto ToDto(
    ServiceRequest r,
    DateTimeOffset now,
    string category)
{
    var status = DeriveStatus(r, now);

    return new ServiceRequestDto(
        r.RequestId,
        r.RequesterId,
        r.CategoryId,
        category,
        status,
        r.Description,
        r.SubmittedAt,
        r.DueAt,
        r.UpdatedAt,
        r.ClosedAt);
}

    private static string DeriveStatus(ServiceRequest r, DateTimeOffset now)
    {
        // Overdue is derived: if due date has passed and request is not closed/resolved
        if (r.DueAt.HasValue
            && r.DueAt.Value < now
            && r.CurrentStatus?.Name != "Closed"
            && r.CurrentStatus?.Name != "Resolved")
        {
            return "Overdue";
        }

        return r.CurrentStatus?.Name ?? "Unknown";
    }

    private sealed record RequestSearchRow(
        long RequestId,
        long CategoryId,
        long? AssignedEmployeeId,
        string Status,
        string Category,
        string? Assignee,
        string Description,
        DateTimeOffset SubmittedAt,
        DateTimeOffset? DueAt,
        DateTimeOffset UpdatedAt,
        DateTimeOffset? ClosedAt);
}