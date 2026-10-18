using CivicConnect.Data;

namespace CivicConnect.UnitTests;

/// <summary>In-memory stand-in for the repository so AuthService rules can be tested without a database.</summary>
internal sealed class FakeAccountRepository : IAccountRepository
{
    private long _nextId = 1;

    public List<AccountRecord> Accounts { get; } = [];

    /// <summary>Simulates a concurrent registration winning between the pre-check and the INSERT.</summary>
    public bool FailCreateWithDuplicate { get; set; }

    public Task<AccountRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.FirstOrDefault(a => a.Email.ToLowerInvariant() == email));

    public Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken) =>
        Task.FromResult(Accounts.Any(a => a.Email.ToLowerInvariant() == email));

    public Task<AccountRecord> CreateRequesterAsync(
        string name, string email, string passwordHash, CancellationToken cancellationToken)
    {
        if (FailCreateWithDuplicate || Accounts.Any(a => a.Email.ToLowerInvariant() == email))
        {
            throw new DuplicateEmailException();
        }

        var account = new AccountRecord(_nextId++, AccountTypes.Requester, name, email, passwordHash, Roles.Requester, true);
        Accounts.Add(account);
        return Task.FromResult(account);
    }
}