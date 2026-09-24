using Framework.Application.Common.Interfaces;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Identity.Users;

public sealed record DeleteUserCommand(Guid Id) : IRequest<Result>;

public sealed class DeleteUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<DeleteUserCommand, Result>
{
    public Task<Result> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        => identityService.DeleteUserAsync(request.Id, cancellationToken);
}
