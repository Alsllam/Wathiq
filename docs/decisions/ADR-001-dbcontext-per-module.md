# ADR-001 — One DbContext and one SQL schema per module

**Date:** 2026-08-27 · **Status:** Accepted · **Phase:** 0 (applies from Phase 1)

## Context

The reference backend (`MoD.PoC.HousingProject`) separates modules into projects but maps every
entity in a single shared `MoDBenefitTrackDbContext` (`Shared/MoD.Framework.Infrastructure`),
grouped by `#region`. That makes cross-module joins trivial — and therefore common — so module
boundaries erode over time. PLAN §4 states the rule "no cross-module DB joins" but does not say
how it is enforced.

## Decision

Each Wathiq module (`Identity`, `Documents`, `Reminders`, `Guides`, `Ai`) owns:

- its own `DbContext` in `<Module>.EntityFrameworkCore`, mapping only its tables;
- its own SQL schema (`documents.*`, `reminders.*`, …) set via `modelBuilder.HasDefaultSchema`;
- its own migrations history (ABP supports multiple contexts on one connection string).

References to another module's rows are plain `Guid` columns with no foreign-key constraint.
Cross-module reads use the owning module's application-service interface (DTOs); reactions use
ABP local events.

## Consequences

- **+** A join across modules is impossible by construction — the compiler enforces the rule.
- **+** Modules can be extracted into services later without schema surgery.
- **−** No database-level referential integrity across modules; orphan checks are the owning
  service's job (and a periodic consistency job in Phase 7).
- **−** Reporting queries that span modules must be composed in code or in a read-only view
  created by a `Shared` migration.
- Documented in Architecture D10 and Database DB1/DB2.
