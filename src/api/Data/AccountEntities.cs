namespace CivicConnect.Data;

public sealed class EmployeeRole
{
    public long RoleId { get; set; }

    public string Name { get; set; } = string.Empty;
}

public sealed class Requester
{
    public long RequesterId { get; set; }

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class Employee
{
    public long EmployeeId { get; set; }

    public long RoleId { get; set; }

    public EmployeeRole Role { get; set; } = null!;

    public string Name { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;
}