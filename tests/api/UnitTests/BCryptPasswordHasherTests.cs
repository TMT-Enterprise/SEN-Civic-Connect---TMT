using System.Text.RegularExpressions;

using CivicConnect.Auth;

namespace CivicConnect.UnitTests;

public sealed partial class BCryptPasswordHasherTests
{
    private readonly BCryptPasswordHasher _hasher = new();

    [GeneratedRegex(@"^\$2[aby]\$12\$[./A-Za-z0-9]{53}$")]
    private static partial Regex BcryptFormat();

    [Fact]
    public void Hash_is_not_the_plaintext_and_has_bcrypt_format()
    {
        var hash = _hasher.Hash("Str0ng!Passw0rd");

        Assert.NotEqual("Str0ng!Passw0rd", hash);
        Assert.DoesNotContain("Str0ng!Passw0rd", hash, StringComparison.Ordinal);
        Assert.Matches(BcryptFormat(), hash);
    }

    [Fact]
    public void Hashing_the_same_password_twice_gives_different_hashes()
    {
        Assert.NotEqual(_hasher.Hash("Str0ng!Passw0rd"), _hasher.Hash("Str0ng!Passw0rd"));
    }

    [Fact]
    public void Verify_accepts_the_right_password_and_rejects_a_wrong_one()
    {
        var hash = _hasher.Hash("Str0ng!Passw0rd");

        Assert.True(_hasher.Verify("Str0ng!Passw0rd", hash));
        Assert.False(_hasher.Verify("wrong-password", hash));
    }

    [Fact]
    public void Verify_returns_false_for_a_stored_value_that_is_not_a_hash()
    {
        // The seed file stores this placeholder for demo accounts; it must fail the login, not crash it.
        Assert.False(_hasher.Verify("anything", "DEMO_ONLY_NOT_A_VALID_PASSWORD_HASH"));
    }
}