using FluentValidation;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Identity.Roles;

public sealed record CreateRoleCommand(string Name, string? Description) : IRequest<Result<Guid>>;

public sealed class CreateRoleCommandValidator : AbstractValidator<CreateRoleCommand>
{
    public CreateRoleCommandValidator()
    {
        RuleFor(v => v.Name).NotEmpty().MaximumLength(256);
        RuleFor(v => v.Description).MaximumLength(512);
    }
}

public sealed class CreateRoleCommandHandler(IIdentityService identityService)
    : IRequestHandler<CreateRoleCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreateRoleCommand request, CancellationToken cancellationToken)
        => identityService.CreateRoleAsync(new CreateRoleRequest(request.Name, request.Description), cancellationToken);
}
