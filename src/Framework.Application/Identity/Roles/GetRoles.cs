using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using MediatR;

namespace Framework.Application.Identity.Roles;

public sealed record GetRolesQuery : IRequest<IReadOnlyList<RoleListItemDto>>;

public sealed class GetRolesQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetRolesQuery, IReadOnlyList<RoleListItemDto>>
{
    public Task<IReadOnlyList<RoleListItemDto>> Handle(GetRolesQuery request, CancellationToken cancellationToken)
        => identityService.GetRolesAsync(cancellationToken);
}
