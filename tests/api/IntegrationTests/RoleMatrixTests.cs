using System.Security.Claims;

using CivicConnect.Api;
using CivicConnect.Data;

using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace CivicConnect.IntegrationTests;

public sealed class RoleMatrixTests
{
    private static readonly IAuthorizationService Authz = new ServiceCollection()
        .AddLogging()
        .AddApiAuthentication()
        .BuildServiceProvider()
        .GetRequiredService<IAuthorizationService>();

    private static ClaimsPrincipal PrincipalFor(string? role) =>
        role is null
            ? new ClaimsPrincipal(new ClaimsIdentity()) // anonymous
            : new ClaimsPrincipal(new ClaimsIdentity(
                [new Claim("sub", "1"), new Claim("role", role)], "test", "sub", "role"));

    // Basis: NF-002 (role-based access), ASR-1. One row per cell of the role x policy decision table.
    [Theory(DisplayName = "S2-04 / NF-002 / ASR-1: role x policy decision table")]
    // RequesterOnly
    [InlineData(Roles.Requester, Policies.RequesterOnly, true)]
    [InlineData(Roles.Staff, Policies.RequesterOnly, false)]
    [InlineData(Roles.Admin, Policies.RequesterOnly, false)]
    [InlineData(null, Policies.RequesterOnly, false)]
    // StaffOnly
    [InlineData(Roles.Requester, Policies.StaffOnly, false)]
    [InlineData(Roles.Staff, Policies.StaffOnly, true)]
    [InlineData(Roles.Admin, Policies.StaffOnly, false)]
    [InlineData(null, Policies.StaffOnly, false)]
    // AdminOnly (combined Admin/Management role)
    [InlineData(Roles.Requester, Policies.AdminOnly, false)]
    [InlineData(Roles.Staff, Policies.AdminOnly, false)]
    [InlineData(Roles.Admin, Policies.AdminOnly, true)]
    [InlineData(null, Policies.AdminOnly, false)]
    // AnyEmployee
    [InlineData(Roles.Requester, Policies.AnyEmployee, false)]
    [InlineData(Roles.Staff, Policies.AnyEmployee, true)]
    [InlineData(Roles.Admin, Policies.AnyEmployee, true)]
    [InlineData(null, Policies.AnyEmployee, false)]
    public async Task Role_policy_decision_table(string? role, string policy, bool expected)
    {
        var result = await Authz.AuthorizeAsync(PrincipalFor(role), null, policy);

        Assert.Equal(expected, result.Succeeded);
    }
}