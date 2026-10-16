namespace CivicConnect.Data;

public static class Roles
{
    public const string Requester = "Requester";
    public const string Staff = "Staff";
    public const string Admin = "Admin";

    public static readonly string[] Employee = [Staff, Admin];
}

public static class AccountTypes
{
    public const string Requester = "requester";
    public const string Employee = "employee";
}