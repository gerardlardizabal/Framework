using Framework.Application.Identity;

namespace Framework.Application.Common.Interfaces;

public interface IPermissionCatalog
{
    Task<IReadOnlyList<PermissionListItemDto>> GetCatalogAsync(CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetNamesAsync(CancellationToken cancellationToken = default);

    Task<bool> IsDefinedAsync(string permission, CancellationToken cancellationToken = default);

    void Invalidate();
}
