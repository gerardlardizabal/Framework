using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Framework.Infrastructure.Persistence;

internal static class ApplicationDbContextOptions
{
    public static void Configure(
        DbContextOptionsBuilder options,
        string connectionString,
        IEnumerable<IInterceptor>? interceptors = null)
    {
        if (interceptors is not null)
        {
            options.AddInterceptors(interceptors);
        }

        options.UseSqlServer(connectionString);
        // Identity schema v3 includes passkey metadata that can look like pending model
        // changes even when the snapshot is current.
        options.ConfigureWarnings(warnings =>
            warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
    }
}
