using System.Security.Claims;
using Framework.Application.Common.Interfaces;
using Framework.Domain.Authorization;
using Framework.Domain.Notifications;
using Framework.Infrastructure.Identity;
using Framework.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Framework.Infrastructure.Persistence;

public sealed class ApplicationDbContextInitializer(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    UserManager<ApplicationUser> userManager,
    RoleManager<ApplicationRole> roleManager,
    IPermissionCatalog permissionCatalog,
    IConfiguration configuration,
    ILogger<ApplicationDbContextInitializer> logger)
{
    public async Task InitialiseAsync()
    {
        try
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            await context.Database.MigrateAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while migrating the database.");
            throw;
        }
    }

    public async Task SeedAsync()
    {
        try
        {
            await TrySeedAsync();
        }
        catch (Exception exception)
        {
            logger.LogError(exception, "An error occurred while seeding the database.");
            throw;
        }
    }

    private async Task TrySeedAsync()
    {
        await EnsureRoleAsync(RoleNames.Administrator, "Full access to every feature.");
        await EnsureRoleAsync(RoleNames.User, "Default application user.");
        await EnsurePermissionDefinitionsAsync();

        var administrator = await roleManager.FindByNameAsync(RoleNames.Administrator);
        if (administrator is not null)
        {
            await using var context = await dbContextFactory.CreateDbContextAsync();
            var allPermissions = await context.Permissions
                .AsNoTracking()
                .Select(permission => permission.Name)
                .ToListAsync();
            await SyncPermissionsAsync(administrator, allPermissions);
        }

        var userRole = await roleManager.FindByNameAsync(RoleNames.User);
        if (userRole is not null)
        {
            await SyncPermissionsAsync(userRole, [Permissions.Dashboard.View]);
        }

        var adminEmail = configuration["Seed:AdminEmail"] ?? "admin@localhost";
        var adminPassword = configuration["Seed:AdminPassword"] ?? "Admin123!";

        var admin = await userManager.FindByEmailAsync(adminEmail);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true,
                FirstName = "System",
                LastName = "Administrator",
                IsActive = true
            };

            var create = await userManager.CreateAsync(admin, adminPassword);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create the default administrator: {string.Join(", ", create.Errors.Select(e => e.Description))}");
            }
        }

        if (!await userManager.IsInRoleAsync(admin, RoleNames.Administrator))
        {
            await userManager.AddToRoleAsync(admin, RoleNames.Administrator);
        }

        await EnsureWelcomeNotificationsAsync(admin.Id);

        permissionCatalog.Invalidate();
    }

    private async Task EnsureWelcomeNotificationsAsync(Guid userId)
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        if (await context.Notifications.AnyAsync(notification => notification.UserId == userId))
        {
            return;
        }

        context.Notifications.AddRange(
            new UserNotification
            {
                UserId = userId,
                Title = "Welcome to Framework",
                Message = "Your workspace is ready. Start with users, roles, and permissions.",
                Kind = NotificationKind.Brand,
                ActionUrl = "dashboard",
                ActionText = "Open dashboard"
            },
            new UserNotification
            {
                UserId = userId,
                Title = "Permissions live in the database",
                Message = "Add permission types without rebuilding. Assign them to roles from Administration.",
                Kind = NotificationKind.Default,
                ActionUrl = "admin/permissions",
                ActionText = "Manage permissions"
            },
            new UserNotification
            {
                UserId = userId,
                Title = "Background jobs are running",
                Message = "Hangfire is installed. Open the dashboard when you need to inspect queues.",
                Kind = NotificationKind.Success,
                ActionUrl = "hangfire",
                ActionText = "Open Hangfire"
            });

        await context.SaveChangesAsync();
    }

    private async Task EnsurePermissionDefinitionsAsync()
    {
        await using var context = await dbContextFactory.CreateDbContextAsync();
        var existing = await context.Permissions.ToListAsync();

        foreach (var definition in Permissions.BuiltIn)
        {
            var permission = existing.FirstOrDefault(item => item.Name == definition.Name);
            if (permission is null)
            {
                context.Permissions.Add(new Permission
                {
                    Group = definition.Group,
                    Name = definition.Name,
                    DisplayName = definition.DisplayName,
                    IsSystem = true
                });
                continue;
            }

            permission.Group = definition.Group;
            permission.DisplayName = definition.DisplayName;
            permission.IsSystem = true;
        }

        await context.SaveChangesAsync();
    }

    private async Task EnsureRoleAsync(string name, string description)
    {
        var role = await roleManager.FindByNameAsync(name);
        if (role is null)
        {
            role = new ApplicationRole(name) { Description = description };
            var create = await roleManager.CreateAsync(role);
            if (!create.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Failed to create role '{name}': {string.Join(", ", create.Errors.Select(e => e.Description))}");
            }

            return;
        }

        if (role.Description != description)
        {
            role.Description = description;
            await roleManager.UpdateAsync(role);
        }
    }

    private async Task SyncPermissionsAsync(ApplicationRole role, IReadOnlyCollection<string> permissions)
    {
        var current = (await roleManager.GetClaimsAsync(role))
            .Where(c => c.Type == AppClaimTypes.Permission)
            .ToList();

        foreach (var claim in current.Where(c => !permissions.Contains(c.Value)))
        {
            await roleManager.RemoveClaimAsync(role, claim);
        }

        var existing = current.Select(c => c.Value).ToHashSet();
        foreach (var permission in permissions.Where(p => !existing.Contains(p)))
        {
            await roleManager.AddClaimAsync(role, new Claim(AppClaimTypes.Permission, permission));
        }
    }
}
