using Framework.Application.Jobs;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Framework.Infrastructure.Jobs;

public static class HangfireExtensions
{
    public static IServiceCollection AddBackgroundJobs(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddTransient<IHeartbeatJob, HeartbeatJob>();
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UseSqlServerStorage(connectionString, new SqlServerStorageOptions
            {
                CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
                SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
                QueuePollInterval = TimeSpan.Zero,
                UseRecommendedIsolationLevel = true,
                DisableGlobalLocks = true,
                PrepareSchemaIfNecessary = true,
                SchemaName = "Hangfire"
            }));
        services.AddHangfireServer();
        services.AddHostedService<RecurringJobScheduler>();

        return services;
    }

    public static IEndpointConventionBuilder MapBackgroundJobsDashboard(this IEndpointRouteBuilder endpoints)
    {
        return endpoints.MapHangfireDashboard("/hangfire", new DashboardOptions
        {
            DashboardTitle = "Framework Jobs",
            Authorization = [new HangfireDashboardAuthorizationFilter()],
            IgnoreAntiforgeryToken = true
        });
    }
}
