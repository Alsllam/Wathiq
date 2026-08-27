---
name: make-doc
description: Create or update one of the project's deliverable documents (vision, srs, architecture, database, api, ai-safety, privacy, user-guide, test-plan) as Markdown, render it to .docx with Pandoc, and commit it as a doc(...) commit. Use when the user invokes /make-doc <key> or asks for the SRS / architecture / Word document.
---

# make-doc

Produce a professional, bilingual-aware Word deliverable from a Markdown source.

## Inputs

`/make-doc <key> [section or change request]`. Keys and their purpose are in `docs/PLAN.md` §7.
Source: `docs/deliverables/<key>.md`. Output: `docs/deliverables/out/<Key>.docx`.
Template and build script: `docs/deliverables/_template/` (created in roadmap step 0.2; if it
does not exist yet, tell the user to run that step first).

## Procedure

1. **Read what exists**: the current `<key>.md` (if any), `docs/PLAN.md`, `ROADMAP.md`, and the
   code that the document describes (entities for `database`, controllers/app services for `api`,
   prompts and `Ai` module for `ai-safety`). Documents must describe the **actual** repo state —
   never invent endpoints or tables that do not exist; mark planned items as *Planned*.
2. **Structure** — every document has:
   - YAML front matter: `title`, `subtitle` (Arabic title), `author`, `version`, `date`, `status`
     (Draft / Review / Approved).
   - A revision history table (append a row per change; version bumps: patch for wording,
     minor for new sections, major for scope change).
   - Numbered headings; requirement/entity IDs that are stable (`FR-DOC-003`, `NFR-PRV-001`,
     `E-Document`); Mermaid diagrams as fenced blocks **plus** a rendered PNG/SVG in
     `docs/deliverables/assets/` (Pandoc does not render Mermaid — use `mmdc` if available,
     otherwise ASCII/PlantUML fallback and say so).
   - A bilingual glossary section (Arabic ↔ English) in `srs`, `privacy`, `user-guide`.
   Per-key outlines:
   - `vision`: problem, target users, product principles, scope & non-goals, success metrics,
     operating model (solo), risks, roadmap summary.
   - `srs` (IEEE 830): introduction, overall description, actors, use cases with pre/post
     conditions, functional requirements per module, non-functional (privacy, security,
     performance, i18n/RTL, accessibility, offline), external interfaces, traceability matrix.
   - `architecture`: C4 context/container/component, module map & boundaries, key flows
     (add document, reminder run, RAG answer), decisions log (from PLAN §6 + `docs/decisions/`).
   - `database`: schema-per-module ERD, data dictionary per table, indexes, migrations log,
     retention & encryption notes.
   - `api`: auth, conventions, endpoints per module (from OpenAPI), error model, examples.
   - `ai-safety`: providers & routing, prompts (versioned), data handling/privacy boundaries,
     validation & guardrails, usage caps, evaluation method & results, failure modes.
   - `privacy`: privacy policy + terms, ar and en sections.
   - `user-guide`: task-based, screenshots when apps exist.
   - `test-plan`: strategy, levels, environments, deployment steps, rollback, runbook.
3. **Write/update the Markdown.** Clear, specific, testable wording. Keep Arabic text in
   RTL-marked paragraphs (the template's `rtl` custom style: `::: {custom-style="RTL"}`).
4. **Render** with the build script (`docs/deliverables/_template/build.ps1 <key>` or the
   documented pandoc command). Open nothing; just verify the file exists and is non-trivial in
   size. Report any Pandoc warnings.
5. **Commit**: `doc(<key>): <what changed>` — stage the `.md`, assets, and the `.docx`.
6. **Hand back**: path of the `.md` and `.docx`, version number, and what is still marked
   *Planned* so the next phase knows what to fill in.

## Quality bar

- No lorem-ipsum, no placeholder sections without a *Planned* marker.
- Requirements use "shall"; each has an ID, a priority (M/S/C), and a source (use case or
  roadmap step).
- Diagrams match the code and the module rule (no cross-module joins).
