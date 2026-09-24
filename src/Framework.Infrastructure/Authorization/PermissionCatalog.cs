using Framework.Application.Common.Interfaces;
using Framework.Application.Identity;
using Framework.Domain.Authorization;
using Framework.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Framework.Infrastructure.Authorization;

public sealed class PermissionCatalog(
    IDbContextFactory<ApplicationDbContext> dbContextFactory,
    IMemoryCache cache) : IPermissionCatalog
{
    public const string CacheKey = "permissions:catalog";

    public async Task<IReadOnlyList<PermissionListItemDto>> GetCatalogAsync(CancellationToken cancellationToken = default)
    {
        var catalog = await cache.GetOrCreateAsync(CacheKey, async entry =>
        {
            entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
            await using var context = await dbContextFactory.CreateDbContextAsync(cancellationToken);
            return await context.Permissions
                .AsNoTracking()
                .OrderBy(permission => permission.Group)
                .ThenBy(permission => permission.DisplayName)
                .Select(permission => new PermissionListItemDto(
                    permission.Id,
                    permission.Group,
                    permission.Name,
                    permission.DisplayName,
                    permission.Description,
                    permission.IsSystem))
                .ToListAsync(cancellationToken);
        });

        return catalog ?? [];
    }

    public async Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken = default)
        => (await GetCatalogAsync(cancellationToken)).Select(permission => permission.Name).ToArray();

    public async Task<bool> IsDefinedAsync(string permission, CancellationToken cancellationToken = default)
    {
        if (Permissions.IsBuiltIn(permission))
        {
            return true;
        }

        var catalog = await GetCatalogAsync(cancellationToken);
        return catalog.Any(item => item.Name.Equals(permission, StringComparison.Ordinal));
    }

    public void Invalidate() => cache.Remove(CacheKey);
}
