using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using MediatR;

namespace Framework.Application.Identity.PermissionCatalog;

public sealed record GetPermissionsQuery : IRequest<IReadOnlyList<PermissionListItemDto>>;

public sealed class GetPermissionsQueryHandler(IPermissionService permissions)
    : IRequestHandler<GetPermissionsQuery, IReadOnlyList<PermissionListItemDto>>
{
    public Task<IReadOnlyList<PermissionListItemDto>> Handle(GetPermissionsQuery request, CancellationToken cancellationToken)
        => permissions.GetPermissionsAsync(cancellationToken);
}
