using CivicConnect.Data;

namespace CivicConnect.Auth;

public sealed record AccessToken(string Token, DateTimeOffset ExpiresAt);

public interface ITokenService
{
    AccessToken Create(AccountRecord account);
}