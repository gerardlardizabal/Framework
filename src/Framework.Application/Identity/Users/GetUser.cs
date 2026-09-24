using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using MediatR;

namespace Framework.Application.Identity.Users;

public sealed record GetUserQuery(Guid Id) : IRequest<UserDetailsDto?>;

public sealed class GetUserQueryHandler(IIdentityService identityService)
    : IRequestHandler<GetUserQuery, UserDetailsDto?>
{
    public Task<UserDetailsDto?> Handle(GetUserQuery request, CancellationToken cancellationToken)
        => identityService.GetUserAsync(request.Id, cancellationToken);
}
