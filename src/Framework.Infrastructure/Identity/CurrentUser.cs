using System.Security.Claims;
using Framework.Application.Common.Interfaces;
using Framework.Domain.Authorization;
using Microsoft.AspNetCore.Http;

namespace Framework.Infrastructure.Identity;

public sealed class CurrentUser(IHttpContextAccessor httpContextAccessor) : ICurrentUser
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? Id =>
        Guid.TryParse(User?.FindFirstValue(ClaimTypes.NameIdentifier), out var id) ? id : null;

    public string? Email => User?.FindFirstValue(ClaimTypes.Email) ?? User?.Identity?.Name;

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated == true;

    public bool IsInRole(string role) => User?.IsInRole(role) == true;

    public bool HasPermission(string permission) =>
        IsInRole(RoleNames.Administrator)
        || User?.HasClaim(AppClaimTypes.Permission, permission) == true;
}
