using System.Security.Claims;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Authorization;
using Framework.Domain.Common;
using Framework.Infrastructure.Identity;
using Framework.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PermissionEntity = Framework.Domain.Authorization.Permission;

namespace Framework.Infrastructure.Authorization;

public sealed class PermissionService(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IPermissionCatalog catalog,
    RoleManager<ApplicationRole> roleManager) : IPermissionService
{
    public Task<IReadOnlyList<PermissionListItemDto>> GetPermissionsAsync(CancellationToken cancellationToken = default)
        => catalog.GetCatalogAsync(cancellationToken);

    public async Task<PermissionListItemDto?> GetPermissionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var permissions = await catalog.GetCatalogAsync(cancellationToken);
        return permissions.FirstOrDefault(permission => permission.Id == id);
    }

    public async Task<Result<Guid>> CreatePermissionAsync(
        CreatePermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        var name = Permissions.CreateName(request.Group, request.Action);
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);

        if (await context.Permissions.AnyAsync(permission => permission.Name == name, cancellationToken))
        {
            return Result<Guid>.Failure($"Permission '{name}' already exists.");
        }

        var permission = new PermissionEntity
        {
            Group = request.Group.Trim(),
            Name = name,
            DisplayName = string.IsNullOrWhiteSpace(request.DisplayName) ? request.Action.Trim() : request.DisplayName.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            IsSystem = false
        };

        context.Permissions.Add(permission);
        await context.SaveChangesAsync(cancellationToken);
        catalog.Invalidate();
        await GrantAdministratorAsync(permission.Name);

        return Result<Guid>.Success(permission.Id);
    }

    public async Task<Result> UpdatePermissionAsync(
        UpdatePermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var permission = await context.Permissions.FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);
        if (permission is null)
        {
            return Result.Failure("Permission was not found.");
        }

        permission.Group = request.Group.Trim();
        permission.DisplayName = request.DisplayName.Trim();
        permission.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();

        await context.SaveChangesAsync(cancellationToken);
        catalog.Invalidate();
        return Result.Success();
    }

    public async Task<Result> DeletePermissionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var permission = await context.Permissions.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (permission is null)
        {
            return Result.Failure("Permission was not found.");
        }

        if (permission.IsSystem || Permissions.IsBuiltIn(permission.Name))
        {
            return Result.Failure("Built-in permissions cannot be deleted.");
        }

        await RemoveClaimsAsync(permission.Name);
        context.Permissions.Remove(permission);
        await context.SaveChangesAsync(cancellationToken);
        catalog.Invalidate();
        return Result.Success();
    }

    private async Task GrantAdministratorAsync(string permissionName)
    {
        var administrator = await roleManager.FindByNameAsync(RoleNames.Administrator);
        if (administrator is null)
        {
            return;
        }

        var claims = await roleManager.GetClaimsAsync(administrator);
        if (claims.Any(claim => claim.Type == AppClaimTypes.Permission && claim.Value == permissionName))
        {
            return;
        }

        await roleManager.AddClaimAsync(administrator, new Claim(AppClaimTypes.Permission, permissionName));
    }

    private async Task RemoveClaimsAsync(string permissionName)
    {
        var roles = await roleManager.Roles.ToListAsync();
        foreach (var role in roles)
        {
            var claims = (await roleManager.GetClaimsAsync(role))
                .Where(claim => claim.Type == AppClaimTypes.Permission && claim.Value == permissionName)
                .ToList();

            foreach (var claim in claims)
            {
                await roleManager.RemoveClaimAsync(role, claim);
            }
        }
    }
}
