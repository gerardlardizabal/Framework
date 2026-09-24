using FluentValidation;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Application.Notifications;
using Framework.Domain.Common;
using Framework.Domain.Notifications;
using MediatR;

namespace Framework.Application.Identity.Users;

public sealed record CreateUserCommand(
    string Email,
    string Password,
    string? FirstName,
    string? LastName,
    bool IsActive,
    IReadOnlyList<string> Roles) : IRequest<Result<Guid>>;

public sealed class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(v => v.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(v => v.Password).NotEmpty().MinimumLength(8).MaximumLength(100);
        RuleFor(v => v.FirstName).MaximumLength(100);
        RuleFor(v => v.LastName).MaximumLength(100);
    }
}

public sealed class CreateUserCommandHandler(
    IIdentityService identityService,
    INotificationService notifications,
    ICurrentUser currentUser)
    : IRequestHandler<CreateUserCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
    {
        var result = await identityService.CreateUserAsync(
            new CreateUserRequest(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.IsActive,
                request.Roles),
            cancellationToken);

        if (result.Succeeded && currentUser.Id is Guid actorId)
        {
            await notifications.CreateAsync(
                new CreateNotificationRequest(
                    actorId,
                    "New user created",
                    $"{request.Email} can now sign in.",
                    NotificationKind.Success,
                    result.Value is Guid id ? $"admin/users/{id}" : "admin/users",
                    "View user"),
                cancellationToken);
        }

        return result;
    }
}
