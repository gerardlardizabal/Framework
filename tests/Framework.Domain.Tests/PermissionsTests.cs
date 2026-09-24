using Framework.Domain.Authorization;
using Shouldly;

namespace Framework.Domain.Tests;

public class PermissionsTests
{
    [Fact]
    public void Built_in_catalog_contains_unique_permission_names()
    {
        Permissions.BuiltInNames.ShouldBe(Permissions.BuiltInNames.Distinct(), ignoreOrder: true);
        Permissions.BuiltIn.Count.ShouldBeGreaterThan(0);
    }

    [Fact]
    public void Known_permissions_are_built_in()
    {
        Permissions.IsBuiltIn(Permissions.Users.View).ShouldBeTrue();
        Permissions.IsBuiltIn(Permissions.Jobs.View).ShouldBeTrue();
        Permissions.IsBuiltIn(Permissions.PermissionTypes.View).ShouldBeTrue();
        Permissions.IsBuiltIn("Permissions.Unknown.Thing").ShouldBeFalse();
    }

    [Fact]
    public void Groups_are_derived_from_nested_types()
    {
        Permissions.BuiltIn.Select(p => p.Group).ShouldContain("Users");
        Permissions.BuiltIn.Select(p => p.Group).ShouldContain("Roles");
        Permissions.BuiltIn.Select(p => p.Group).ShouldContain("Dashboard");
        Permissions.BuiltIn.Select(p => p.Group).ShouldContain("Jobs");
        Permissions.BuiltIn.Select(p => p.Group).ShouldContain("PermissionTypes");
    }

    [Fact]
    public void CreateName_uses_group_and_action()
    {
        Permissions.CreateName("Reports", "View").ShouldBe("Permissions.Reports.View");
        Permissions.IsValidToken("Reports").ShouldBeTrue();
        Permissions.IsValidToken("ViewAll").ShouldBeTrue();
        Permissions.IsValidToken("view-all").ShouldBeFalse();
        Permissions.IsValidToken("1Reports").ShouldBeFalse();
    }
}
