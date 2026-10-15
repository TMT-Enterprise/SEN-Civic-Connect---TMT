namespace CivicConnect.Auth;

public interface IPasswordHasher
{
    string Hash(string password);

    /// <summary>Returns false (never throws) for a wrong password or a stored value that is not a valid hash.</summary>
    bool Verify(string password, string hash);

    /// <summary>Spends the same time as a real verification so unknown emails cannot be told apart by response time.</summary>
    void VerifyUnknownAccount(string password);
}