using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using MediatR;

namespace Framework.Application.Identity.Users;

public sealed record GetUsersQuery : IRequest<IReadOnlyList<UserListItemDto>>;

public sealed class GetUsersQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUsersQuery, IReadOnlyList<UserListItemDto>>
{
    public Task<IReadOnlyList<UserListItemDto>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
        => identityService.GetUsersAsync(cancellationToken);
}
