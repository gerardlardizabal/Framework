using Framework.Application.Common.Interfaces;
using Framework.Application.Jobs;
using Microsoft.Extensions.Logging;

namespace Framework.Infrastructure.Jobs;

public sealed class HeartbeatJob(
    IApplicationDbContextFactory dbContextFactory,
    ILogger<HeartbeatJob> logger,
    IDateTime dateTime) : IHeartbeatJob
{
    public async Task RunAsync(CancellationToken cancellationToken = default)
    {
        await using (await dbContextFactory.CreateDbContextAsync(cancellationToken))
        {
            logger.LogInformation("Hangfire heartbeat at {UtcNow}", dateTime.UtcNow);
        }
    }
}
