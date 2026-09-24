using Framework.Application.Authorization;
using Framework.Domain.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace Framework.Infrastructure.Authorization;

public sealed class PermissionAuthorizationHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        if (context.User.IsInRole(RoleNames.Administrator)
            || context.User.HasClaim(AppClaimTypes.Permission, requirement.Permission))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
