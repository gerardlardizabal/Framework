using Framework.Domain.Notifications;

namespace Framework.Application.Notifications;

public sealed record NotificationDto(
    Guid Id,
    string Title,
    string? Message,
    string Kind,
    string? ActionUrl,
    string? ActionText,
    bool IsRead,
    DateTimeOffset Created,
    string RelativeTime);

public sealed record CreateNotificationRequest(
    Guid UserId,
    string Title,
    string? Message = null,
    NotificationKind Kind = NotificationKind.Default,
    string? ActionUrl = null,
    string? ActionText = null);
