---
title: "Wathiq — AI Safety & Guardrails"
subtitle: "وثيق — سلامة الذكاء الاصطناعي وضوابطه"
author: "Abdulsalam"
version: "0.2"
date: "2026-08-29"
status: "Draft"
---

# Document control {-}

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 0.1 | 2026-08-29 | Abdulsalam | First version: routing, prompts, validation, caps, eval method (roadmap step 3.8). Live eval results pending the first dev-box run |
| 0.2 | 2026-09-07 | Abdulsalam | §8 Grounded chat (RAG): grounding chain, citation validation, refusal paths, RAG eval method (roadmap step 5.6) |

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

# 8. Grounded chat (RAG) — FR-GDE-002/003, FR-AI-003

The guides chat answers **only from retrieved, published guide content**. The safety chain, in
order, with the layer that enforces each link:

1. **Corpus scope** (`GuideRetriever`): only *served* content is searchable — active guides,
   latest published version per language. Drafts and superseded versions are never candidates
   (their chunks remain in SQL so past citations stay resolvable). Chunks embedded by a
   different model than the current one are invisible, not low-scoring — vectors from different
   models share no space.
2. **Similarity floor** (config `Guides:Retrieval`, default 0.5): below it, the corpus "does
   not speak to this question" and the pipeline refuses before any chat-model call. An empty
   corpus refuses before even the *embedding* call.
3. **Label isolation** (`ChatAppService` → `IGuideAnswerer`): the model receives excerpts as
   opaque labels `C1..Cn` with text — never real ids. It cannot leak, guess or mangle a chunk
   or version id, and an invented label is trivially detectable.
4. **Versioned grounding prompt** (`guides-chat@v1`, embedded resource, pinned name): answer
   only from excerpts, refuse with `answer: null` otherwise, cite every excerpt used. The
   version string rides into every `ai.Usage` row (purpose `GuideChat`) via the 3.3 decorator —
   the cap applies, which is why chat requires sign-in while reading stays anonymous.
5. **Parse defensively** (`GuideAnswerParser`): prose, torn JSON, citation noise — every
   malformed output degrades to a refusal, never an exception, never a served answer.
6. **Citation validation** (`ChatAppService`): cited labels are mapped back against the set
   retrieved *for this request*. Invented citations are dropped and flagged
   (`hallucinatedCitationsDropped`); an answer left with **zero** valid citations is refused
   whole — grounding is definitional, not decorative.
7. **Freshness on every answer** (Vision R2): each citation carries its source version's
   `LastVerifiedAt`; the answer-level date is the **oldest** cited source — an answer is only
   as fresh as its stalest ingredient.
8. **Reader feedback** (`GuideFeedback`): any reader — anonymous included — can flag a version
   as outdated/wrong; flags land in an admin queue (list + resolve). The correction loop for
   everything the layers above cannot know.

**RAG evaluation method** (the §6 philosophy at conversation level): a 10-case bilingual set of
grounded Q&A pairs — 6 answerable from the seeded guide, **4 must-refuse** (off-topic and
guides-that-don't-exist-yet). A refusal case scores a point only for silence: a set without
must-refuse cases cannot distinguish a grounded assistant from a confident liar. The
`[OllamaFact]`-gated runner (`Rag_Eval_Scores_The_Live_Pipeline`) chunks and embeds the real
seeded guide with the production chunker + bge-m3, retrieves with the production math and
defaults, answers through the real `GuideAnswerer`, and replicates the service's citation
policy. Metrics: answer rate, refusal accuracy, clean-citation rate — floors at 50% reject a
broken pipeline; record per prompt version here after each dev-box run
(`WATHIQ_OLLAMA_SMOKE=1 dotnet test --filter Rag_Eval`). **Results:** *pending the first
dev-box run.*

# 9. Planned hardening {-}

- P8: encryption at rest for attachments and `OcrText`/`RawJson`; purge of extraction PII 90
  days after acceptance; hard-delete flow.
- Phase 5: guides RAG grounding + citation checks (its own section here when built).
- Baseline-pinned eval threshold once ≥2 prompt versions exist.
