using Framework.Domain.Notifications;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Framework.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationConfiguration : IEntityTypeConfiguration<UserNotification>
{
    public void Configure(EntityTypeBuilder<UserNotification> builder)
    {
        builder.ToTable("Notifications");
        builder.Property(notification => notification.Title).HasMaxLength(160).IsRequired();
        builder.Property(notification => notification.Message).HasMaxLength(512);
        builder.Property(notification => notification.Kind).HasConversion<string>().HasMaxLength(32);
        builder.Property(notification => notification.ActionUrl).HasMaxLength(512);
        builder.Property(notification => notification.ActionText).HasMaxLength(64);
        builder.HasIndex(notification => new { notification.UserId, notification.IsRead, notification.Created });
        builder.HasIndex(notification => notification.UserId);
    }
}
