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

## Phase 1 — Backend core (ABP + SQL Server)  `active`  *(expand at start: step 1.0)*

ABP solution `Wathiq` (open-source modules only), `Documents` module end-to-end: `DocumentType`,
`Document` aggregate with `ExpiryDate` value object, EF migration on LocalDB, CRUD app service,
permissions, OpenAPI, seed data, one integration test. Update `srs`, `database`, `api` docs.
*Topics: ABP layering, aggregates/value objects, EF Core migrations, DTOs, permissions, xUnit.*

- [ ] 1.0 Expand phase into steps

## Phase 2 — Reminders & background jobs  *(expand at start)*

`Reminders` module, Hangfire recurring job, email channel (MailKit + local smtp4dev),
domain event `DocumentExpiryChanged` → reschedule. Tests for scheduling math.
*Topics: Hangfire, idempotent jobs, domain events, time zones, testing time.*

- [ ] 2.0 Expand phase into steps

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
