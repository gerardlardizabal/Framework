namespace Framework.Application.Identity;

public sealed record UserListItemDto(
    Guid Id,
    string Email,
    string? DisplayName,
    bool EmailConfirmed,
    bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record UserDetailsDto(
    Guid Id,
    string Email,
    string? UserName,
    string? FirstName,
    string? LastName,
    bool EmailConfirmed,
    bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record CreateUserRequest(
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record UpdateUserRequest(
    Guid Id,
    string? FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyList<string> Roles);

public sealed record RoleListItemDto(
    Guid Id,
    string Name,
    string? Description,
    int UserCount,
    int PermissionCount);

public sealed record RoleDetailsDto(
    Guid Id,
    string Name,
    string? Description,
    IReadOnlyList<string> Permissions);

public sealed record CreateRoleRequest(string Name, string? Description);

public sealed record UpdateRoleRequest(Guid Id, string Name, string? Description);
