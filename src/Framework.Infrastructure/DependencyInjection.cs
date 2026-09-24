using Framework.Application.Common.Interfaces;
using Framework.Infrastructure.Authorization;
using Framework.Infrastructure.Identity;
using Framework.Infrastructure.Notifications;
using Framework.Infrastructure.Jobs;
using Framework.Infrastructure.Persistence;
using Framework.Infrastructure.Persistence.Interceptors;
using Framework.Infrastructure.Time;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Framework.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddMemoryCache();
        services.AddSingleton<IPermissionCatalog, PermissionCatalog>();
        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<IDateTime, DateTimeService>();
        services.AddSingleton<ICurrentUser, CurrentUser>();
        services.AddSingleton<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddSingleton<IApplicationDbContextFactory, ApplicationDbContextFactory>();
        services.AddHttpContextAccessor();

        var connectionString = configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");

        services.AddDbContextFactory<ApplicationDbContext>((sp, options) =>
            ApplicationDbContextOptions.Configure(
                options,
                connectionString,
                sp.GetServices<ISaveChangesInterceptor>()));

        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<ApplicationDbContext>>().CreateDbContext());
        services.AddScoped<IApplicationDbContext>(sp => sp.GetRequiredService<ApplicationDbContext>());
        services.AddScoped<ApplicationDbContextInitializer>();

        services.AddIdentityCore<ApplicationUser>(options =>
            {
                options.SignIn.RequireConfirmedAccount = false;
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireDigit = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager<ApplicationSignInManager>()
            .AddClaimsPrincipalFactory<ApplicationUserClaimsPrincipalFactory>()
            .AddDefaultTokenProviders();

        services.AddScoped<IIdentityService, IdentityService>();
        services.AddScoped<INotificationService, NotificationService>();
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
        services.AddBackgroundJobs(configuration);

        return services;
    }

    public static async Task InitialiseDatabaseAsync(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var initializer = scope.ServiceProvider.GetRequiredService<ApplicationDbContextInitializer>();
        await initializer.InitialiseAsync();
        await initializer.SeedAsync();
    }
}
