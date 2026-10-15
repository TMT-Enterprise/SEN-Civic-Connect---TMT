using System.ComponentModel.DataAnnotations;

using CivicConnect.Auth;

namespace CivicConnect.UnitTests;

public sealed class JwtOptionsTests
{
    private static bool IsValid(JwtOptions options) =>
        Validator.TryValidateObject(options, new ValidationContext(options), new List<ValidationResult>(), validateAllProperties: true);

    private static JwtOptions Valid() => new()
    {
        Secret = new string('s', 32),
        Issuer = "issuer",
        Audience = "audience",
        ExpiryMinutes = 60
    };

    [Fact]
    public void A_complete_configuration_is_valid() => Assert.True(IsValid(Valid()));

    [Fact]
    public void A_missing_secret_is_invalid()
    {
        var options = Valid();
        options.Secret = string.Empty;
        Assert.False(IsValid(options));
    }

    [Fact]
    public void A_short_secret_is_invalid()
    {
        var options = Valid();
        options.Secret = new string('s', 31);
        Assert.False(IsValid(options));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(1441)]
    public void An_out_of_range_expiry_is_invalid(int minutes)
    {
        var options = Valid();
        options.ExpiryMinutes = minutes;
        Assert.False(IsValid(options));
    }
}