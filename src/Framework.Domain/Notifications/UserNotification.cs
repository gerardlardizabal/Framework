using Framework.Domain.Common;

namespace Framework.Domain.Notifications;

public sealed class UserNotification : AuditableEntity
{
    public Guid UserId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string? Message { get; set; }

    public NotificationKind Kind { get; set; } = NotificationKind.Default;

    public string? ActionUrl { get; set; }

    public string? ActionText { get; set; }

    public bool IsRead { get; set; }

    public DateTimeOffset? ReadAt { get; set; }
}
