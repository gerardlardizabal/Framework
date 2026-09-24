using System.Security.Claims;
using Framework.Domain.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Framework.Infrastructure.Identity;

public sealed class ApplicationUserClaimsPrincipalFactory(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IOptions<IdentityOptions> options)
    : UserClaimsPrincipalFactory<ApplicationUser, ApplicationRole>(userManager, roleManager, options)
{
    protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
    {
        var identity = await base.GenerateClaimsAsync(user);

        if (!string.IsNullOrWhiteSpace(user.FirstName))
        {
            identity.AddClaim(new Claim(AppClaimTypes.FirstName, user.FirstName));
        }

        if (!string.IsNullOrWhiteSpace(user.LastName))
        {
            identity.AddClaim(new Claim(AppClaimTypes.LastName, user.LastName));
        }

        foreach (var roleName in await UserManager.GetRolesAsync(user))
        {
            var role = await RoleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                continue;
            }

            foreach (var claim in await RoleManager.GetClaimsAsync(role))
            {
                if (claim.Type == AppClaimTypes.Permission && !identity.HasClaim(claim.Type, claim.Value))
                {
                    identity.AddClaim(claim);
                }
            }
        }

        return identity;
    }
}
