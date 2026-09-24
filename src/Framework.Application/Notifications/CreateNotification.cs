using FluentValidation;
using Framework.Application.Common.Interfaces;
using Framework.Domain.Common;
using Framework.Domain.Notifications;
using MediatR;

namespace Framework.Application.Notifications;

public sealed record CreateNotificationCommand(
    Guid UserId,
    string Title,
    string? Message,
    NotificationKind Kind = NotificationKind.Default,
    string? ActionUrl = null,
    string? ActionText = null) : IRequest<Result<Guid>>;

public sealed class CreateNotificationCommandValidator : AbstractValidator<CreateNotificationCommand>
{
    public CreateNotificationCommandValidator()
    {
        RuleFor(v => v.UserId).NotEmpty();
        RuleFor(v => v.Title).NotEmpty().MaximumLength(160);
        RuleFor(v => v.Message).MaximumLength(512);
        RuleFor(v => v.ActionUrl).MaximumLength(512);
        RuleFor(v => v.ActionText).MaximumLength(64);
        RuleFor(v => v.Kind).IsInEnum();
    }
}

public sealed class CreateNotificationCommandHandler(INotificationService notifications)
    : IRequestHandler<CreateNotificationCommand, Result<Guid>>
{
    public Task<Result<Guid>> Handle(CreateNotificationCommand request, CancellationToken cancellationToken)
        => notifications.CreateAsync(
            new CreateNotificationRequest(
                request.UserId,
                request.Title,
                request.Message,
                request.Kind,
                request.ActionUrl,
                request.ActionText),
            cancellationToken);
}
