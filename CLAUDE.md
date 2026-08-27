# Wathiq (وثيق) — Personal Documents & Deadlines Assistant (Learning Project)

Wathiq is a **real product built as a learning project**: a free, privacy-first service that helps
people keep track of their personal documents (ID, passport, license, vehicle registration,
insurance, contracts…), reminds them before expiry, and uses a **free/self-hosted AI** to extract
document data and answer "how do I renew X?" questions in Arabic and English.

The developer (Abdulsalam) is an experienced Angular/Nx + ABP developer. Every step is
simultaneously a piece of the product **and** a lesson in one of the target topics below.
Target: publish it as a community service run by one person.

## The three applications + AI

| App | Path | Stack | Learning focus |
| --- | --- | --- | --- |
| `backend/` | ABP modular monolith, .NET 10, EF Core, SQL Server, Hangfire | Clean modular design, EF migrations, background jobs, AI integration |
| `frontend/` | Nx + Angular 20 (standalone, signals, zoneless-ready), `wathiq_portal` + `wathiq_admin` | Signals, new control flow, RTL/i18n, Nx libs |
| `mobile/` | Flutter 3.x, Riverpod, go_router, Dio, Isar/Drift offline | Flutter from scratch (new language for the dev), offline-first, camera, push |
| AI | `Microsoft.Extensions.AI` + Ollama (local) with Groq/Gemini free tiers as swappable providers | OCR, extraction, RAG, embeddings in SQL Server, guardrails |

Reference repos (**read-only**, mine for patterns — never modify):
- Angular: `d:\Projects\MoD.HousingProject.Frontend`
- Backend: `D:\Projects\MoD.PoC.HousingProject` (modules: Identity, FileManagment, Notifications, …)

Master plan: [docs/PLAN.md](docs/PLAN.md). Progress: [ROADMAP.md](ROADMAP.md).
Deliverable documents (SRS, architecture, …) live in `docs/deliverables/` as Markdown sources and
are rendered to `.docx` with `/make-doc`.

## The workflow — non-negotiable rules

1. **One roadmap step = one commit.** Never bundle two steps; never commit a half-done step.
2. **Every step ships a learning doc** at `docs/learning/<step-id>-<slug>.md`, following
   [docs/learning/TEMPLATE.md](docs/learning/TEMPLATE.md). It names the topics applied and what
   was new for this developer. Written *with* the code, not retrofitted.
3. **Commit message convention:**
   ```
   step(1.3): add Document aggregate and EF migration

   Learned: ABP aggregate roots, value objects, EF Core owned types
   ```
   First line `step(<id>): <what>`; body starts with `Learned:`.
   Checkpoints: `checkpoint(<phase>): passed`. Documents: `doc(<name>): <what>`.
4. **Teach while building.** Explain the *why* at the line where a new idea appears, in terms an
   Angular/ABP developer maps to instantly. Small readable diffs over large generated blobs.
   For Flutter (new to the dev) go slower: introduce one concept per step.
5. **ROADMAP.md is the source of truth.** Tick the box in the same commit. Phases are expanded
   into commit-sized steps just-in-time (step `N.0` of each phase).
6. **Checkpoints gate phases** via `/checkpoint`: the user answers from memory before seeing the
   answer.
7. **Documents are part of the product.** Each phase updates the relevant deliverable(s) in
   `docs/deliverables/` (SRS grows with features; DB doc grows with migrations).

## Stack decisions (locked — rationale in docs/PLAN.md §6)

Backend: .NET 10 · ABP Framework (open-source modules only) · EF Core · SQL Server (LocalDB in
dev, SQL Server Express/Docker in prod) · Hangfire · Serilog · OpenAPI · xUnit.
AI: `Microsoft.Extensions.AI` abstractions · Ollama (`qwen2.5:7b`, `qwen2.5vl`, `bge-m3`) ·
Tesseract (ara+eng) OCR · providers switchable by config (Ollama / Groq / Gemini / OpenAI-compatible).
Web: Nx 21 · Angular 20 standalone · signals · Tailwind 4 (logical properties) · Transloco or
existing i18n approach · Jest/Vitest · Playwright.
Mobile: Flutter 3.x · Riverpod · go_router · Dio · Drift (SQLite) · Firebase Messaging (push) ·
camera + image_picker · flutter_secure_storage.
Docs: Markdown → `.docx` via Pandoc (reference template in `docs/deliverables/_template/`).

## Code guardrails

- **Privacy first.** Document images and extracted fields never leave the server to a cloud AI;
  only Ollama processes them. Cloud free tiers are allowed only for the public "guides" chat.
  Every AI call is logged in `ai.Usage` with a per-user daily cap.
- **AI behind an interface.** All model calls go through `IChatClient`/`IEmbeddingGenerator`;
  no provider SDK types leak into application services. Prompts live in versioned files.
- **Validate AI output.** Extracted dates/numbers are re-validated with regex/parsers; the user
  always confirms before save.
- **Modular monolith.** Modules: `Identity`, `Documents`, `Reminders`, `Guides`, `Ai`, `Shared`.
  No cross-module DB joins; communicate via application services / local events.
- **Migrations are reviewed.** Read every generated migration before applying; name them.
- **Arabic-first, RTL-safe UI:** logical CSS properties only (`ms-`/`me-`/`text-start`);
  i18n keys from the first commit of every screen (ar + en).
- **No secrets in git.** `appsettings.*.json` secrets via user-secrets / env vars; `.env` ignored.
- **Tests where they pay:** domain rules (expiry calc, reminder schedule), AI output parsers,
  and one happy-path integration test per app service.

## Skills in this repo

- `/next-step` — implement the next unchecked ROADMAP step, write its learning doc, commit.
- `/checkpoint` — run the active phase's quiz; on pass, close the phase and commit.
- `/make-doc <name>` — build/update a deliverable (`srs`, `architecture`, `database`, `api`,
  `ai-safety`, `privacy`, `user-guide`, `vision`) from Markdown to `.docx` and commit `doc(...)`.
- `/learn <topic>` — a focused explanation of a topic from the plan, anchored to this repo's code.

## Environment notes

- Windows 11, PowerShell primary. .NET 10.0.400, Node 22.16, Flutter 3.41.7, Docker Desktop,
  SQL Server LocalDB (`MSSQLLocalDB`) + sqlcmd, Python 3.14 (python-docx), Pandoc.
- Ollama is **not installed yet** — installing it and pulling models is a roadmap step.
- Git: local only, branch `main`, commit directly. No remote until the "publish" phase.
