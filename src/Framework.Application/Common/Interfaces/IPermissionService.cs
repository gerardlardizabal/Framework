using Framework.Application.Identity;
using Framework.Domain.Common;

namespace Framework.Application.Common.Interfaces;

public interface IPermissionService
{
    Task<IReadOnlyList<PermissionListItemDto>> GetPermissionsAsync(CancellationToken cancellationToken = default);

    Task<PermissionListItemDto?> GetPermissionAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Result<Guid>> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken cancellationToken = default);

    Task<Result> UpdatePermissionAsync(UpdatePermissionRequest request, CancellationToken cancellationToken = default);

    Task<Result> DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default);
}
