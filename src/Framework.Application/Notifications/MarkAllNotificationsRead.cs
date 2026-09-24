using Framework.Application.Common.Interfaces;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Notifications;

public sealed record MarkAllNotificationsReadCommand : IRequest<Result>;

public sealed class MarkAllNotificationsReadCommandHandler(INotificationService notifications, ICurrentUser currentUser)
    : IRequestHandler<MarkAllNotificationsReadCommand, Result>
{
    public Task<Result> Handle(MarkAllNotificationsReadCommand request, CancellationToken cancellationToken)
        => currentUser.Id is Guid userId
            ? notifications.MarkAllReadAsync(userId, cancellationToken)
            : Task.FromResult(Result.Failure("You must be signed in."));
}
