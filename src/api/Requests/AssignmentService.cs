using CivicConnect.Data;
using CivicConnect.Data.Entities;

using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace CivicConnect.Requests;

public sealed class AssignmentService(
    AppDbContext db,
    RequestStatusTransitionValidator validator,
    TimeProvider clock)
{
    public Task<List<AssignableEmployeeDto>> GetAssignableEmployeesAsync(
        CancellationToken ct) =>
        db.Employees
            .AsNoTracking()
            .Where(employee => employee.IsActive && employee.Role.Name == Roles.Staff)
            .OrderBy(employee => employee.Name)
            .Select(employee => new AssignableEmployeeDto(
                employee.EmployeeId,
                employee.Name))
            .ToListAsync(ct);

    // ---------- ASSIGN (FR-008) ----------
    public async Task<Result<AssignmentDto>> AssignAsync(
        long requestId,
        long employeeId,
        DateTimeOffset dueAt,
        long actorId,
        CancellationToken ct)
    {
        var now = clock.GetUtcNow();

        if (!AssignmentRules.IsDueDateAcceptable(now, dueAt))
        {
            return Result<AssignmentDto>.Fail(
                Outcome.Invalid,
                "Due date must be at least 24 hours from now.");
        }

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var request = await LockRequestAsync(requestId, ct);
        if (request is null)
        {
            return Result<AssignmentDto>.Fail(Outcome.NotFound, "Request not found.");
        }

        var statuses = await LoadStatusesAsync(ct);
        if (!statuses.TryGetStatus(request.CurrentStatusId, out var current))
        {
            return Result<AssignmentDto>.Fail(Outcome.Invalid, "Request is in a status that cannot be assigned.");
        }

        // The second of two concurrent callers waits on the row lock, then lands here.
        if (current == RequestStatus.Assigned)
        {
            return Result<AssignmentDto>.Fail(Outcome.Conflict, "Request is already assigned.");
        }

        var check = validator.Validate(current, RequestStatus.Assigned);
        if (!check.IsAllowed)
        {
            return Result<AssignmentDto>.Fail(Outcome.Invalid, check.Error ?? "Transition not allowed.");
        }

        var employeeOk = await db.Employees
            .AnyAsync(e => e.EmployeeId == employeeId && e.IsActive, ct);
        if (!employeeOk)
        {
            return Result<AssignmentDto>.Fail(Outcome.NotFound, "Employee not found or inactive.");
        }

        var assignment = new RequestAssignment
        {
            RequestId = requestId,
            EmployeeId = employeeId,
            AssignedByEmployeeId = actorId,
            AssignedAt = now,
            Decision = AssignmentDecision.Pending
        };

        db.RequestAssignments.Add(assignment);
        AddHistory(requestId, actorId, statuses.IdOf(RequestStatus.Assigned), now);

        request.CurrentStatusId = statuses.IdOf(RequestStatus.Assigned);
        request.AssignedEmployeeId = employeeId;
        request.DueAt = dueAt;
        request.UpdatedAt = now;

        try
        {
            // Assignment + status update + history row: one SaveChanges inside one transaction.
            await db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // Backstop: ux_request_assignment_one_open caught a race the row lock missed.
            return Result<AssignmentDto>.Fail(Outcome.Conflict, "Request already has an open assignment.");
        }
        catch (DbUpdateException ex)
            when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.ForeignKeyViolation })
        {
            // Nothing was committed: the transaction is disposed without Commit, so everything rolls back.
            return Result<AssignmentDto>.Fail(Outcome.Invalid, "Employee reference is invalid.");
        }

        return Result<AssignmentDto>.Ok(ToDto(assignment, request, RequestStatus.Assigned));
    }

    // ---------- ACCEPT (FR-010) ----------
    public async Task<Result<AssignmentDto>> AcceptAsync(
        long requestId,
        long employeeId,
        CancellationToken ct)
    {
        var now = clock.GetUtcNow();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var request = await LockRequestAsync(requestId, ct);
        if (request is null)
        {
            return Result<AssignmentDto>.Fail(Outcome.NotFound, "Request not found.");
        }

        var assignment = await FindOpenAssignmentAsync(requestId, ct);
        if (assignment is null)
        {
            return Result<AssignmentDto>.Fail(Outcome.Conflict, "Request has no open assignment.");
        }

        if (assignment.EmployeeId != employeeId)
        {
            return Result<AssignmentDto>.Fail(Outcome.Forbidden, "This assignment belongs to another employee.");
        }

        if (assignment.Decision != AssignmentDecision.Pending)
        {
            return Result<AssignmentDto>.Fail(Outcome.Conflict, "Assignment was already accepted.");
        }

        assignment.Decision = AssignmentDecision.Accepted;
        assignment.DecisionAt = now;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        var statuses = await LoadStatusesAsync(ct);
        statuses.TryGetStatus(request.CurrentStatusId, out var current);
        return Result<AssignmentDto>.Ok(ToDto(assignment, request, current));
    }

    // ---------- REJECT (FR-011) ----------
    public async Task<Result<AssignmentDto>> RejectAsync(
        long requestId,
        long employeeId,
        string? comment,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(comment))
        {
            return Result<AssignmentDto>.Fail(Outcome.Invalid, "A rejection comment is required.");
        }

        var now = clock.GetUtcNow();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var request = await LockRequestAsync(requestId, ct);
        if (request is null)
        {
            return Result<AssignmentDto>.Fail(Outcome.NotFound, "Request not found.");
        }

        var assignment = await FindOpenAssignmentAsync(requestId, ct);
        if (assignment is null)
        {
            return Result<AssignmentDto>.Fail(Outcome.Conflict, "Request has no open assignment.");
        }

        if (assignment.EmployeeId != employeeId)
        {
            return Result<AssignmentDto>.Fail(Outcome.Forbidden, "This assignment belongs to another employee.");
        }

        if (assignment.Decision != AssignmentDecision.Pending)
        {
            return Result<AssignmentDto>.Fail(Outcome.Conflict, "An accepted assignment cannot be rejected.");
        }

        var statuses = await LoadStatusesAsync(ct);
        if (!statuses.TryGetStatus(request.CurrentStatusId, out var current))
        {
            return Result<AssignmentDto>.Fail(Outcome.Invalid, "Request is in an unsupported status.");
        }

        var check = validator.Validate(current, RequestStatus.Submitted);
        if (!check.IsAllowed)
        {
            return Result<AssignmentDto>.Fail(Outcome.Invalid, check.Error ?? "Transition not allowed.");
        }

        assignment.Decision = AssignmentDecision.Rejected;
        assignment.DecisionAt = now;
        assignment.RejectionComment = comment.Trim();

        // Back to the unassigned pool.
        request.CurrentStatusId = statuses.IdOf(RequestStatus.Submitted);
        request.AssignedEmployeeId = null;
        request.UpdatedAt = now;

        AddHistory(requestId, employeeId, statuses.IdOf(RequestStatus.Submitted), now);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Result<AssignmentDto>.Ok(ToDto(assignment, request, RequestStatus.Submitted));
    }

    // ---------- STATUS CHANGE (FR-013) ----------
    public async Task<Result<StatusChangeDto>> ChangeStatusAsync(
        long requestId,
        RequestStatus target,
        string? completionDescription,
        long actorId,
        bool actorIsAdmin,
        CancellationToken ct)
    {
        // Assigned / Submitted only change through assign / reject so assignment rows stay consistent.
        if (target is RequestStatus.Assigned or RequestStatus.Submitted)
        {
            return Result<StatusChangeDto>.Fail(
                Outcome.Invalid,
                "Use the assign and reject endpoints for that status.");
        }

        var now = clock.GetUtcNow();

        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var request = await LockRequestAsync(requestId, ct);
        if (request is null)
        {
            return Result<StatusChangeDto>.Fail(Outcome.NotFound, "Request not found.");
        }

        if (!actorIsAdmin && request.AssignedEmployeeId != actorId)
        {
            return Result<StatusChangeDto>.Fail(Outcome.Forbidden, "Only the assigned employee or an admin can change this request.");
        }

        var statuses = await LoadStatusesAsync(ct);
        if (!statuses.TryGetStatus(request.CurrentStatusId, out var current))
        {
            return Result<StatusChangeDto>.Fail(Outcome.Invalid, "Request is in an unsupported status.");
        }

        // 409 for a disallowed transition, 422 for a missing completion note.
        if (!StatusTransitionMap.IsAllowed(current, target))
        {
            return Result<StatusChangeDto>.Fail(
                Outcome.Conflict,
                $"Transition from {current} to {target} is not permitted.");
        }

        var check = validator.Validate(current, target, completionDescription);
        if (!check.IsAllowed)
        {
            return Result<StatusChangeDto>.Fail(Outcome.Invalid, check.Error ?? "Invalid status change.");
        }

        request.CurrentStatusId = statuses.IdOf(target);
        request.UpdatedAt = now;
        if (target == RequestStatus.Closed)
        {
            request.ClosedAt = now;
        }

        AddHistory(requestId, actorId, statuses.IdOf(target), now);

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return Result<StatusChangeDto>.Ok(new StatusChangeDto(requestId, target));
    }

    // ---------- HISTORY (read-only) ----------
    public async Task<IReadOnlyList<HistoryEntryDto>> GetHistoryAsync(long requestId, CancellationToken ct)
    {
        return await (
            from h in db.RequestStatusHistory.AsNoTracking()
            join s in db.RequestStatuses.AsNoTracking() on h.StatusId equals s.StatusId
            where h.RequestId == requestId
            orderby h.ChangedAt, h.HistoryId
            select new HistoryEntryDto(h.HistoryId, h.RequestId, h.EmployeeId, s.Name, h.ChangedAt))
            .ToListAsync(ct);
    }

    // ---------- helpers ----------
    // SELECT ... FOR UPDATE serialises writers per request. Must run inside a transaction.
    private async Task<ServiceRequest?> LockRequestAsync(long requestId, CancellationToken ct)
    {
        var rows = await db.ServiceRequests
            .FromSqlInterpolated($"SELECT * FROM service_request WHERE request_id = {requestId} FOR UPDATE")
            .ToListAsync(ct);

        return rows.SingleOrDefault();
    }

    private Task<RequestAssignment?> FindOpenAssignmentAsync(long requestId, CancellationToken ct) =>
        db.RequestAssignments.SingleOrDefaultAsync(
            a => a.RequestId == requestId
                 && (a.Decision == AssignmentDecision.Pending || a.Decision == AssignmentDecision.Accepted),
            ct);

    private async Task<StatusLookup> LoadStatusesAsync(CancellationToken ct) =>
        new(await db.RequestStatuses.AsNoTracking().ToListAsync(ct));

    private void AddHistory(long requestId, long? employeeId, long statusId, DateTimeOffset at) =>
        db.RequestStatusHistory.Add(new RequestStatusHistory
        {
            RequestId = requestId,
            EmployeeId = employeeId,
            StatusId = statusId,
            ChangedAt = at
        });

    private static AssignmentDto ToDto(RequestAssignment a, ServiceRequest r, RequestStatus status) =>
        new(a.AssignmentId, a.RequestId, a.EmployeeId, a.AssignedAt, r.DueAt, a.Decision, a.DecisionAt, status);
}