namespace Framework.Application.Common.Interfaces;

public interface IDateTime
{
    DateTimeOffset UtcNow { get; }
}
