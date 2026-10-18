using CivicConnect.Auth;
using CivicConnect.Data;

using Microsoft.Extensions.Options;

namespace CivicConnect.UnitTests;

public sealed class AuthServiceTests
{
    private const string Password = "Str0ng!Passw0rd";

    private readonly FakeAccountRepository _accounts = new();
    private readonly BCryptPasswordHasher _hasher = new();
    private readonly AuthService _service;

    public AuthServiceTests()
    {
        var options = Options.Create(new JwtOptions
        {
            Secret = new string('s', 48),
            Issuer = "test-issuer",
            Audience = "test-audience",
            ExpiryMinutes = 5
        });

        _service = new AuthService(
            _accounts,
            _hasher,
            new JwtTokenService(options, new FixedTimeProvider(DateTimeOffset.Parse("2026-10-14T10:00:00Z"))));
    }

    [Fact]
    public async Task Register_creates_a_requester_with_a_hashed_password()
    {
        var result = await _service.RegisterAsync(new RegisterRequest("Ada Lovelace", "  Ada@Example.com ", Password), default);

        Assert.Equal(RegisterStatus.Created, result.Status);
        Assert.Equal(Roles.Requester, result.Account!.Role);

        var stored = Assert.Single(_accounts.Accounts);
        Assert.Equal("ada@example.com", stored.Email);
        Assert.NotEqual(Password, stored.PasswordHash);
        Assert.True(_hasher.Verify(Password, stored.PasswordHash));
    }

    [Theory]
    [InlineData("ada@example.com")]
    [InlineData("ADA@EXAMPLE.COM")]
    [InlineData("  Ada@Example.com  ")]
    public async Task Register_rejects_a_duplicate_email_in_any_casing(string secondEmail)
    {
        await _service.RegisterAsync(new RegisterRequest("Ada", "ada@example.com", Password), default);

        var result = await _service.RegisterAsync(new RegisterRequest("Other", secondEmail, Password), default);

        Assert.Equal(RegisterStatus.DuplicateEmail, result.Status);
        Assert.Single(_accounts.Accounts);
    }

    [Fact]
    public async Task Register_reports_a_duplicate_when_the_database_index_wins_a_race()
    {
        _accounts.FailCreateWithDuplicate = true;

        var result = await _service.RegisterAsync(new RegisterRequest("Ada", "ada@example.com", Password), default);

        Assert.Equal(RegisterStatus.DuplicateEmail, result.Status);
    }

    [Theory]
    [InlineData(null, "ada@example.com", Password, "name")]
    [InlineData("  ", "ada@example.com", Password, "name")]
    [InlineData("Ada", "not-an-email", Password, "email")]
    [InlineData("Ada", null, Password, "email")]
    [InlineData("Ada", "ada@example.com", "short", "password")]
    [InlineData("Ada", "ada@example.com", null, "password")]
    public async Task Register_rejects_invalid_input_with_field_errors(string? name, string? email, string? password, string field)
    {
        var result = await _service.RegisterAsync(new RegisterRequest(name, email, password), default);

        Assert.Equal(RegisterStatus.Invalid, result.Status);
        Assert.Contains(field, result.Errors!.Keys);
        Assert.Empty(_accounts.Accounts);
    }

    [Fact]
    public async Task Register_rejects_a_password_longer_than_bcrypt_can_use()
    {
        var result = await _service.RegisterAsync(
            new RegisterRequest("Ada", "ada@example.com", new string('a', AuthService.MaxPasswordBytes + 1)), default);

        Assert.Equal(RegisterStatus.Invalid, result.Status);
        Assert.Contains("password", result.Errors!.Keys);
    }

    [Fact]
    public async Task Register_rejects_a_phone_number_longer_than_the_database_column()
    {
        var result = await _service.RegisterAsync(
            new RegisterRequest("Ada", "ada@example.com", Password, new string('1', 31)), default);

        Assert.Equal(RegisterStatus.Invalid, result.Status);
        Assert.Contains("phoneNumber", result.Errors!.Keys);
        Assert.Empty(_accounts.Accounts);
    }

    [Fact]
    public async Task Login_returns_a_bearer_token_for_valid_credentials()
    {
        await _service.RegisterAsync(new RegisterRequest("Ada", "ada@example.com", Password), default);

        var login = await _service.LoginAsync(new LoginRequest("ADA@example.com", Password), default);

        Assert.NotNull(login);
        Assert.Equal("Bearer", login.TokenType);
        Assert.Equal(Roles.Requester, login.Role);
        Assert.False(string.IsNullOrWhiteSpace(login.AccessToken));
        Assert.Single(_accounts.SuccessfulLogins);
    }

    [Fact]
    public async Task Login_fails_the_same_way_for_wrong_password_unknown_email_and_inactive_account()
    {
        await _service.RegisterAsync(new RegisterRequest("Ada", "ada@example.com", Password), default);
        var active = _accounts.Accounts.Single();
        _accounts.Accounts.Add(active with { Id = 99, Email = "inactive@example.com", IsActive = false });

        Assert.Null(await _service.LoginAsync(new LoginRequest("ada@example.com", "wrong-password"), default));
        Assert.Null(await _service.LoginAsync(new LoginRequest("nobody@example.com", Password), default));
        Assert.Null(await _service.LoginAsync(new LoginRequest("inactive@example.com", Password), default));
        Assert.Null(await _service.LoginAsync(new LoginRequest(null, null), default));
        Assert.Empty(_accounts.SuccessfulLogins);
    }

    [Fact]
    public async Task Login_fails_instead_of_throwing_when_the_stored_value_is_not_a_hash()
    {
        _accounts.Accounts.Add(new AccountRecord(
            1, AccountTypes.Employee, "Demo", "demo@example.com", "DEMO_ONLY_NOT_A_VALID_PASSWORD_HASH", Roles.Staff, true));

        Assert.Null(await _service.LoginAsync(new LoginRequest("demo@example.com", Password), default));
    }
}