namespace Framework.Application.Identity;

public sealed record PermissionListItemDto(
    Guid Id,
    string Group,
    string Name,
    string DisplayName,
    string? Description,
    bool IsSystem);

public sealed record CreatePermissionRequest(
    string Group,
    string Action,
    string? DisplayName,
    string? Description);

public sealed record UpdatePermissionRequest(
    Guid Id,
    string Group,
    string DisplayName,
    string? Description);
