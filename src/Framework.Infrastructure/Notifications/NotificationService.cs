using Framework.Application.Common.Interfaces;
using Framework.Application.Notifications;
using Framework.Domain.Common;
using Framework.Domain.Notifications;
using Framework.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Framework.Infrastructure.Notifications;

public sealed class NotificationService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IDateTime dateTime) : INotificationService
{
    public async Task<Result<Guid>> CreateAsync(CreateNotificationRequest request, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var notification = new UserNotification
        {
            UserId = request.UserId,
            Title = request.Title.Trim(),
            Message = string.IsNullOrWhiteSpace(request.Message) ? null : request.Message.Trim(),
            Kind = request.Kind,
            ActionUrl = string.IsNullOrWhiteSpace(request.ActionUrl) ? null : request.ActionUrl.Trim(),
            ActionText = string.IsNullOrWhiteSpace(request.ActionText) ? null : request.ActionText.Trim()
        };

        context.Notifications.Add(notification);
        await context.SaveChangesAsync(cancellationToken);
        return Result<Guid>.Success(notification.Id);
    }

    public async Task<IReadOnlyList<NotificationDto>> GetRecentAsync(
        Guid userId,
        int take,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var now = dateTime.UtcNow;
        var items = await context.Notifications
            .AsNoTracking()
            .Where(notification => notification.UserId == userId)
            .OrderByDescending(notification => notification.Created)
            .Take(take)
            .ToListAsync(cancellationToken);

        return items.Select(notification => Map(notification, now)).ToArray();
    }

    public async Task<int> GetUnreadCountAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        return await context.Notifications.CountAsync(
            notification => notification.UserId == userId && !notification.IsRead,
            cancellationToken);
    }

    public async Task<Result> MarkReadAsync(Guid userId, Guid notificationId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var notification = await context.Notifications
            .FirstOrDefaultAsync(item => item.Id == notificationId && item.UserId == userId, cancellationToken);
        if (notification is null)
        {
            return Result.Failure("Notification was not found.");
        }

        if (!notification.IsRead)
        {
            notification.IsRead = true;
            notification.ReadAt = dateTime.UtcNow;
            await context.SaveChangesAsync(cancellationToken);
        }

        return Result.Success();
    }

    public async Task<Result> MarkAllReadAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var unread = await context.Notifications
            .Where(notification => notification.UserId == userId && !notification.IsRead)
            .ToListAsync(cancellationToken);

        if (unread.Count == 0)
        {
            return Result.Success();
        }

        var now = dateTime.UtcNow;
        foreach (var notification in unread)
        {
            notification.IsRead = true;
            notification.ReadAt = now;
        }

        await context.SaveChangesAsync(cancellationToken);
        return Result.Success();
    }

    private static NotificationDto Map(UserNotification notification, DateTimeOffset now) =>
        new(
            notification.Id,
            notification.Title,
            notification.Message,
            notification.Kind.ToString(),
            notification.ActionUrl,
            notification.ActionText,
            notification.IsRead,
            notification.Created,
            RelativeTime.From(notification.Created, now));
}
