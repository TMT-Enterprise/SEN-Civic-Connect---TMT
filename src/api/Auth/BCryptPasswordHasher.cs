using BCrypt.Net;

namespace CivicConnect.Auth;

public sealed class BCryptPasswordHasher : IPasswordHasher
{
    private const int WorkFactor = 12;

    private static readonly string DummyHash =
        BCrypt.Net.BCrypt.HashPassword(Guid.NewGuid().ToString("N"), WorkFactor);

    public string Hash(string password) => BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);

    public bool Verify(string password, string hash)
    {
        try
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
        catch (Exception ex) when (ex is SaltParseException or ArgumentException)
        {
            // e.g. the DEMO_ONLY_NOT_A_VALID_PASSWORD_HASH placeholder in the seed data.
            return false;
        }
    }

    public void VerifyUnknownAccount(string password)
    {
        _ = Verify(password, DummyHash);
    }
}