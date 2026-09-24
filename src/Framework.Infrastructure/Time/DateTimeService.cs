using Framework.Application.Common.Interfaces;

namespace Framework.Infrastructure.Time;

public sealed class DateTimeService : IDateTime
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
