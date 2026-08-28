# Roadmap

Source of truth for progress. One step = one commit = one learning doc.
Legend: `[ ]` todo · `[x]` done · each step lists the **topics it teaches**.
Phases beyond the active one stay coarse; the first step of each phase (`N.0`) expands it.

---

## Phase 0 — Bootstrap & docs foundation  `done`

- [x] **0.1 Repo bootstrap** — git init, CLAUDE.md, skills, plan, roadmap, learning template.
      *Topics: learning-by-building workflow; modular monolith on paper.*
- [x] **0.2 Docs pipeline** — `docs/deliverables/_template/` (Pandoc reference `.docx` with
      Arabic-capable fonts, RTL paragraph support), `make-doc` script, render a smoke-test doc.
      *Topics: Pandoc Markdown→docx, reference templates, bilingual documents.*
- [x] **0.3 Vision & Charter doc** — `docs/deliverables/vision.md` → `.docx`: problem, users,
      scope, non-goals, success metrics, operating model (solo), roadmap summary.
      *Topics: project charter, scope/non-goals discipline.*
- [x] **0.4 SRS v0.1** — actors, use cases (UC-01…UC-06), functional reqs (FR-xx) for
      Documents/Reminders/Guides/AI, non-functional (privacy, performance, i18n), glossary ar/en.
      *Topics: IEEE 830 structure, testable requirement wording, traceability IDs.*
- [x] **0.5 Architecture & DB v0.1** — C4 context + container diagrams (Mermaid), module map,
      ERD v0.1 → `architecture.docx`, `database.docx`.
      *Topics: C4 model, modular monolith boundaries, ERD notation, schema-per-module.*
- [x] **0.6 Repo layout & tooling** — `backend/`, `frontend/`, `mobile/` folders with READMEs,
      `.editorconfig`, `.gitattributes`, root `README.md`; Docker Compose skeleton (SQL Server only).
      *Topics: monorepo layout for 3 apps, Docker Compose basics.*
- [x] **0.CP Checkpoint** — "Name the six modules and the one rule that keeps them decoupled."

## Phase 1 — Backend core (ABP + SQL Server)  `steps done · 1.CP pending`

ABP solution `Wathiq` (open-source modules only), `Documents` module end-to-end: `DocumentType`,
`Document` aggregate with `ExpiryDate` value object, EF migration on LocalDB, CRUD app service,
permissions, OpenAPI, seed data, one integration test. Update `srs`, `database`, `api` docs.
*Topics: ABP layering, aggregates/value objects, EF Core migrations, DTOs, permissions, xUnit.*
Entities and fields below follow `docs/deliverables/database.md` v0.1 exactly — no re-deriving
the schema mid-phase.

- [x] 1.0 Expand phase into steps
- [x] **1.1 Scaffold the ABP solution** — `dotnet new abp` (or ABP CLI) app template: `Wathiq.sln`,
      `Wathiq.Host` (OpenIddict, Swagger, Serilog), Identity module working end-to-end against
      LocalDB (register, log in, get a token). No custom modules yet — this proves the shell boots.
      *Topics: ABP application template, host project composition, LocalDB connection string.*
- [x] **1.2 `Shared` module skeleton** — `IFileStore` abstraction + local-disk implementation
      (unencrypted; a `// TODO(P8)` marks where encryption plugs in per DB1/NFR-SEC-001),
      ABP localization sources for `ar`/`en`. No entities — pure cross-cutting services consumed
      by later modules. *Topics: cross-cutting module with no aggregates, ABP virtual file
      system, localization resources.*
- [x] **1.3 `Documents` module skeleton** — the four projects (`Domain`, `Application`,
      `EntityFrameworkCore`, `HttpApi`) under `backend/modules/Documents/`, registered as an ABP
      module in the host, its own `DocumentsDbContext` mapped to schema `documents` (ADR-001),
      first (empty) migration applied to LocalDB. *Topics: ABP module system & dependency graph,
      DbContext-per-module in practice, first EF Core migration.*
- [x] **1.4 `DocumentType` and `Holder` entities** — aggregate roots (`FullAuditedAggregateRoot`),
      EF configuration classes, migration, a data seed contributor for the document-type
      catalogue (ar/en names, default validity) and each user's default `Holder` (self).
      *Topics: ABP aggregate root base classes, EF `IEntityTypeConfiguration`, ABP data seeding
      contributors.* — FR-DOC-001, FR-DOC-007.
- [x] **1.5 `Document` aggregate with `ExpiryDate` value object** — `Document` (owner, holder,
      type, number, issue/expiry dates, status, notes) with `ExpiryDate` as an owned value type
      that rejects an expiry before the issue date; `Attachment` stored via `Shared.IFileStore`;
      migration. *Topics: value objects vs. primitive obsession, EF Core owned types / value
      conversions, aggregate-internal collections.* — FR-DOC-002, FR-DOC-003, FR-DOC-004.
- [x] **1.6 Application services, DTOs and permissions** — `DocumentTypeAppService` (read),
      `HolderAppService` and `DocumentAppService` (CRUD) with request/response DTOs and object
      mapping; ABP permission definitions gating each action. *Topics: ABP application services,
      DTO mapping (AutoMapper or manual), the ABP permission system.*
- [x] **1.7 OpenAPI surface** — confirm ABP's auto API controllers expose the Documents endpoints
      correctly in `swagger.json`; adjust route/group names for clarity; write `api.md` v0.1 from
      the real generated spec. *Topics: ABP auto API controllers, OpenAPI/Swagger customization.*
      *Docs: `api`.*
- [x] **1.8 Tests, then close the loop on docs** — xUnit: a domain test for `ExpiryDate`
      validation and one integration test (ABP test host + LocalDB/Sqlite) exercising the full
      "create document → confirm it's stored" happy path. Flip the FR-DOC/FR-IDM rows this phase
      implements from *Planned* to *Implemented* in `srs.md`, and append this phase's migrations
      to the migrations log in `database.md`. *Topics: ABP integration testing (`AbpIntegratedTest`),
      xUnit, keeping deliverable status columns honest.* *Docs: `srs`, `database`.*
- [ ] 1.CP Checkpoint — "Why is `ExpiryDate` a value object with validation instead of a
      nullable DateTime on the entity?" *(deferred by user; run `/checkpoint` any time)*

## Phase 2 — Reminders & background jobs  `active`

`Reminders` module end-to-end: `ReminderRule` (offsets value object, channels, quiet hours,
time zone), `Reminder` + `DeliveryLog`, scheduling domain service, `DocumentExpiryChanged` local
event → reschedule, Hangfire nightly job (idempotent, FR-REM-002), email via MailKit + smtp4dev.
*Topics: Hangfire, idempotent jobs, domain events, time zones, testing time.*
Entities and fields follow `docs/deliverables/database.md` §schema `reminders` exactly — the
unique index `UQ_Reminder_DocumentId_OffsetDays` is the idempotency backbone, not an afterthought.

- [x] 2.0 Expand phase into steps
- [x] **2.1 `Reminders` module skeleton** — the four projects under `backend/modules/Reminders/`,
      `RemindersDbContext` on schema `reminders`, empty first migration, registered in the host
      **and** the DbMigrator in the same commit (the 1.7 lesson: every executable's module graph).
      *Topics: repeating the ABP module recipe unaided, per-executable module graphs.*
      *Docs: `database` (migrations log).*
- [x] **2.2 `ReminderRule` with the `ReminderOffsets` value object** — one rule per user
      (UQ UserId): offsets as a value object over CSV storage (EF value converter, e.g.
      `90,30,7,1`), `ReminderChannels` flags enum, quiet hours, IANA `TimeZoneId`; default rule
      materialised on first use (the self-holder pattern); migration.
      *Topics: value converters vs owned types, flags enums in EF, defaults-on-first-use.*
      — FR-REM-001. *Docs: `database` (migrations log + status).*
- [x] **2.3 `Reminder`, `DeliveryLog` and the scheduling math** — entities per DB doc with
      `UQ_Reminder_DocumentId_OffsetDays` and `IX_Reminder_Status_DueDate`; `ReminderScheduler`
      domain service computing due dates from expiry − offsets in the user's time zone, skipping
      past dates; pure domain tests for the math (year boundaries, `Asia/Riyadh` vs UTC).
      *Topics: time-zone-safe date math, domain services, testing time without the clock.*
      — FR-REM-001. *Docs: `database` (migrations log + status).*
- [x] **2.4 Reschedule via local events** — `DocumentExpiryChangedEto` contract in `Shared`,
      published by `Documents` on create/update/renew/delete; `Reminders` handler upserts or
      cancels reminders. No project reference between the two modules.
      *Topics: ABP local event bus, module decoupling through event contracts.* — FR-REM-004.
- [x] **2.5 Hangfire nightly job** — Hangfire + SQL Server storage in the host, dashboard in dev,
      recurring job scanning `Pending` reminders with `DueDate <= today`; safe to run twice by
      design (unique index + status transition, no side effect before the state claim).
      *Topics: Hangfire recurring jobs, idempotent job design, at-least-once thinking.*
      — FR-REM-002.
- [x] **2.6 Email channel** — ABP MailKit emailing against smtp4dev (added to Docker Compose),
      bilingual ar/en reminder template, one `DeliveryLog` row per attempt, failures mark the
      reminder `Failed`; quiet hours defer delivery. *Topics: ABP `IEmailSender`/MailKit, email
      templates, smtp4dev as a free local SMTP sink.* — FR-REM-003 (email), FR-REM-005.
- [x] **2.7 App services and API** — `ReminderRuleAppService` (get/update own rule) and
      `ReminderAppService` (upcoming reminders list); permissions under `WathiqReminders`;
      plural routes under `/api/reminders/*`; api.md §reminders from the regenerated spec.
      *Topics: second lap on app services/permissions, now with an existing pattern to match.*
      *Docs: `api`.*
- [x] **2.8 Tests, then close the loop on docs** — integration tests: nightly job run twice on
      the same day sends once (FR-REM-002); changing an expiry reschedules (FR-REM-004); flip the
      FR-REM rows in `srs.md`; verify the migrations log. *Topics: testing background jobs
      deterministically, keeping deliverable statuses honest.* *Docs: `srs`, `database`.*
- [ ] 2.CP Checkpoint — "How do you make the nightly reminder job safe to run twice?"

## Phase 3 — AI: OCR + extraction (local, free)  *(expand at start)*

Install Ollama, pull `qwen2.5:7b`, `qwen2.5vl:7b`, `bge-m3`; Tesseract ara+eng; `Ai` module with
`IChatClient` routing; extraction prompt v1 with JSON schema; validators; usage caps; eval set of
10 synthetic sample documents. Update `ai-safety` doc.
*Topics: Microsoft.Extensions.AI, structured output, prompt versioning, validation, evals.*

- [ ] 3.0 Expand phase into steps

## Phase 4 — Angular portal  *(expand at start)*

Nx workspace, `wathiq_portal`, auth (OpenIddict from ABP), documents list/detail, add-document
wizard (upload → extraction review → confirm), expiry timeline, ar/en + RTL.
*Topics: signals, control flow, signal forms, resource/httpResource, Nx libs, Tailwind logical.*

- [ ] 4.0 Expand phase into steps

## Phase 5 — Guides + RAG  *(expand at start)*

`Guides` module, versions, chunking + `bge-m3` embeddings stored in SQL Server, cosine search,
grounded chat with citations, "outdated?" feedback, eval questions.
*Topics: RAG pipeline, chunking, embeddings in SQL, grounding, hallucination controls.*

- [ ] 5.0 Expand phase into steps

## Phase 6 — Flutter resident app  *(expand at start)*

Flutter project, Riverpod, go_router, auth, documents list, camera capture → upload, reminders
list, Drift offline queue, FCM push. One Dart concept per step.
*Topics: Dart, widgets, state, navigation, networking, offline, push.*

- [ ] 6.0 Expand phase into steps

## Phase 7 — Admin app & operations  *(expand at start)*

`wathiq_admin`: guides editor, extraction failures queue, usage dashboard; Serilog + Seq;
Docker Compose (API + SQL + Ollama + Hangfire); health checks; backup script.
*Topics: observability, Docker Compose, ops runbooks.*

- [ ] 7.0 Expand phase into steps

## Phase 8 — Hardening & privacy  *(expand at start)*

File encryption at rest, export/delete my data, rate limiting, security review, `privacy` doc.
*Topics: data protection, threat modelling, OWASP basics.*

- [ ] 8.0 Expand phase into steps

## Phase 9 — Publish  *(expand at start)*

Landing page, Privacy/Terms, VPS deploy, GitHub public, store listing, announcement.
*Topics: release management, community launch.*

- [ ] 9.0 Expand phase into steps
