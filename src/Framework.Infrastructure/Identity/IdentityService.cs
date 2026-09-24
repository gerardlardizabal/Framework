using System.Security.Claims;
using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Authorization;
using Framework.Domain.Common;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Framework.Infrastructure.Identity;

public sealed class IdentityService(
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    ICurrentUser currentUser,
    IPermissionCatalog permissionCatalog) : IIdentityService
{
    public async Task<IReadOnlyList<UserListItemDto>> GetUsersAsync(CancellationToken cancellationToken = default)
    {
        var users = await userManager.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .ToListAsync(cancellationToken);

        var result = new List<UserListItemDto>(users.Count);
        foreach (var user in users)
        {
            var roles = await userManager.GetRolesAsync(user);
            result.Add(new UserListItemDto(
                user.Id,
                user.Email ?? string.Empty,
                user.DisplayName,
                user.EmailConfirmed,
                user.IsActive,
                roles.ToArray()));
        }

        return result;
    }

    public async Task<UserDetailsDto?> GetUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return null;
        }

        var roles = await userManager.GetRolesAsync(user);
        return new UserDetailsDto(
            user.Id,
            user.Email ?? string.Empty,
            user.UserName,
            user.FirstName,
            user.LastName,
            user.EmailConfirmed,
            user.IsActive,
            roles.ToArray());
    }

    public async Task<Result<Guid>> CreateUserAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            IsActive = request.IsActive,
            EmailConfirmed = true
        };

        var create = await userManager.CreateAsync(user, request.Password);
        if (!create.Succeeded)
        {
            return Result<Guid>.Failure(create.Errors.Select(e => e.Description));
        }

        if (request.Roles.Count > 0)
        {
            var roles = await userManager.AddToRolesAsync(user, request.Roles);
            if (!roles.Succeeded)
            {
                return Result<Guid>.Failure(roles.Errors.Select(e => e.Description));
            }
        }

        return Result<Guid>.Success(user.Id);
    }

    public async Task<Result> UpdateUserAsync(UpdateUserRequest request, CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(request.Id.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        user.FirstName = request.FirstName;
        user.LastName = request.LastName;
        user.IsActive = request.IsActive;

        var update = await userManager.UpdateAsync(user);
        if (!update.Succeeded)
        {
            return Result.Failure(update.Errors.Select(e => e.Description));
        }

        return await SetUserRolesAsync(request.Id, request.Roles, cancellationToken);
    }

    public async Task<Result> DeleteUserAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        if (currentUser.Id == userId)
        {
            return Result.Failure("You cannot delete your own account.");
        }

        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        if (await userManager.IsInRoleAsync(user, RoleNames.Administrator))
        {
            var adminCount = await CountAdminsAsync(cancellationToken);
            if (adminCount <= 1)
            {
                return Result.Failure("The last administrator cannot be deleted.");
            }
        }

        var delete = await userManager.DeleteAsync(user);
        return delete.Succeeded
            ? Result.Success()
            : Result.Failure(delete.Errors.Select(e => e.Description));
    }

    public async Task<Result> SetUserRolesAsync(
        Guid userId,
        IReadOnlyCollection<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString());
        if (user is null)
        {
            return Result.Failure("User was not found.");
        }

        var currentRoles = await userManager.GetRolesAsync(user);
        var removingAdmin = currentRoles.Contains(RoleNames.Administrator)
                            && !roleNames.Contains(RoleNames.Administrator);

        if (removingAdmin && await CountAdminsAsync(cancellationToken) <= 1)
        {
            return Result.Failure("The last administrator cannot be removed from the Administrator role.");
        }

        var toRemove = currentRoles.Except(roleNames).ToArray();
        if (toRemove.Length > 0)
        {
            var remove = await userManager.RemoveFromRolesAsync(user, toRemove);
            if (!remove.Succeeded)
            {
                return Result.Failure(remove.Errors.Select(e => e.Description));
            }
        }

        var toAdd = roleNames.Except(currentRoles).ToArray();
        if (toAdd.Length > 0)
        {
            var add = await userManager.AddToRolesAsync(user, toAdd);
            if (!add.Succeeded)
            {
                return Result.Failure(add.Errors.Select(e => e.Description));
            }
        }

        await userManager.UpdateSecurityStampAsync(user);
        return Result.Success();
    }

    public async Task<IReadOnlyList<RoleListItemDto>> GetRolesAsync(CancellationToken cancellationToken = default)
    {
        var roles = await roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .ToListAsync(cancellationToken);

        var result = new List<RoleListItemDto>(roles.Count);
        foreach (var role in roles)
        {
            var users = await userManager.GetUsersInRoleAsync(role.Name!);
            var claims = await roleManager.GetClaimsAsync(role);
            result.Add(new RoleListItemDto(
                role.Id,
                role.Name!,
                role.Description,
                users.Count,
                claims.Count(c => c.Type == AppClaimTypes.Permission)));
        }

        return result;
    }

    public async Task<RoleDetailsDto?> GetRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            return null;
        }

        var claims = await roleManager.GetClaimsAsync(role);
        return new RoleDetailsDto(
            role.Id,
            role.Name!,
            role.Description,
            claims.Where(c => c.Type == AppClaimTypes.Permission).Select(c => c.Value).ToArray());
    }

    public async Task<Result<Guid>> CreateRoleAsync(CreateRoleRequest request, CancellationToken cancellationToken = default)
    {
        if (await roleManager.RoleExistsAsync(request.Name))
        {
            return Result<Guid>.Failure($"Role '{request.Name}' already exists.");
        }

        var role = new ApplicationRole(request.Name) { Description = request.Description };
        var create = await roleManager.CreateAsync(role);
        return create.Succeeded
            ? Result<Guid>.Success(role.Id)
            : Result<Guid>.Failure(create.Errors.Select(e => e.Description));
    }

    public async Task<Result> UpdateRoleAsync(UpdateRoleRequest request, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(request.Id.ToString());
        if (role is null)
        {
            return Result.Failure("Role was not found.");
        }

        if (role.Name == RoleNames.Administrator && request.Name != RoleNames.Administrator)
        {
            return Result.Failure("The Administrator role cannot be renamed.");
        }

        var existing = await roleManager.FindByNameAsync(request.Name);
        if (existing is not null && existing.Id != role.Id)
        {
            return Result.Failure($"Role '{request.Name}' already exists.");
        }

        role.Name = request.Name;
        role.NormalizedName = roleManager.NormalizeKey(request.Name);
        role.Description = request.Description;

        var update = await roleManager.UpdateAsync(role);
        return update.Succeeded
            ? Result.Success()
            : Result.Failure(update.Errors.Select(e => e.Description));
    }

    public async Task<Result> DeleteRoleAsync(Guid roleId, CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            return Result.Failure("Role was not found.");
        }

        if (role.Name is RoleNames.Administrator or RoleNames.User)
        {
            return Result.Failure("Built-in roles cannot be deleted.");
        }

        var users = await userManager.GetUsersInRoleAsync(role.Name!);
        if (users.Count > 0)
        {
            return Result.Failure("Remove all users from this role before deleting it.");
        }

        var delete = await roleManager.DeleteAsync(role);
        return delete.Succeeded
            ? Result.Success()
            : Result.Failure(delete.Errors.Select(e => e.Description));
    }

    public async Task<Result> SetRolePermissionsAsync(
        Guid roleId,
        IReadOnlyCollection<string> permissions,
        CancellationToken cancellationToken = default)
    {
        var role = await roleManager.FindByIdAsync(roleId.ToString());
        if (role is null)
        {
            return Result.Failure("Role was not found.");
        }

        if (role.Name == RoleNames.Administrator)
        {
            permissions = await permissionCatalog.GetNamesAsync(cancellationToken);
        }

        var defined = (await permissionCatalog.GetNamesAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);
        var invalid = permissions.Where(permission => !defined.Contains(permission)).ToArray();
        if (invalid.Length > 0)
        {
            return Result.Failure($"Unknown permissions: {string.Join(", ", invalid)}");
        }

        var current = (await roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == AppClaimTypes.Permission)
            .ToList();

        foreach (var claim in current.Where(c => !permissions.Contains(c.Value)))
        {
            var remove = await roleManager.RemoveClaimAsync(role, claim);
            if (!remove.Succeeded)
            {
                return Result.Failure(remove.Errors.Select(e => e.Description));
            }
        }

        var existingValues = current.Select(c => c.Value).ToHashSet();
        foreach (var permission in permissions.Where(p => !existingValues.Contains(p)))
        {
            var add = await roleManager.AddClaimAsync(role, new Claim(AppClaimTypes.Permission, permission));
            if (!add.Succeeded)
            {
                return Result.Failure(add.Errors.Select(e => e.Description));
            }
        }

        return Result.Success();
    }

    public async Task<IReadOnlyList<string>> GetRoleNamesAsync(CancellationToken cancellationToken = default)
        => await roleManager.Roles
            .AsNoTracking()
            .OrderBy(r => r.Name)
            .Select(r => r.Name!)
            .ToListAsync(cancellationToken);

    private async Task<int> CountAdminsAsync(CancellationToken cancellationToken)
    {
        var admins = await userManager.GetUsersInRoleAsync(RoleNames.Administrator);
        return admins.Count;
    }
}
