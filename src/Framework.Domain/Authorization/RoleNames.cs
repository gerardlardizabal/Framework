namespace Framework.Domain.Authorization;

public static class RoleNames
{
    public const string Administrator = "Administrator";
    public const string User = "User";

    public static IReadOnlyList<string> All { get; } = [Administrator, User];
}
