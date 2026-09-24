using FluentValidation;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Common;
using MediatR;
using KnownPermissions = Framework.Domain.Authorization.Permissions;

namespace Framework.Application.Identity.PermissionCatalog;

public sealed record CreatePermissionCommand(string Group, string Action, string? DisplayName, string? Description)
    : IRequest<Result<Guid>>;

public sealed class CreatePermissionCommandValidator : AbstractValidator<CreatePermissionCommand>
{
    public CreatePermissionCommandValidator()
    {
        RuleFor(v => v.Group)
            .NotEmpty()
            .MaximumLength(64)
            .Must(KnownPermissions.IsValidToken)
            .WithMessage("Group must start with a letter and contain only letters and numbers.");

        RuleFor(v => v.Action)
            .NotEmpty()
            .MaximumLength(64)
            .Must(KnownPermissions.IsValidToken)
            .WithMessage("Action must start with a letter and contain only letters and numbers.");

        RuleFor(v => v.DisplayName).MaximumLength(128);
        RuleFor(v => v.Description).MaximumLength(512);
    }
}

public sealed class CreatePermissionCommandHandler(IPermissionService permissions)
    : IRequestHandler<CreatePermissionCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreatePermissionCommand request, CancellationToken cancellationToken)
        => permissions.CreatePermissionAsync(
            new CreatePermissionRequest(request.Group, request.Action, request.DisplayName, request.Description),
            cancellationToken);
}
