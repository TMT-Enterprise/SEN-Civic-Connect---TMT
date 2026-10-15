namespace CivicConnect.Data;

public sealed record AccountRecord(
    long Id,
    string AccountType,
    string Name,
    string Email,
    string PasswordHash,
    string Role,
    bool IsActive);

public sealed class DuplicateEmailException : Exception
{
    public DuplicateEmailException() : base("An account with this email already exists.")
    {
    }

    public DuplicateEmailException(Exception innerException)
        : base("An account with this email already exists.", innerException)
    {
    }
}

public interface IAccountRepository
{
    Task<AccountRecord?> FindByEmailAsync(string email, CancellationToken cancellationToken);

    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken);

    Task<AccountRecord> CreateRequesterAsync(
        string name,
        string email,
        string passwordHash,
        CancellationToken cancellationToken);
}