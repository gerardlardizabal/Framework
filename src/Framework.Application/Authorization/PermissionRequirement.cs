using Microsoft.AspNetCore.Authorization;

namespace Framework.Application.Authorization;

public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
