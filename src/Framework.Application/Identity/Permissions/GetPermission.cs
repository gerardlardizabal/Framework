using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using MediatR;

namespace Framework.Application.Identity.PermissionCatalog;

public sealed record GetPermissionQuery(Guid Id) : IRequest<PermissionListItemDto?>;

public sealed class GetPermissionQueryHandler(IPermissionService permissions)
    : IRequestHandler<GetPermissionQuery, PermissionListItemDto?>
{
    public Task<PermissionListItemDto?> Handle(GetPermissionQuery request, CancellationToken cancellationToken)
        => permissions.GetPermissionAsync(request.Id, cancellationToken);
}
