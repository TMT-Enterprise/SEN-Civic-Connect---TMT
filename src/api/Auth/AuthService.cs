using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;

using CivicConnect.Data;

namespace CivicConnect.Auth;

public sealed class AuthService(IAccountRepository accounts, IPasswordHasher hasher, ITokenService tokens)
{
    public const int MinPasswordLength = 8;
    public const int MaxPasswordBytes = 72; // bcrypt ignores anything beyond 72 bytes
    private const int MaxNameLength = 150;
    private const int MaxEmailLength = 254;

    private static readonly EmailAddressAttribute EmailValidator = new();

    public async Task<RegisterResult> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken)
    {
        var errors = Validate(request);
        if (errors.Count > 0)
        {
            return new RegisterResult(RegisterStatus.Invalid, Errors: errors);
        }

        var name = request.Name!.Trim();
        var email = NormaliseEmail(request.Email!);

        if (await accounts.EmailExistsAsync(email, cancellationToken))
        {
            return new RegisterResult(RegisterStatus.DuplicateEmail);
        }

        var hash = hasher.Hash(request.Password!);

        try
        {
            var phoneNumber = string.IsNullOrWhiteSpace(request.PhoneNumber) ? null : request.PhoneNumber.Trim();
            var account = await accounts.CreateRequesterAsync(name, email, hash, phoneNumber, cancellationToken);
            return new RegisterResult(
                RegisterStatus.Created,
                new RegisteredAccount(account.Id, account.Name, account.Email, account.Role));
        }
        catch (DuplicateEmailException)
        {
            return new RegisterResult(RegisterStatus.DuplicateEmail);
        }
    }

    /// <summary>Returns null for every kind of failure so callers cannot tell which one occurred.</summary>
    public async Task<LoginSuccess?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        var password = request.Password ?? string.Empty;

        if (string.IsNullOrWhiteSpace(request.Email) || password.Length == 0)
        {
            hasher.VerifyUnknownAccount(password);
            return null;
        }

        var account = await accounts.FindByEmailAsync(NormaliseEmail(request.Email), cancellationToken);
        if (account is null)
        {
            hasher.VerifyUnknownAccount(password);
            return null;
        }

        var passwordMatches = hasher.Verify(password, account.PasswordHash);
        if (!passwordMatches || !account.IsActive)
        {
            return null;
        }

        await accounts.UpdateLastLoginAsync(account, cancellationToken);

        var token = tokens.Create(account);

        return new LoginSuccess(
            token.Token,
            "Bearer",
            token.ExpiresAt,
            account.Role,
            account.Name
        );
    }

    public static string NormaliseEmail(string email) => email.Trim().ToLower(CultureInfo.InvariantCulture);

    private static Dictionary<string, string[]> Validate(RegisterRequest request)
    {
        var errors = new Dictionary<string, string[]>();

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            errors["name"] = ["Name is required."];
        }
        else if (request.Name.Trim().Length > MaxNameLength)
        {
            errors["name"] = [$"Name must be at most {MaxNameLength} characters."];
        }

        if (string.IsNullOrWhiteSpace(request.Email)
            || request.Email.Trim().Length > MaxEmailLength
            || !EmailValidator.IsValid(request.Email.Trim()))
        {
            errors["email"] = ["A valid email address is required."];
        }

        if (string.IsNullOrEmpty(request.Password) || request.Password.Length < MinPasswordLength)
        {
            errors["password"] = [$"Password must be at least {MinPasswordLength} characters."];
        }
        else if (Encoding.UTF8.GetByteCount(request.Password) > MaxPasswordBytes)
        {
            errors["password"] = [$"Password must be at most {MaxPasswordBytes} bytes."];
        }

        if (request.PhoneNumber?.Trim().Length > 30)
        {
            errors["phoneNumber"] = ["Phone number must be at most 30 characters."];
        }

        return errors;
    }
}