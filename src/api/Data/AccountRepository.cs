using Microsoft.EntityFrameworkCore;

using Npgsql;

namespace CivicConnect.Data;

public sealed class AccountRepository(AppDbContext db) : IAccountRepository
{
    public async Task<AccountRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken)
    {
        // 1. Check Employee table first (Admin or Staff)
        var employee = await db.Employees
            .AsNoTracking()
            .Where(e => e.Email.ToLower() == email)
            .Select(e => new AccountRecord(
                e.EmployeeId, AccountTypes.Employee, e.Name, e.Email, e.PasswordHash, e.Role.Name, e.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        if (employee is not null)
        {
            return employee;
        }

        // 2. If not found in employee, check Requester table
        return await db.Requesters
            .AsNoTracking()
            .Where(r => r.Email.ToLower() == email)
            .Select(r => new AccountRecord(
                r.RequesterId, AccountTypes.Requester, r.Name, r.Email, r.PasswordHash, Roles.Requester, r.IsActive))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken)
    {
        return await db.Employees.AnyAsync(e => e.Email.ToLower() == email, cancellationToken)
            || await db.Requesters.AnyAsync(r => r.Email.ToLower() == email, cancellationToken);
    }

    public async Task<AccountRecord> CreateRequesterAsync(
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken)
    {
        var requester = new Requester
        {
            Name = name,
            Email = email,
            PasswordHash = passwordHash,
            IsActive = true,
            CreatedAt = DateTimeOffset.UtcNow
        };

        db.Requesters.Add(requester);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            // The pre-check can lose a race with a concurrent registration; the unique index is the real guard (FR-001).
            throw new DuplicateEmailException(ex);
        }

        return new AccountRecord(
            requester.RequesterId, AccountTypes.Requester, requester.Name, requester.Email,
            requester.PasswordHash, Roles.Requester, requester.IsActive);
    }
}