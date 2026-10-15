using System.ComponentModel.DataAnnotations;
using System.Text;

using Microsoft.IdentityModel.Tokens;

namespace CivicConnect.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    [Required]
    [MinLength(32)]
    public string Secret { get; set; } = string.Empty;

    [Required]
    public string Issuer { get; set; } = string.Empty;

    [Required]
    public string Audience { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int ExpiryMinutes { get; set; } = 60;

    public SymmetricSecurityKey CreateSigningKey() => new(Encoding.UTF8.GetBytes(Secret));
}