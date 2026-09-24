using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using MediatR;

namespace Framework.Application.Identity.Roles;

public sealed record GetRoleQuery(Guid Id) : IRequest<RoleDetailsDto?>;

public sealed class GetRoleQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetRoleQuery, RoleDetailsDto?>
{
    public Task<RoleDetailsDto?> Handle(GetRoleQuery request, CancellationToken cancellationToken)
        => identityService.GetRoleAsync(request.Id, cancellationToken);
}
