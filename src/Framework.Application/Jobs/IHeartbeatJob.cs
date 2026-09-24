namespace Framework.Application.Jobs;

public interface IHeartbeatJob
{
    Task RunAsync(CancellationToken cancellationToken = default);
}
