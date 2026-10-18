using CivicConnect.Data;
using CivicConnect.Data.Entities;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;

namespace CivicConnect.Requests;

public static class ManagementEndpoints
{
    public static IEndpointRouteBuilder MapManagementEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var api = endpoints.MapGroup("/api/v1").WithTags("Management");
        api.MapGet("/categories", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.ServiceCategories.AsNoTracking().Where(c => c.IsActive)
                .OrderBy(c => c.Type).Select(c => new CategoryDto(c.CategoryId, c.Type)).ToListAsync(ct)))
            .RequireAuthorization();

        api.MapGet("/categories/{id:long}", async (long id, AppDbContext db, CancellationToken ct) =>
        {
            var category = await db.ServiceCategories.AsNoTracking().FirstOrDefaultAsync(c => c.CategoryId == id, ct);
            return category is null ? Results.NotFound() : Results.Ok(new CategoryDto(category.CategoryId, category.Type));
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        api.MapPost("/categories", async (CategoryWriteDto dto, AppDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Type)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["type"] = ["Category name is required."] });
            var type = dto.Type.Trim();
            if (await db.ServiceCategories.AnyAsync(c => c.Type.ToLower() == type.ToLower(), ct)) return Results.Conflict(new { message = "A category with this name already exists." });
            var category = new ServiceCategory { Type = type, IsActive = true };
            db.ServiceCategories.Add(category); await db.SaveChangesAsync(ct);
            return Results.Created($"/api/v1/categories/{category.CategoryId}", new CategoryDto(category.CategoryId, category.Type));
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        api.MapPut("/categories/{id:long}", async (long id, CategoryWriteDto dto, AppDbContext db, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(dto.Type)) return Results.ValidationProblem(new Dictionary<string, string[]> { ["type"] = ["Category name is required."] });
            var category = await db.ServiceCategories.FirstOrDefaultAsync(c => c.CategoryId == id, ct);
            if (category is null) return Results.NotFound();
            category.Type = dto.Type.Trim(); category.IsActive = dto.IsActive;
            await db.SaveChangesAsync(ct); return Results.Ok(new CategoryDto(category.CategoryId, category.Type));
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        api.MapDelete("/categories/{id:long}", async (long id, AppDbContext db, CancellationToken ct) =>
        {
            var category = await db.ServiceCategories.FirstOrDefaultAsync(c => c.CategoryId == id, ct);
            if (category is null) return Results.NotFound();
            category.IsActive = false; await db.SaveChangesAsync(ct); return Results.NoContent();
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        api.MapGet("/admin/staff", async (AppDbContext db, CancellationToken ct) =>
            Results.Ok(await db.Employees.AsNoTracking().Where(e => e.IsActive && e.Role.Name == Roles.Staff)
                .OrderBy(e => e.Name).Select(e => new StaffDto(e.EmployeeId, e.Name, e.Email)).ToListAsync(ct)))
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin));

        api.MapGet("/management/dashboard", async (AppDbContext db, CancellationToken ct) =>
        {
            var now = DateTimeOffset.UtcNow;
            var rows = await db.ServiceRequests.AsNoTracking()
                .Join(db.RequestStatuses, r => r.CurrentStatusId, s => s.StatusId, (r, s) => new { r.RequestId, r.CategoryId, r.AssignedEmployeeId, r.DueAt, Status = s.Name })
                .ToListAsync(ct);
            var byStatus = rows.GroupBy(r => r.Status).Select(g => new { status = g.Key, count = g.Count() }).OrderBy(x => x.status).ToList();
            var byCategory = await db.ServiceRequests.AsNoTracking().Join(db.ServiceCategories, r => r.CategoryId, c => c.CategoryId, (r, c) => c.Type)
                .GroupBy(c => c).Select(g => new { category = g.Key, count = g.Count() }).OrderByDescending(x => x.count).ToListAsync(ct);
            var completed = rows.Count(r => r.Status == "Closed");
            var overdue = rows.Count(r => r.DueAt.HasValue && r.DueAt.Value < now && r.Status is not ("Closed" or "Resolved"));
            var activeStaff = await db.Employees.AsNoTracking().CountAsync(e => e.IsActive && e.Role.Name == Roles.Staff, ct);
            return Results.Ok(new { totalRequests = rows.Count, activeRequests = rows.Count(r => r.Status != "Closed"), completedRequests = completed, overdueRequests = overdue, activeStaff, completionRate = rows.Count == 0 ? 0 : Math.Round(completed * 100d / rows.Count, 1), byStatus, byCategory });
        }).RequireAuthorization(policy => policy.RequireRole(Roles.Admin));
        return endpoints;
    }
}

public sealed record CategoryDto(long CategoryId, string Type);
public sealed record CategoryWriteDto(string? Type, bool IsActive = true);
public sealed record StaffDto(long EmployeeId, string Name, string Email);