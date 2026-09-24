namespace Framework.Application.Common.Interfaces;

public interface IApplicationDbContext : IDisposable, IAsyncDisposable
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
