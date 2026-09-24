using Framework.Application.Identity;
using Framework.Domain.Common;

namespace Framework.Application.Common.Interfaces;

public interface IIdentityService
{
    Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken cancellationToken = default);
    Task<UserDetailsDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Result> SetUserRolesAsync(Guid userId, IReadOnlyCollection<string> roleNames, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<RoleListItemDto>> GetRolesAsync(CancellationToken cancellationToken = default);
    Task<RoleDetailsDto?> GetRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Result<Guid>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdateRoleAsync(UpdateRoleRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default);
    Task<Result> SetRolePermissionsAsync(Guid roleId, IReadOnlyCollection<string> permissions, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetRoleNamesAsync(CancellationToken cancellationToken = default);
}
