---
title: "Wathiq — AI Safety & Guardrails"
subtitle: "وثيق — سلامة الذكاء الاصطناعي وضوابطه"
author: "Abdulsalam"
version: "0.1"
date: "2026-08-29"
status: "Draft"
---

# Document control {-}

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 0.1 | 2026-08-29 | Abdulsalam | First version: routing, prompts, validation, caps, eval method (roadmap step 3.8). Live eval results pending the first dev-box run |

**Status:** Draft · **Related:** SRS FR-AI-001…005 / FR-DOC-005 / C1, Architecture D5,
Database (`ai` schema, `documents.ExtractionResult`), API §4.5.

This document describes what the code **enforces today** — every claim names the mechanism that
enforces it. Aspirations are marked *Planned*.

# 1. Providers and routing (FR-AI-001, FR-AI-002)

- All model calls go through `Microsoft.Extensions.AI` abstractions (`IChatClient`); no provider
  SDK type crosses a module boundary. Two **keyed** clients exist: `"extraction"` and
  `"guides"`, each bound from the `Ai` configuration section.
- **The privacy wall is a boot guard, not a convention**: `AiOptions.Validate()` refuses to
  start the host if `Ai:Extraction.Provider` is anything but `ollama` (verified live in 3.3: a
  `groq` override kills the boot with an FR-AI-002 message). Personal-document text can
  therefore only ever reach the self-hosted model.
- The `"guides"` client (public how-to chat, Phase 5) *may* later point at a free cloud tier —
  it never sees document content by construction (separate keyed client, separate purpose).

# 2. Data handling and privacy boundary (C1)

| Data | Where it may go | Enforced by |
| --- | --- | --- |
| Attachment bytes | Local disk (`IFileStore`), local Tesseract process | OCR is a CLI child process (`TesseractOcrService`); no network client in its path |
| OCR text (`Attachment.OcrText`) | SQL Server + the `"extraction"` keyed client only | boot guard above; `IDocumentDataExtractor` is the only consumer |
| Extraction output | `documents.ExtractionResult.RawJson` (SQL) | append-only row; purge 90 days after acceptance is *Planned (P8)* |
| Guides questions (Phase 5) | Cloud free tier allowed | separate keyed client; never carries document data |

# 3. Prompts as versioned artifacts (FR-AI-005)

- `extract-document@v1` ships as an **embedded resource** with a pinned `LogicalName` — the
  binary and its prompt are inseparable, so the version the ledger records is provably the
  version that ran. A prompt edit = a new file + a new version constant, reviewable in a diff.
- The version id flows through `ChatOptions` into every `ai.Usage` row and is stored on every
  `ExtractionResult` — both ends of a call are attributable to an exact prompt.

# 4. Validation: the model is an untrusted client (FR-AI-003)

The prompt *asks* for clean JSON; the parsers *decide* what survives. Every rule exists twice —
once in the prompt (to make good output likely) and once in C# (to make bad output harmless):

- Dates: Arabic-Indic digits normalized, then strict `yyyy-MM-dd` via `TryParseExact` —
  impossible calendar dates (`2027-02-30`) and wrong formats die here.
- Numbers: allow-list regex (`A–Z a–z 0–9 space / - .`, ≤64 chars) — injection-shaped strings
  vanish rather than being escaped.
- Free text (holder name, kind): control characters stripped, length-bounded.
- Cross-field: expiry before issue ⇒ both dates dropped (cannot tell which is wrong).
- Every dropped value becomes a **user-facing warning** ("Expiry date '…' is not a valid date -
  dropped") so the review UI explains empty fields instead of hiding them.
- Nothing is written to a `Document` until the user confirms; the confirm endpoint re-runs
  normal domain validation (`ExpiryBeforeIssue` etc.). The AI's blast radius is one append-only
  table.

# 5. Usage caps and the ledger (FR-AI-004)

- `UsageTrackingChatClient` (a delegating decorator wrapped around **every** registered client)
  checks the per-user daily count **before** the call leaves the process and refuses with
  `Wathiq.Ai:DailyCapExceeded` (HTTP 403) at the cap — default 50 calls/user/UTC-day
  (`Ai:DailyCallCapPerUser`).
- Every call — success or failure — lands in `ai.Usage`: user, purpose, provider, model, tokens
  in/out, duration, prompt version. Ledger writes use their own transaction (`requiresNew`), so
  a rolled-back business operation still leaves its AI call on the books.
- End-to-end proof (3.8 test): the real extractor over the real decorator — call 2 of cap 1
  never reaches the model, and the ledger row carries `extract-document@v1`. This test also
  caught the ledger's `PromptVersion` column being too narrow for real ids (migration
  `WidenUsagePromptVersion`).

# 6. Evaluation method and results

- **Offline set:** 10 synthetic OCR documents (5 Arabic, 5 English — clean, noisy, mixed-digit,
  Hijri-date, and one non-document decoy), each labeled with the proposal a correct pipeline
  should emit. Nulls are labels too: proposing a value where the truth is "nothing readable"
  is scored as a miss, so hallucination costs points.
- **Metric:** field accuracy over (number, issue date, expiry date) = 30 graded fields.
- **Runner:** an `[OllamaFact]`-gated test (`Extraction_Eval_Scores_The_Live_Model`) that runs
  the real registered extractor per case and prints a per-case table + the total. Floor
  assertion: > 50% (rejects a broken pipeline; to be tightened to the recorded baseline once
  prompt versions are compared).
- **Results:** *Pending the first dev-box run* (this container has no model — the run is one
  command: `WATHIQ_OLLAMA_SMOKE=1 dotnet test --filter Extraction_Eval`). Record the score here
  per prompt version.
- **Online signal:** every `ExtractionResult` records Accepted / Edited / Rejected per prompt
  version — production ground truth accumulating as a side effect of the review UX.

# 7. Failure modes

| Failure | Behavior | User sees |
| --- | --- | --- |
| Ollama down | Health check reports **Degraded** (never Unhealthy — documents/reminders keep working); extraction returns 403 `ExtractionFailed`; a `Failed` result row is still recorded in its own transaction | Localized "try again later"; document unaffected |
| OCR not finished | 403 `ExtractionNotReady` | Localized "still reading, try shortly" |
| Daily cap reached | 403 `Wathiq.Ai:DailyCapExceeded` before any model call | Localized cap message |
| Model returns garbage | No fields proposed; warnings explain; raw kept for diagnosis | Empty proposal + reasons |
| Model returns bad values | Fields dropped by parsers, per §4 | Empty fields + warnings |

# 8. Planned hardening {-}

- P8: encryption at rest for attachments and `OcrText`/`RawJson`; purge of extraction PII 90
  days after acceptance; hard-delete flow.
- Phase 5: guides RAG grounding + citation checks (its own section here when built).
- Baseline-pinned eval threshold once ≥2 prompt versions exist.
