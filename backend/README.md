# backend — ABP modular monolith (.NET 10)

`Wathiq.slnx` was scaffolded in step 1.1 with the ABP CLI (`abp new Wathiq -t app -u no-ui -d ef
-dbms SqlServer --no-multi-tenancy --no-social-logins`), open-source modules only (all packages
resolve from `nuget.org` — no `myget.abp.io` commercial feed). `-u no-ui` means there is no
MVC/Angular/Blazor project here: `wathiq_portal`/`wathiq_admin` are the separate Nx apps in
[frontend/](../frontend/).

## Current layout

```
backend/
  Wathiq.slnx
  common.props                 shared MSBuild props (LangVersion, AbpProjectType)
  src/
    Wathiq.Domain(.Shared)/     Identity, OpenIddict, localization — from the ABP template
    Wathiq.Application(.Contracts)/
    Wathiq.EntityFrameworkCore/ WathiqDbContext (host schema: identity/OpenIddict tables)
    Wathiq.HttpApi(.Host/.Client)/
    Wathiq.DbMigrator/          console app: applies migrations + seeds admin user & OpenIddict clients
  test/
    Wathiq.{Domain,Application,EntityFrameworkCore}.Tests/, Wathiq.TestBase/
```

Each future module (`Documents`, `Reminders`, `Guides`, `Ai`, `Shared`) gets its own
`Domain`/`Application`/`EntityFrameworkCore`/`HttpApi` set under `backend/modules/<Name>/` with
its own `DbContext` and SQL schema — **no cross-module DB joins** (ADR-001). See
`docs/deliverables/architecture.md`.

## Running it locally

1. **Database** — LocalDB, instance `MSSQLLocalDB`, database `Wathiq` (default connection string
   in every `appsettings.json`):
   ```
   Server=(localdb)\MSSQLLocalDB;Database=Wathiq;Trusted_Connection=True;TrustServerCertificate=True
   ```
   Alternative: `docker compose up -d sql` from the repo root, then point `ConnectionStrings:Default`
   at `localhost,14333` with the `sa` password from `.env`.
2. **OpenIddict certificate** (one-time, not committed — see `.gitignore`):
   ```powershell
   dotnet dev-certs https -v -ep src/Wathiq.HttpApi.Host/openiddict.pfx -p 31c8c74d-bb83-42ac-b8d2-96e80652ed97
   ```
3. **Migrate + seed** (creates the DB, an `admin` user, and the `Wathiq_App`/`Wathiq_Swagger`
   OpenIddict clients):
   ```
   cd src/Wathiq.DbMigrator && dotnet run
   ```
   Must be run *from that project's own folder* — configuration is loaded relative to the
   current working directory, not the project path.
4. **Run the API**:
   ```
   cd src/Wathiq.HttpApi.Host && dotnet run
   ```
   Health check: `https://localhost:44352/health-status`. Swagger: `/swagger`.

### Dev credentials (LocalDB only — never used in production)

- Seeded admin: `admin` / `1q2w3E*`
- Public OAuth client (password/authorization_code/refresh, no secret): `Wathiq_App`

### Getting a token / registering (verified in step 1.1)

```bash
# token for the seeded admin
curl -sk -X POST https://localhost:44352/connect/token \
  -d "grant_type=password&username=admin&password=1q2w3E*&client_id=Wathiq_App&scope=Wathiq offline_access"

# self-register a new resident
curl -sk -X POST https://localhost:44352/api/account/register -H "Content-Type: application/json" \
  -d '{"userName":"jane","emailAddress":"jane@example.com","password":"P@ssw0rd!23","appName":"Wathiq_App"}'
```

## Gotchas (from step 1.1)

- `abp new` requires an **empty** output folder — generate into a scratch directory and move the
  result in, rather than pointing `-o` at `backend/` directly.
- The `-cs` connection-string argument was embedded into `appsettings.json` with an unescaped
  single backslash (`(localdb)\MSSQLLocalDB`), which is **invalid JSON** — `.NET`'s config loader
  fails silently down to an empty connection string. Fixed by escaping it to `\\MSSQLLocalDB`.
- `Volo.Abp.AspNetCore.Mvc.Libs.AbpMvcLibsOptions.CheckLibs = false` is required for `-u no-ui`
  hosts — otherwise every request 500s because the host probes for a non-existent `wwwroot/libs`
  (a leftover check meant for MVC/Blazor UI templates).
