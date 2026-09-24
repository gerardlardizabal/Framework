using Framework.Application.Notifications;
using Framework.Domain.Common;

namespace Framework.Application.Common.Interfaces;

public interface INotificationService
{
    Task<Result<Guid>> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<NotificationDto>> GetRecentAsync(Guid userId, int take, CancellationToken cancellationToken = default);

    Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default);

    Task<Result> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default);

    Task<Result> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default);
}
