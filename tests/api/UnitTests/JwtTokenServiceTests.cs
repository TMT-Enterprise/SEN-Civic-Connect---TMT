using CivicConnect.Auth;
using CivicConnect.Data;

using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace CivicConnect.UnitTests;

public sealed class JwtTokenServiceTests
{
    private static readonly DateTimeOffset Now = DateTimeOffset.Parse("2026-10-14T10:00:00Z");

    private static JwtOptions Configured(int expiryMinutes = 15) => new()
    {
        Secret = new string('s', 48),
        Issuer = "test-issuer",
        Audience = "test-audience",
        ExpiryMinutes = expiryMinutes
    };

    private static JwtTokenService Service(JwtOptions options) =>
        new(Microsoft.Extensions.Options.Options.Create(options), new FixedTimeProvider(Now));

    [Theory]
    [InlineData(Roles.Requester, AccountTypes.Requester)]
    [InlineData(Roles.Staff, AccountTypes.Employee)]
    [InlineData(Roles.Admin, AccountTypes.Employee)]
    public void Token_carries_exactly_one_role_claim_and_the_account_identity(string role, string accountType)
    {
        var account = new AccountRecord(7, accountType, "Test", "t@example.com", "hash", role, true);

        var token = new JsonWebTokenHandler().ReadJsonWebToken(Service(Configured()).Create(account).Token);

        Assert.Equal(role, Assert.Single(token.Claims, c => c.Type == "role").Value);
        Assert.Equal("7", token.Subject);
        Assert.Equal(accountType, token.GetClaim("acct").Value);
        Assert.Equal("t@example.com", token.GetClaim("email").Value);
        Assert.Equal("test-issuer", token.Issuer);
    }

    [Theory]
    [InlineData(5)]
    [InlineData(60)]
    public void Expiry_comes_from_the_configured_minutes(int minutes)
    {
        var account = new AccountRecord(1, AccountTypes.Requester, "T", "t@example.com", "hash", Roles.Requester, true);

        var issued = Service(Configured(minutes)).Create(account);
        var token = new JsonWebTokenHandler().ReadJsonWebToken(issued.Token);

        Assert.Equal(Now.AddMinutes(minutes), issued.ExpiresAt);
        Assert.Equal(TimeSpan.FromMinutes(minutes), token.ValidTo - token.ValidFrom);
    }

    [Fact]
    public async Task A_token_does_not_validate_against_a_different_secret()
    {
        var account = new AccountRecord(1, AccountTypes.Requester, "T", "t@example.com", "hash", Roles.Requester, true);
        var token = Service(Configured()).Create(account).Token;

        var wrongKey = Configured();
        wrongKey.Secret = new string('x', 48);

        var result = await new JsonWebTokenHandler().ValidateTokenAsync(token, new TokenValidationParameters
        {
            ValidIssuer = "test-issuer",
            ValidAudience = "test-audience",
            IssuerSigningKey = wrongKey.CreateSigningKey(),
            ValidateLifetime = false
        });

        Assert.False(result.IsValid);
    }
}