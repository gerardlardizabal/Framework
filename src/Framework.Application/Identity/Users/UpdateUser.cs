using FluentValidation;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Identity.Users;

public sealed record UpdateUserCommand(
    Guid Id,
    string? FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyList<string> Roles) : IRequest<Result>;

public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
{
    public UpdateUserCommandValidator()
    {
        RuleFor(v => v.Id).NotEmpty();
        RuleFor(v => v.FirstName).MaximumLength(100);
        RuleFor(v => v.LastName).MaximumLength(100);
    }
}

public sealed class UpdateUserCommandHandler(IIdentityService identityService)
    : IRequestHandler<UpdateUserCommand, Result>
{
    public Task<Result> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        => identityService.UpdateUserAsync(
            new UpdateUserRequest(
                request.Id,
                request.FirstName,
                request.LastName,
                request.IsActive,
                request.Roles),
            cancellationToken);
}
