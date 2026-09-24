using Framework.Application.Common.Interfaces;
using MediatR;

namespace Framework.Application.Notifications;

public sealed record GetUnreadNotificationCountQuery : IRequest<int>;

public sealed class GetUnreadNotificationCountQueryHandler(INotificationService notifications, ICurrentUser currentUser)
    : IRequestHandler<GetUnreadNotificationCountQuery, int>
{
    public Task<int> Handle(GetUnreadNotificationCountQuery request, CancellationToken cancellationToken)
        => currentUser.Id is Guid userId
            ? notifications.GetUnreadCountAsync(userId, cancellationToken)
            : Task.FromResult(0);
}
