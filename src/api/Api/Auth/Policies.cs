namespace CivicConnect.Api;

/// <summary>Authorization policy names for the three roles. Endpoints from later slices attach these.</summary>
public static class Policies
{
    public const string RequesterOnly = nameof(RequesterOnly);
    public const string StaffOnly = nameof(StaffOnly);
    public const string AdminOnly = nameof(AdminOnly);
    public const string AnyEmployee = nameof(AnyEmployee);
}