using Microsoft.AspNetCore.Identity;

namespace Framework.Infrastructure.Identity;

public sealed class ApplicationSignInManager : SignInManager<ApplicationUser>
{
    public ApplicationSignInManager(
        UserManager<ApplicationUser> userManager,
        Microsoft.AspNetCore.Http.IHttpContextAccessor contextAccessor,
        IUserClaimsPrincipalFactory<ApplicationUser> claimsFactory,
        Microsoft.Extensions.Options.IOptions<IdentityOptions> optionsAccessor,
        Microsoft.Extensions.Logging.ILogger<SignInManager<ApplicationUser>> logger,
        Microsoft.AspNetCore.Authentication.IAuthenticationSchemeProvider schemes,
        IUserConfirmation<ApplicationUser> confirmation)
        : base(userManager, contextAccessor, claimsFactory, optionsAccessor, logger, schemes, confirmation)
    {
    }

    public override async Task<bool> CanSignInAsync(ApplicationUser user)
        => user.IsActive && await base.CanSignInAsync(user);
}
