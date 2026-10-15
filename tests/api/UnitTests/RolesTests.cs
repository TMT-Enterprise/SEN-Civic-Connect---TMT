using CivicConnect.Data;

namespace CivicConnect.UnitTests;

public sealed class RolesTests
{
    [Fact]
    public void There_are_exactly_three_roles()
    {
        var all = new[] { Roles.Requester, Roles.Staff, Roles.Admin };

        Assert.Equal(3, all.Distinct(StringComparer.Ordinal).Count());
        Assert.Equal(new[] { Roles.Staff, Roles.Admin }, Roles.Employee);
    }
}