using FluentValidation;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Common;
using MediatR;
using KnownPermissions = Framework.Domain.Authorization.Permissions;

namespace Framework.Application.Identity.PermissionCatalog;

public sealed record UpdatePermissionCommand(Guid Id, string Group, string DisplayName, string? Description)
    : IRequest<Result>;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    public UpdatePermissionCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.Group)
            .NotEmpty()
            .MaximumLength(64)
            .Must(KnownPermissions.IsValidToken)
            .WithMessage("Group must start with a letter and contain only letters and numbers.");
        RuleFor(v => v.DisplayName).NotEmpty().MaximumLength(128);
        RuleFor(v => v.Description).MaximumLength(512);
    }
}

public sealed class UpdatePermissionCommandHandler(IPermissionService permissions)
    : IRequestHandler<UpdatePermissionCommand, Result>
{
    public Task<Result> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
        => permissions.UpdatePermissionAsync(
            new UpdatePermissionRequest(request.Id, request.Group, request.DisplayName, request.Description),
            cancellationToken);
}
