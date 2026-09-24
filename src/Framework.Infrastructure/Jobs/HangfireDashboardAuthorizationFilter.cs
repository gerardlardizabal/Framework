using Framework.Domain.Authorization;
using Hangfire.Dashboard;

namespace Framework.Infrastructure.Jobs;

public sealed class HangfireDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var user = context.GetHttpContext().User;
        return user.Identity?.IsAuthenticated == true
               && (user.IsInRole(RoleNames.Administrator)
                   || user.HasClaim(AppClaimTypes.Permission, Permissions.Jobs.View));
    }
}
