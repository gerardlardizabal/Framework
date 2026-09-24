using Framework.Application.Common.Interfaces;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Identity.Roles;

public sealed record DeleteRoleCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteRoleCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteRoleCommand, Result>
{
    public Task<Result> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        => identityService.DeleteRoleAsync(request.Id, cancellationToken);
}
