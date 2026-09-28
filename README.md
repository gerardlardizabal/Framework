# Framework

.NET 10 Blazor boilerplate with Clean Architecture, ASP.NET Core Identity, permission-based authorization, and Tailwind CSS v4.

## Solution layout

```
src/Framework.Domain           Business rules, permission catalog, result types
src/Framework.Application      Use cases (MediatR), validators, authorization attributes
src/Framework.Infrastructure   EF Core, Identity, Hangfire, permission policies, seeding
src/Framework.Web              Blazor UI, Identity pages, Tailwind
tests/                         Domain, application, and architecture tests
```

Dependencies point inward: Web → Infrastructure → Application → Domain.

## Prerequisites

- .NET 10 SDK
- Node.js 20+ (for the Tailwind v4 CLI used during `dotnet build`)
- SQL Server LocalDB (installed with Visual Studio / SQL Server Express)

## Run

```bash
dotnet restore
dotnet build
dotnet run --project src/Framework.Web
```

The first run applies migrations and seeds:

| Account | Password | Access |
| --- | --- | --- |
| `admin@localhost` | `Admin123!` | Administrator (every permission) |
| New registrations | — | `User` role (`Permissions.Dashboard.View`) |

Change the seeded administrator in `src/Framework.Web/appsettings.json` under `Seed`.

The default connection string uses LocalDB and creates a `Framework` database on first run. Point `ConnectionStrings:DefaultConnection` at a full SQL Server instance when you need to:

```
Server=localhost,1433;Database=Framework;User Id=sa;Password=...;TrustServerCertificate=true;MultipleActiveResultSets=true
```

## Permissions

Built-in permissions used by the app (Users, Roles, Dashboard, Jobs, PermissionTypes) still live as constants in `Framework.Domain.Authorization.Permissions`. Those names are seeded into SQL Server on startup.

Custom permission types are stored in the `PermissionDefinitions` table. Add them from **Administration → Permissions**. They become authorization policies immediately and can be assigned on the role editor. The Administrator role receives every permission, including new ones.

Protect a page (compile-time name):

```razor
@attribute [HasPermission(Permissions.Users.View)]
```

Protect a fragment:

```razor
<AuthorizeView Policy="@Permissions.Users.Create">
    <a href="admin/users/new">New user</a>
</AuthorizeView>
```

Protect a feature with a database permission:

```razor
<AuthorizeView Policy="Permissions.Reports.View">
    <a href="reports">Reports</a>
</AuthorizeView>
```

Pages that use `[HasPermission]` still need a code change because the attribute is compiled. Adding a permission type so it can be granted to roles does not.

## Logging

Serilog is configured in `src/Framework.Web/appsettings.json`. Console output is always on; rolling JSON files go to `src/Framework.Web/logs`. Unhandled HTTP exceptions are logged by `GlobalExceptionHandler` (and Blazor render errors by `LoggingErrorBoundary`) through the same Serilog pipeline.

## Background jobs

Hangfire uses the same SQL Server database (`Hangfire` schema) and starts a worker with the web host. A `heartbeat` job is registered every minute as an example.

The dashboard lives at `/hangfire` and requires `Permissions.Jobs.View`. Administrators receive that permission automatically.

## Notifications

In-app notifications follow Untitled UI cues: a floating toast for short confirmations, and a persistent inbox behind the header bell for anything you may need later.

- Toasts appear top-right on mobile and bottom-right on desktop, pause while hovered, and dismiss after five seconds. Call `frameworkUi.toast({ kind, title, message })` from JavaScript (`success`, `brand`, `warning`, `error`, or `default`).
- The bell lists recent items with unread count. Full history is at `/notifications`. Authenticated users only see their own.
- Create inbox items from Application with `INotificationService` or `CreateNotificationCommand`. Creating a user already notifies the signed-in administrator.

## Dialogs

`AppDialog` is an Untitled UI-style modal for **message** (acknowledge) and **confirmation** (cancel / confirm) flows. Interactive Server pages inject `IDialogService` and render `<DialogHost />` once on the same circuit.

```razor
<DialogHost />

@code {
    private async Task DeleteAsync()
    {
        if (!await Dialogs.ConfirmAsync(
            "Delete user?",
            "This account will be removed. This action cannot be undone.",
            kind: DialogKind.Error,
            confirmText: "Delete",
            destructive: true))
        {
            return;
        }

        // ...
        await Dialogs.ShowMessageAsync("Couldn't delete user", error, DialogKind.Error);
    }
}
```

Delete on users, roles, and permission types uses confirmation. Dashboard includes a Message / Confirm demo.

## Tailwind v4

CSS is compiled as part of the Web project build:

- Source: `src/Framework.Web/Styles/app.css`
- Output: `src/Framework.Web/wwwroot/css/app.css`

The UI follows Untitled UI cues: Inter, a gray canvas, purple brand, light sidebar, page headers, and table/form patterns.

Watch independently while iterating on markup:

```bash
cd src/Framework.Web
npm run css:watch
```

## DbContext factories

EF Core is registered with `IDbContextFactory<ApplicationDbContext>` so Blazor circuits and Hangfire jobs can create short-lived contexts instead of sharing a scoped instance.

- Runtime: inject `IApplicationDbContextFactory` from Application, or `IDbContextFactory<ApplicationDbContext>` from Infrastructure
- Identity still gets a scoped `ApplicationDbContext` created from the factory for the current request/circuit
- Design-time: `ApplicationDbContextDesignTimeFactory` implements `IDesignTimeDbContextFactory<ApplicationDbContext>` for `dotnet ef`

```csharp
await using var db = await dbContextFactory.CreateDbContextAsync(cancellationToken);
```

## Migrations

```bash
dotnet ef migrations add <Name> --project src/Framework.Infrastructure --startup-project src/Framework.Web
```
