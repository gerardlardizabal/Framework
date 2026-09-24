using Framework.Application.Common.Interfaces;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Identity.PermissionCatalog;

public sealed record DeletePermissionCommand(Guid Id) : IRequest<Result>;

public sealed class DeletePermissionCommandHandler(IPermissionService permissions)
    : IRequestHandler<DeletePermissionCommand, Result>
{
    public Task<Result> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
        => permissions.DeletePermissionAsync(request.Id, cancellationToken);
}
