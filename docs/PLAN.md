# Wathiq — Master Plan

## 1. Vision

**Problem.** People miss renewal deadlines for IDs, passports, licenses, vehicle registrations,
insurance policies, contracts, and permits — paying fines, losing time, and scrambling to find
which documents a renewal needs.

**Product.** Wathiq (وثيق, "trusted / documented") is a free, Arabic-first, privacy-first personal
documents vault: photograph a document → AI reads it → you confirm → Wathiq reminds you before it
expires and tells you exactly how to renew it. Works for you and your family members.

**Operator.** One person. No institution onboarding, no payments, no user-to-user content.
Runs on one small server with self-hosted AI. Cost target: under $20/month.

**Success.** 1,000 active users in the first 6 months after publish; open-source core; at least
one write-up or talk in an Arabic developer community.

## 2. Users and core use cases

Actors: **Resident** (end user), **Family manager** (resident managing dependants' documents),
**Admin** (the operator, curating guides and monitoring AI usage).

1. Add a document (photo/PDF/manual) → AI extracts type, holder, number, issue/expiry → confirm.
2. See a timeline of upcoming expiries; get push/email reminders (90/30/7/1 days, configurable).
3. Ask "how do I renew my driving license?" → answer grounded in a curated **Guide** (steps,
   documents, fees, where, last-verified date) with citations.
4. Manage family members and their documents; share a document link temporarily.
5. Export / delete all my data (privacy promise).
6. Admin: author guides, review AI extraction failures, watch usage caps and costs.

## 3. Phases, learning goals and checkpoint questions

| # | Phase | Product outcome | Learning goals | Checkpoint question |
| --- | --- | --- | --- | --- |
| 0 | Bootstrap & docs foundation | Repo, plan, Vision doc, SRS v0.1, ERD v0.1 as `.docx` | Requirements engineering; Markdown→Word pipeline; modular monolith boundaries on paper | Name the six modules and the one rule that keeps them decoupled. |
| 1 | Backend core (ABP + SQL Server) | ABP solution, `Documents` module: aggregate, EF migration, CRUD app service, OpenAPI | ABP layering (Domain/Application/EF/HttpApi), aggregate roots & value objects, EF Core migrations, DTO mapping, permissions | Why is `ExpiryDate` a value object with validation instead of a nullable DateTime on the entity? |
| 2 | Reminders & jobs | `Reminders` module, Hangfire recurring job computes due reminders; email channel | Background jobs, idempotency, domain events, time-zone-safe date math, unit tests for scheduling | How do you make the nightly reminder job safe to run twice? |
| 3 | AI: OCR + extraction (local) | Ollama installed; `Ai` module; upload → Tesseract → LLM extraction → validated JSON → confirm | `Microsoft.Extensions.AI`, structured output, prompt versioning, output validation, usage caps | The model returns an expiry date of 30/02/2027 — where is it caught? |
| 4 | Angular portal | `wathiq_portal`: auth, documents list/detail, add-document wizard, timeline; ar/en RTL | Angular 20 signals, `@if/@for`, standalone, signal forms, Nx lib boundaries, Tailwind logical properties | When do you use `computed` vs `effect`, and which one should you almost never need? |
| 5 | Guides + RAG | `Guides` module, admin authoring, embeddings in SQL Server, grounded chat with citations | Chunking, embeddings, cosine search in SQL, prompt grounding, hallucination controls, eval set | Why store the chunk's `GuideVersion` alongside its embedding? |
| 6 | Flutter app | Resident app: login, documents, camera capture → upload, reminders, offline queue, push | Dart basics, widgets & layout, Riverpod state, go_router, Dio + interceptors, Drift offline, FCM | Explain the difference between `StatelessWidget`, `StatefulWidget`, and a Riverpod `Notifier`. |
| 7 | Admin app & ops | `wathiq_admin`: guides editor, AI failures queue, usage dashboard; Serilog + Seq; Docker Compose | Observability, Docker Compose for API+SQL+Ollama+Hangfire, backups, health checks | What three signals would tell you AI extraction quality dropped this week? |
| 8 | Hardening & privacy | Encryption at rest for files, export/delete-my-data, rate limits, security review, tests | Data protection, threat modelling, OWASP basics, ABP data filters | Where is a document image decrypted, and who can trigger that path? |
| 9 | Publish | Landing page, Privacy/Terms, app-store listing, deploy to VPS, GitHub public, announcement | Release management, semantic versioning, store review, community launch | What is in the day-1 incident runbook? |
| 10 | Future | Family sharing, e-signature, voice, national-ID integrations, multi-tenant for charities | — | — |

Phases 4 and 6 may run interleaved with 5 (frontend work while AI quality is tuned).

## 4. Architecture (target)

```
 Flutter app ─┐                          ┌─ Ollama (qwen2.5:7b, qwen2.5vl, bge-m3)
 Angular portal├─ HTTPS ─► ABP API ──────┤─ Tesseract (ara+eng)
 Angular admin ┘           │             └─ Groq/Gemini free tier (guides chat only, no PII)
                           ├─ SQL Server: identity.*, documents.*, reminders.*, guides.*, ai.*
                           ├─ Hangfire: reminders, OCR queue, re-embedding
                           └─ File storage: encrypted blobs on disk (S3-compatible later)
```

Modules (`backend/modules/<Name>`, each with Domain / Application / EntityFrameworkCore / HttpApi):

- **Identity** — ABP Identity (users, roles, family links).
- **Documents** — document types, documents, holders, attachments, extraction results.
- **Reminders** — reminder rules, scheduled reminders, delivery log (email/push).
- **Guides** — guides, steps, versions, chunks + embeddings, feedback ("outdated?").
- **Ai** — provider routing, prompts, usage/caps, evaluation samples.
- **Shared** — cross-cutting: file storage abstraction, localization, audit.

Rule: **no cross-module DB joins**; modules talk through application services and local events.

## 5. Data model v0.1 (grows in the Database doc)

- `documents.DocumentType` (code, name ar/en, default validity, renewal guide link)
- `documents.Document` (owner, holder, type, number, issueDate, expiryDate, status, notes)
- `documents.Attachment` (document, blob key, mime, ocrText, encrypted)
- `documents.ExtractionResult` (attachment, provider, promptVersion, json, confidence, accepted)
- `reminders.ReminderRule` (user, offsets, channels, quiet hours)
- `reminders.Reminder` (document, dueAt, channel, status, sentAt)
- `guides.Guide` / `GuideVersion` / `GuideStep` / `GuideChunk` (text, embedding, version)
- `ai.Usage` (user, provider, model, tokensIn/Out, purpose, at) · `ai.Prompt` (name, version, body)

## 6. Stack decisions and rationale

| ID | Decision | Why |
| --- | --- | --- |
| D1 | ABP modular monolith, not microservices | Solo operator; one deployable; same layering as the reference backend |
| D2 | SQL Server incl. embeddings | LocalDB free in dev, Express free in prod; vectors as `VARBINARY` + cosine in code until SQL Server 2025 `VECTOR` is available |
| D3 | `Microsoft.Extensions.AI` over Semantic Kernel first | Smaller surface; provider swap is config; SK can sit on top later for RAG orchestration |
| D4 | Ollama + Qwen 2.5 family | Best free Arabic quality at 7B; vision variant for documents; fully private |
| D5 | Tesseract before vision-LLM | Deterministic and cheap; the LLM only structures text. Vision model as fallback |
| D6 | Angular 20 signals, standalone, no NgModules | Modern Angular the dev wants to master; contrast with the reference repo |
| D7 | Flutter + Riverpod + go_router + Drift | Mainstream, well-documented choices for a first Flutter app; offline-first fits photos on bad networks |
| D8 | Docs as Markdown → Pandoc `.docx` | Versionable, diffable, regenerable; Word output for stakeholders |
| D9 | Local git only until Phase 9 | Learning cadence first; publish once privacy features exist |

## 7. Deliverable documents

| Key | File | Grows in phase |
| --- | --- | --- |
| `vision` | Vision & Project Charter | 0 |
| `srs` | Software Requirements Specification (IEEE 830 style, ar+en glossary) | 0 → every phase |
| `architecture` | Software Architecture Document (C4 L1–L3, module map, flows) | 1, 3, 5, 7 |
| `database` | Database Design (ERD, dictionary, indexes, migrations log) | 1, 2, 5 |
| `api` | API Specification (OpenAPI export + narrative) | 1 → |
| `ai-safety` | AI Assistant Design, Safety & Privacy | 3, 5, 8 |
| `privacy` | Privacy Policy & Terms (ar+en) | 8, 9 |
| `user-guide` | User Manual (web + mobile) | 4, 6 |
| `test-plan` | Test Plan & Deployment Guide | 7, 8 |

## 8. Reading list (mapped to phases)

- ABP docs: Module development, Domain-driven design guide (P1–2)
- EF Core: Migrations, owned types, value conversions (P1)
- Hangfire docs: recurring jobs, idempotency patterns (P2)
- Microsoft Learn: `Microsoft.Extensions.AI` overview; Ollama docs; Qwen model cards (P3, P5)
- Angular.dev: Signals guide, control flow, zoneless (P4)
- Flutter docs: "Flutter for web developers", Riverpod docs, go_router, Drift (P6)
- OWASP ASVS (lite), Microsoft Data Protection APIs (P8)
