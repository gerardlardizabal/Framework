using Framework.Application.Common.Interfaces;
using MediatR;

namespace Framework.Application.Identity.Roles;

public sealed record GetRoleNamesQuery : IRequest<IReadOnlyList<string>>;

public sealed class GetRoleNamesQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetRoleNamesQuery, IReadOnlyList<string>>
{
    public Task<IReadOnlyList<string>> Handle(GetRoleNamesQuery request, CancellationToken cancellationToken)
        => identityService.GetRoleNamesAsync(cancellationToken);
}
