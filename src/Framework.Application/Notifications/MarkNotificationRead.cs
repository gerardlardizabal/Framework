using Framework.Application.Common.Interfaces;
using Framework.Domain.Common;
using MediatR;

namespace Framework.Application.Notifications;

public sealed record MarkNotificationReadCommand(Guid Id) : IRequest<Result>;

public sealed class MarkNotificationReadCommandHandler(INotificationService notifications, ICurrentUser currentUser)
    : IRequestHandler<MarkNotificationReadCommand, Result>
{
    public Task<Result> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        => currentUser.Id is Guid userId
            ? notifications.MarkReadAsync(userId, request.Id, cancellationToken)
            : Task.FromResult(Result.Failure("You must be signed in."));
}
