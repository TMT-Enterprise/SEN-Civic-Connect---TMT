namespace CivicConnect.Auth;

// Request records have no Role property on purpose: the client can never choose its own role.
public sealed record RegisterRequest(string? Name, string? Email, string? Password);

public sealed record LoginRequest(string? Email, string? Password);

public sealed record RegisteredAccount(long Id, string Name, string Email, string Role);

public sealed record LoginSuccess(string AccessToken, string TokenType, DateTimeOffset ExpiresAt, string Role);

public enum RegisterStatus
{
    Created,
    Invalid,
    DuplicateEmail
}

public sealed record RegisterResult(
    RegisterStatus Status,
    RegisteredAccount? Account = null,
    IDictionary<string, string[]>? Errors = null);