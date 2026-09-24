using Framework.Application.Common.Interfaces;
using MediatR;

namespace Framework.Application.Notifications;

public sealed record GetNotificationsQuery(int Take = 20) : IRequest<IReadOnlyList<NotificationDto>>;

public sealed class GetNotificationsQueryHandler(INotificationService notifications, ICurrentUser currentUser)
    : IRequestHandler<GetNotificationsQuery, IReadOnlyList<NotificationDto>>
{
    public async Task<IReadOnlyList<NotificationDto>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
    {
        if (currentUser.Id is not Guid userId)
        {
            return [];
        }

        var take = Math.Clamp(request.Take, 1, 100);
        return await notifications.GetRecentAsync(userId, take, cancellationToken);
    }
}
