using System.Reflection;
using System.Text.RegularExpressions;

namespace Framework.Domain.Authorization;

public static partial class Permissions
{
    public static class Dashboard
    {
        public const string View = "Permissions.Dashboard.View";
    }

    public static class Users
    {
        public const string View = "Permissions.Users.View";
        public const string Create = "Permissions.Users.Create";
        public const string Edit = "Permissions.Users.Edit";
        public const string Delete = "Permissions.Users.Delete";
    }

    public static class Roles
    {
        public const string View = "Permissions.Roles.View";
        public const string Create = "Permissions.Roles.Create";
        public const string Edit = "Permissions.Roles.Edit";
        public const string Delete = "Permissions.Roles.Delete";
    }

    public static class Jobs
    {
        public const string View = "Permissions.Jobs.View";
    }

    public static class PermissionTypes
    {
        public const string View = "Permissions.PermissionTypes.View";
        public const string Create = "Permissions.PermissionTypes.Create";
        public const string Edit = "Permissions.PermissionTypes.Edit";
        public const string Delete = "Permissions.PermissionTypes.Delete";
    }

    public static IReadOnlyList<PermissionDefinition> BuiltIn { get; } = BuildBuiltIn();

    public static IReadOnlyList<string> BuiltInNames { get; } = BuiltIn.Select(p => p.Name).ToArray();

    public static bool IsBuiltIn(string permission) =>
        BuiltIn.Any(p => p.Name.Equals(permission, StringComparison.Ordinal));

    public static string CreateName(string group, string action) =>
        $"Permissions.{group.Trim()}.{action.Trim()}";

    public static bool IsValidToken(string value) => TokenRegex().IsMatch(value);

    [GeneratedRegex("^[A-Za-z][A-Za-z0-9]*$")]
    private static partial Regex TokenRegex();

    private static IReadOnlyList<PermissionDefinition> BuildBuiltIn()
    {
        var permissions = new List<PermissionDefinition>();

        foreach (var group in typeof(Permissions).GetNestedTypes())
        {
            foreach (var field in group.GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.FlattenHierarchy)
                         .Where(f => f is { IsLiteral: true, IsInitOnly: false } && f.FieldType == typeof(string)))
            {
                var value = (string)field.GetValue(null)!;
                permissions.Add(new PermissionDefinition(group.Name, value, field.Name));
            }
        }

        return permissions;
    }
}
