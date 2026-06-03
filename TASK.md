# TASK — Clean Architecture cleanup

> **Scope:** two small structural cleanups on the backend. Nothing is broken; this
> is tech-debt tidy-up to keep the layering strict. Low risk, no behaviour change.
>
> **Branch:** `hotfix/clean-arch-cleanup`
> **When finished:** delete this file and close the branch (see _Done criteria_).

---

## Background

The backend follows Clean Architecture across four projects:

```
InternalDocs.Domain          (entities, no dependencies)
InternalDocs.Application      (abstractions + services, depends on Domain only)
InternalDocs.Infrastructure   (EF Core, repositories, auth — implements Application abstractions)
InternalDocs.Api             (controllers, DI composition root)
```

The dependency rule, domain purity, dependency inversion, thin controllers, and
DTO mapping are all in good shape. The two items below are the only deviations.

---

## Task 1 — Remove the composition-root leak in `Program.cs`

**File:** `InternalDocs/InternalDocs.Api/Program.cs`

Today `Program.cs` reaches directly into Infrastructure concretions to run the
startup migrate + seed:

```csharp
using var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await dbContext.Database.MigrateAsync();
await DatabaseSeeder.SeedLocalUsersAsync(dbContext);
await DatabaseSeeder.SeedDocumentTypesAsync(dbContext);

if (string.Equals(Environment.GetEnvironmentVariable("SEED_DEMO_DATA"), "true",
    StringComparison.OrdinalIgnoreCase))
{
    await DatabaseSeeder.SeedDemoDataAsync(dbContext);
}
```

This is the only place Api code imports `InternalDocs.Infrastructure.Data`
(`AppDbContext`) and `InternalDocs.Infrastructure.Seeds` (`DatabaseSeeder`).

**Goal:** hide those concretions behind one Infrastructure-owned entry point so
`Program.cs` no longer references `AppDbContext`/`DatabaseSeeder` directly.

**Recommended approach:** add an extension method in Infrastructure, e.g.
`InternalDocs/InternalDocs.Infrastructure/DependencyInjection.cs` (or a new
`DatabaseInitializerExtensions.cs`):

```csharp
public static async Task InitializeDatabaseAsync(this IServiceProvider services)
{
    using var scope = services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
    await DatabaseSeeder.SeedLocalUsersAsync(dbContext);
    await DatabaseSeeder.SeedDocumentTypesAsync(dbContext);

    if (string.Equals(Environment.GetEnvironmentVariable("SEED_DEMO_DATA"), "true",
        StringComparison.OrdinalIgnoreCase))
    {
        await DatabaseSeeder.SeedDemoDataAsync(dbContext);
    }
}
```

Then `Program.cs` becomes just:

```csharp
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
    app.UseCors("DevCors");

    await app.Services.InitializeDatabaseAsync();
}
```

and the `using InternalDocs.Infrastructure.Data;` / `using InternalDocs.Infrastructure.Seeds;`
imports drop out of `Program.cs` (keep `using InternalDocs.Infrastructure;` for the
extension method + `AddInfrastructure`).

**Important — preserve current behaviour exactly:**
- Migration + seeding must still run **only in the Development environment**.
- `SEED_DEMO_DATA=true` must still gate the demo dataset.
- Do not change the seeding order.

> Note: a parallel branch (`SETUP-5`, Docker) adds a `/health` endpoint and may
> shift line numbers in `Program.cs`. Match on the code shown above, not line
> numbers, and rebase if needed.

---

## Task 2 — Consolidate the `NotificationService` DI registration

**Files:** `InternalDocs/InternalDocs.Api/Program.cs`,
`InternalDocs/InternalDocs.Infrastructure/DependencyInjection.cs`

Every service is registered inside `AddInfrastructure(...)` **except** one, which
sits alone near the top of `Program.cs`:

```csharp
builder.Services.AddScoped<INotificationService, NotificationService>();
```

(`NotificationService` lives in `InternalDocs.Application/Notifications/` and is a
pure application service over `INotificationRepository`, so it is correctly placed
— only its *registration* is inconsistent.)

**Goal:** register it alongside the other services so all DI wiring lives in one
place.

**Approach:** move that `AddScoped<INotificationService, NotificationService>()`
line out of `Program.cs` and into `AddInfrastructure` in
`DependencyInjection.cs` (add `using InternalDocs.Application.Notifications;`
there). The three other application-service registrations in `Program.cs`
(`IDocumentService`, `IDocumentCatalogService`, `IApprovalService`) may be moved
the same way for full consistency — optional but preferred. Make sure the build
still resolves every dependency.

---

## Optional Task 3 — Refresh the architecture section of `project-plan.md`

**File:** `project-plan.md` → section `## 🏗️ Architecture`

It still describes a single-project layout (`/Controllers /Services /DTOs /Models
/Data /Middleware /Helpers`) and lists `AutoMapper` as a dependency. Reality is the
four-project Clean Architecture above, with **manual** `FromDto`/`FromEntity`
mapping (no AutoMapper). Update the doc to match what was built. Skip if out of
scope.

---

## Done criteria

1. `dotnet build` succeeds with no new warnings.
2. `dotnet test` passes (all existing tests green).
3. App still boots, migrates, and seeds in Development (verify locally or via
   `docker compose up --build` if the Docker branch is merged): login with
   `employee@internaldocs.local` / `EmployeePass123!` works.
4. `Program.cs` no longer imports `InternalDocs.Infrastructure.Data` or
   `InternalDocs.Infrastructure.Seeds`.
5. **Delete this `TASK.md` file** as part of the final commit.
6. Open the PR / merge to `main`, then delete the `hotfix/clean-arch-cleanup`
   branch.
