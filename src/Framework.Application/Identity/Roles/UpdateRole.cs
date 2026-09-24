using FluentValidation;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Identity.Roles;

public sealed record UpdateRoleCommand(Guid Id, string Name, string? Description, IReadOnlyList<string> Permissions)
    : IRequest<Result>;

public sealed class UpdateRoleCommandValidator : AbstractValidator<UpdateRoleCommand>
{
    public UpdateRoleCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Name).NotEmpty().MaximumLength(256);
        RuleFor(v => v.Description).MaximumLength(512);
    }
}

public sealed class UpdateRoleCommandHandler(IIdentityService identityService) : IRequestHandler<UpdateRoleCommand, Result>
{
    public async Task<Result> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
    {
        var update = await identityService.UpdateRoleAsync(
            new UpdateRoleRequest(request.Id, request.Name, request.Description),
            cancellationToken);

        if (!update.Succeeded)
        {
            return update;
        }

        return await identityService.SetRolePermissionsAsync(request.Id, request.Permissions, cancellationToken);
    }
}
