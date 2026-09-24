using Framework.Domain.Common;

namespace Framework.Application.Notifications;

public static class RelativeTime
{
    public static string From(DateTimeOffset created, DateTimeOffset now)
    {
        var delta = now - created;
        if (delta.TotalSeconds < 45)
        {
            return "Just now";
        }

        if (delta.TotalMinutes < 60)
        {
            var minutes = Math.Max(1, (int)Math.Round(delta.TotalMinutes));
            return $"{minutes}m ago";
        }

        if (delta.TotalHours < 24)
        {
            var hours = Math.Max(1, (int)Math.Round(delta.TotalHours));
            return $"{hours}h ago";
        }

        if (delta.TotalDays < 7)
        {
            var days = Math.Max(1, (int)Math.Round(delta.TotalDays));
            return $"{days}d ago";
        }

        return created.ToLocalTime().ToString("dd MMM yyyy");
    }
}
