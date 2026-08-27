# backend — ABP modular monolith (.NET 10)

Created in Phase 1 (`step 1.x`). Nothing to build yet.

## Layout (target)

```
backend/
  Wathiq.sln
  src/
    Wathiq.Host/                 ASP.NET Core host: OpenIddict, Swagger, Hangfire, Serilog
    Wathiq.Shared/               IFileStore, IEncryptor, localization, audit
  modules/
    Identity/    Documents/    Reminders/    Guides/    Ai/
      Wathiq.<Module>.Domain/                entities, value objects, domain events
      Wathiq.<Module>.Application/           app services, DTOs, permissions
      Wathiq.<Module>.EntityFrameworkCore/   DbContext (one per module, own schema), migrations
      Wathiq.<Module>.HttpApi/               controllers (auto-API where ABP allows)
  test/
    Wathiq.<Module>.Tests/                   xUnit: domain rules + one happy-path per app service
```

Rules that matter here: one `DbContext` and one SQL schema per module (ADR-001); no
cross-module joins; provider SDK types stay inside `Ai`. See `docs/deliverables/architecture.md`.

## Database in dev

LocalDB, instance `MSSQLLocalDB`, database `Wathiq`:

```
Server=(localdb)\MSSQLLocalDB;Database=Wathiq;Trusted_Connection=True;TrustServerCertificate=True
```

Alternative (or on a machine without LocalDB): `docker compose up -d sql` from the repo root and
point the connection string at `localhost,14333` with the `sa` password from `.env`.
