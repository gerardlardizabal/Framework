using Framework.Application.Common.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Framework.Infrastructure.Persistence;

public sealed class ApplicationDbContextFactory(IDbContextFactory<ApplicationDbContext> factory)
    : IApplicationDbContextFactory
{
    public IApplicationDbContext CreateDbContext() => factory.CreateDbContext();

    public async Task<IApplicationDbContext> CreateDbContextAsync(CancellationToken cancellationToken = default)
        => await factory.CreateDbContextAsync(cancellationToken);
}
