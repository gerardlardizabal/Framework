using Framework.Application.Jobs;
using Hangfire;
using Microsoft.Extensions.Hosting;

namespace Framework.Infrastructure.Jobs;

public sealed class RecurringJobScheduler : IHostedService
{
    public Task StartAsync(CancellationToken cancellationToken)
    {
        RecurringJob.AddOrUpdate<IHeartbeatJob>(
            "heartbeat",
            job => job.RunAsync(CancellationToken.None),
            Cron.Minutely);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
