---
title: "Wathiq — Software Requirements Specification"
subtitle: "وثيق — مواصفات متطلبات البرمجيات"
author: "Abdulsalam"
version: "0.1.3"
date: "2026-08-27"
status: "Draft"
---

# Document control {-}

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 0.1 | 2026-08-27 | Abdulsalam | Initial SRS: actors, UC-01…UC-06, FR/NFR baseline, glossary (roadmap step 0.4) |
| 0.1.1 | 2026-08-27 | Abdulsalam | Phase 1 status flips: FR-IDM-001/002 and FR-DOC-001/002/003/006/007 *Implemented*, FR-DOC-004 *Partly implemented* (roadmap step 1.8) |
| 0.1.2 | 2026-08-28 | Abdulsalam | Phase 2 status flips: FR-REM-001/002/004/005 *Implemented*, FR-REM-003 *Partly implemented* (email done, push P6) (roadmap step 2.8) |
| 0.1.3 | 2026-08-29 | Abdulsalam | Phase 3 status flips: FR-DOC-005 and FR-AI-001…005 *Implemented* (roadmap step 3.8); AI safety measures now documented in the `ai-safety` deliverable |

**Status:** Draft · **Structure:** IEEE 830-1998 adapted · **Related:** Vision (`vision`),
Architecture (`architecture`), Database (`database`).

Conventions: requirements use *shall*; each has an ID, a priority (**M** must / **S** should /
**C** could), and a source (use case, Vision scope item or roadmap phase). Items marked *Planned*
are not yet implemented; the "Status" column is updated each phase.

# Introduction

## Purpose

This document specifies the functional and non-functional requirements of Wathiq v1 for the
operator, contributors and testers. It is the reference for acceptance at each roadmap checkpoint.

## Scope

Wathiq is a free, Arabic-first, privacy-first service that stores personal documents, extracts
their data with a self-hosted AI, reminds users before expiry, and answers renewal questions from
curated guides. Scope items S1–S7 and non-goals N1–N6 are defined in the Vision document and are
binding here.

## Definitions, acronyms

See the Glossary. Key terms: *Document*, *Holder*, *Guide*, *Extraction*, *Reminder*.

## References

- Vision & Project Charter v0.1 (`vision`)
- `docs/PLAN.md` §4 (architecture), §5 (data model v0.1)
- IEEE Std 830-1998

# Overall description

## Product perspective

A modular monolith (ABP / .NET) exposing an HTTPS API consumed by a web portal (Angular), a
mobile app (Flutter) and an admin app (Angular). AI runs on the same server (Ollama, Tesseract).
Modules: **Identity**, **Documents**, **Reminders**, **Guides**, **Ai**, **Shared**; modules never
join each other's tables.

## User classes

| Actor | Description | Typical device |
| --- | --- | --- |
| Resident | Registered user managing their own documents | Mobile app, web |
| Family manager | Resident who also manages holders (dependants) | Mobile app, web |
| Admin | The operator; curates guides, monitors AI and usage | Admin web app |
| System | Scheduled jobs (reminder run, OCR queue, re-embedding) | Server |

## Operating environment

Single VPS, Docker Compose: API (.NET 10), SQL Server Express, Ollama, Hangfire. Clients: modern
browsers (last 2 versions), Android 10+ / iOS 15+.

## Constraints

- C1 Personal data (document images, extracted fields) shall never leave the server to a cloud AI (Vision P1/N5).
- C2 Infrastructure cost shall stay under USD 20/month (Vision P4).
- C3 Open-source components only; no paid licences.
- C4 All UI text shall be available in Arabic and English with RTL-safe layout.

## Assumptions and dependencies

- A1 Users can photograph documents with a phone camera.
- A2 A 7B local model plus Tesseract is sufficient for v1 extraction quality (Vision R1).
- A3 Push notifications depend on Firebase Cloud Messaging (free tier, no personal data in payload).

# Use cases

## UC-01 Add a document

| Field | Content |
| --- | --- |
| Actor | Resident, Family manager |
| Source | Vision S1 |
| Preconditions | User is authenticated; at least one holder exists (self by default) |
| Main flow | 1. User chooses photo / PDF / manual. 2. For photo/PDF the system runs OCR and AI extraction and shows proposed fields (type, holder, number, issue date, expiry date) with confidence. 3. User edits/confirms. 4. System validates and saves the document and its attachment. |
| Alternative | 2a. Extraction fails or confidence is below threshold → the form opens empty with the image beside it. |
| Postconditions | Document stored with status *Active*; extraction result recorded as accepted/edited; reminders scheduled (UC-02). |

## UC-02 See expiries and receive reminders

| Field | Content |
| --- | --- |
| Actor | Resident, Family manager, System |
| Source | Vision S2 |
| Preconditions | At least one document with an expiry date |
| Main flow | 1. User opens the timeline sorted by expiry. 2. Nightly, the system computes due reminders per the user's rule (default 90/30/7/1 days). 3. Reminders are delivered by push and/or email. 4. User marks a document as renewed → new expiry, reminders rescheduled. |
| Alternative | 3a. Delivery fails → retried, then logged as failed and shown in-app. |
| Postconditions | Delivery log entry per reminder; no duplicate delivery for the same reminder. |

## UC-03 Ask how to renew

| Field | Content |
| --- | --- |
| Actor | Resident |
| Source | Vision S3 |
| Preconditions | A published guide matching the question exists |
| Main flow | 1. User asks in Arabic or English (free text or from a document's page). 2. System retrieves relevant guide chunks and answers with citations and the guide's *last verified* date. |
| Alternative | 2a. No relevant guide → the system says it has no verified guide and offers the guide list. It shall not answer from model knowledge alone. |
| Postconditions | Question, cited guide versions and usage recorded in `ai.Usage`. |

## UC-04 Manage family members and share a document

| Field | Content |
| --- | --- |
| Actor | Family manager |
| Source | Vision S4 |
| Preconditions | Authenticated |
| Main flow | 1. User adds a holder (name, relation, optional birth date). 2. Documents are added for that holder (UC-01). 3. User creates a temporary share link for one document (lifetime ≤ 7 days, revocable). |
| Postconditions | Holder and documents visible only to the owning user; share link logged with expiry. |

## UC-05 Export or delete my data

| Field | Content |
| --- | --- |
| Actor | Resident |
| Source | Vision S5 |
| Preconditions | Authenticated; re-authentication for delete |
| Main flow | 1. User requests export → system produces a ZIP (JSON + files) within 24 h and notifies. 2. User requests deletion → account and all data are deleted after a 7-day grace period. |
| Postconditions | Export logged; after deletion no personal data remains except aggregate usage counters. |

## UC-06 Administer guides and AI operations

| Field | Content |
| --- | --- |
| Actor | Admin |
| Source | Vision S6 |
| Preconditions | Admin role |
| Main flow | 1. Admin authors/edits a guide (steps, required documents, fees, location, last verified). 2. Publishing creates a new version and re-embeds chunks. 3. Admin reviews the extraction-failure queue and the usage dashboard (calls, tokens, cost estimate, cap hits). |
| Postconditions | Guide version history retained; failures annotated for the eval set. |

# Functional requirements

Columns: ID · Requirement · Priority · Source · Status.

## Identity (FR-IDM)

| ID | Requirement | Pri | Source | Status |
| --- | --- | --- | --- | --- |
| FR-IDM-001 | The system shall allow registration and login with email + password and issue OAuth2/OIDC tokens for web and mobile clients. | M | UC-01 | Implemented (1.1: OpenIddict, register + password/auth-code tokens) |
| FR-IDM-002 | The system shall support the roles *Resident* and *Admin*; every API shall be permission-checked. | M | UC-06 | Implemented (1.6: default `user` + `admin` roles, permission per action) |
| FR-IDM-003 | The system shall require re-authentication before account deletion. | M | UC-05 | Planned (P8) |

## Documents (FR-DOC)

| ID | Requirement | Pri | Source | Status |
| --- | --- | --- | --- | --- |
| FR-DOC-001 | The system shall maintain a catalogue of document types with Arabic and English names and a default validity period. | M | UC-01 | Implemented (1.4 catalogue + seed, 1.6 endpoint) |
| FR-DOC-002 | The system shall store a document with holder, type, number, issue date, expiry date, status and notes, owned by exactly one user. | M | UC-01 | Implemented (1.5 aggregate, 1.6 CRUD API) |
| FR-DOC-003 | The system shall reject an expiry date earlier than the issue date and any non-calendar date. | M | UC-01, PLAN P3 question | Implemented (1.5: `ValidityPeriod` value object) |
| FR-DOC-004 | The system shall store attachments (image/PDF) encrypted at rest and serve them only to the owner. | M | UC-01, C1 | Partly implemented (1.5 aggregate + 3.1 upload/download API + 3.5 OCR; encryption at rest P8) |
| FR-DOC-005 | The system shall record each AI extraction result (provider, prompt version, JSON, confidence, accepted/edited) linked to its attachment. | M | UC-01 | Implemented (3.6–3.7) |
| FR-DOC-006 | The system shall let a user mark a document as renewed by entering the new expiry date, keeping the previous one in history. | S | UC-02 | Implemented (1.6: renew endpoint keeps `PreviousExpiryDate`) |
| FR-DOC-007 | The system shall support holders (family members) per user; a document belongs to one holder. | M | UC-04 | Implemented (1.4 entity + self-holder rule, 1.6 CRUD API) |
| FR-DOC-008 | The system shall create revocable share links for a document with a maximum lifetime of 7 days. | C | UC-04 | Planned (P8) |

## Reminders (FR-REM)

| ID | Requirement | Pri | Source | Status |
| --- | --- | --- | --- | --- |
| FR-REM-001 | The system shall schedule reminders at configurable offsets before expiry, defaulting to 90, 30, 7 and 1 days. | M | UC-02 | Implemented (2.2 rule + offsets, 2.3 scheduling, 2.7 settings API) |
| FR-REM-002 | The nightly reminder job shall be idempotent: running it twice on the same day shall not send a reminder twice. | M | UC-02, PLAN P2 question | Implemented (2.3 unique index + status claim, 2.5 dispatch job; proven by run-twice tests and live) |
| FR-REM-003 | The system shall deliver reminders by email and push, honouring the user's channel choice and quiet hours. | M | UC-02 | Partly implemented (2.6: email channel + quiet hours, hourly dispatch; push P6) |
| FR-REM-004 | The system shall reschedule reminders whenever a document's expiry date changes. | M | UC-02 | Implemented (2.4: `DocumentExpiryChanged` local event → resync) |
| FR-REM-005 | The system shall keep a delivery log (channel, status, sent-at, error) per reminder. | S | UC-02 | Implemented (2.3 `DeliveryLog`, written per attempt by 2.5/2.6) |

## Guides (FR-GDE)

| ID | Requirement | Pri | Source | Status |
| --- | --- | --- | --- | --- |
| FR-GDE-001 | The system shall store guides with steps, required documents, fees, location, language and a *last verified* date, versioned on publish. | M | UC-06 | Planned (P5) |
| FR-GDE-002 | The system shall answer renewal questions only from retrieved guide content and shall cite the guide version used. | M | UC-03, Vision P6 | Planned (P5) |
| FR-GDE-003 | When no relevant guide exists the system shall say so instead of answering. | M | UC-03 | Planned (P5) |
| FR-GDE-004 | Users shall be able to flag a guide as outdated; flags are visible to the Admin. | S | UC-06, Vision R2 | Planned (P5) |

## AI (FR-AI)

| ID | Requirement | Pri | Source | Status |
| --- | --- | --- | --- | --- |
| FR-AI-001 | All model calls shall go through a provider-agnostic interface; the provider shall be selectable by configuration. | M | CLAUDE.md guardrail | Implemented (3.3) |
| FR-AI-002 | Extraction of personal documents shall use only the self-hosted provider. | M | C1 | Implemented (3.3, boot guard) |
| FR-AI-003 | Extracted dates and numbers shall be re-validated by parsers before being shown; invalid values shall be shown as empty with a warning. | M | UC-01, FR-DOC-003 | Implemented (3.6) |
| FR-AI-004 | Every AI call shall be logged (user, provider, model, tokens, purpose) and a per-user daily cap enforced. | M | Vision P1, R5 | Implemented (3.3, e2e-tested 3.8) |
| FR-AI-005 | Prompts shall be versioned files; the version used shall be stored with each result. | S | FR-DOC-005 | Implemented (3.6) |

## Data rights (FR-DR)

| ID | Requirement | Pri | Source | Status |
| --- | --- | --- | --- | --- |
| FR-DR-001 | The system shall export all of a user's data as a ZIP (JSON + files) within 24 hours of request. | M | UC-05 | Planned (P8) |
| FR-DR-002 | The system shall delete a user's account and all personal data after a 7-day grace period. | M | UC-05 | Planned (P8) |

# Non-functional requirements

| ID | Requirement | Pri | Source | Status |
| --- | --- | --- | --- | --- |
| NFR-PRV-001 | No document image or extracted field shall be transmitted to any third-party AI service. | M | C1 | Planned (P3) |
| NFR-PRV-002 | Cloud AI providers, when enabled, shall receive only public guide questions with no user identifiers. | M | Vision P1 | Planned (P5) |
| NFR-SEC-001 | Attachments shall be encrypted at rest; keys shall not be stored with the data. | M | Vision R4 | Planned (P8) |
| NFR-SEC-002 | The API shall rate-limit authentication and AI endpoints per user and per IP. | M | Vision R4 | Planned (P8) |
| NFR-PRF-001 | Document list and timeline responses shall complete within 500 ms at p95 for 1,000 documents per user. | S | UC-02 | Planned (P4) |
| NFR-PRF-002 | AI extraction of one page shall complete within 60 s on the target server. | S | UC-01 | Planned (P3) |
| NFR-I18N-001 | All user-facing text shall exist in Arabic and English; layouts shall use logical CSS properties and render correctly in RTL. | M | C4 | Planned (P4, P6) |
| NFR-A11Y-001 | Web apps shall meet WCAG 2.1 AA for contrast, keyboard navigation and labels. | S | — | Planned (P4) |
| NFR-OFF-001 | The mobile app shall queue captures and edits offline and sync when connected. | S | PLAN D7 | Planned (P6) |
| NFR-OPS-001 | The system shall expose health checks and structured logs; nightly backups shall be restorable. | M | Vision R3 | Planned (P7) |
| NFR-TST-001 | Domain rules (expiry validation, reminder schedule) and AI parsers shall have automated tests. | M | CLAUDE.md guardrail | Planned (P1–P3) |

# External interfaces

| Interface | Description | Phase |
| --- | --- | --- |
| REST / OpenAPI | JSON over HTTPS; documented in the API deliverable | 1 |
| Ollama HTTP API | Local chat / vision / embedding models via `Microsoft.Extensions.AI` | 3 |
| Tesseract | Local OCR (ara+eng) invoked in-process | 3 |
| SMTP | Email reminders (smtp4dev in dev) | 2 |
| Firebase Cloud Messaging | Push reminders; payload contains no document data | 6 |
| Optional cloud LLM (Groq / Gemini) | Guide chat only, disabled by default | 5 |

# Glossary (ar / en) {-}

| English | Arabic | Meaning |
| --- | --- | --- |
| Document | مستند | A personal record with an expiry date (ID, passport, licence, insurance, contract) |
| Document type | نوع المستند | Catalogue entry with names and default validity |
| Holder | صاحب المستند | The person a document belongs to (self or family member) |
| Owner | مالك الحساب | The registered user who manages the holder's documents |
| Expiry date | تاريخ الانتهاء | Date after which the document is invalid |
| Reminder | تذكير | A scheduled notification before expiry |
| Reminder rule | قاعدة التذكير | User's offsets, channels and quiet hours |
| Guide | دليل | Curated renewal procedure with a last-verified date |
| Extraction | استخراج | AI reading of document fields from an image/PDF |
| Confidence | درجة الثقة | Model's self-reported certainty for an extracted field |
| Citation | استشهاد | Reference to the guide version an answer is based on |
| Share link | رابط مشاركة | Temporary, revocable link to one document |
| Usage cap | حد الاستخدام | Maximum AI calls per user per day |

::: {custom-style="RTL"}
ملاحظة: المصطلحات العربية أعلاه هي المعتمدة في واجهات المستخدم وملفات الترجمة، ويجب عدم استخدام
مرادفات أخرى لها في الشاشات أو الأدلة.
:::

# Traceability matrix {-}

| Vision item | Use cases | Requirements |
| --- | --- | --- |
| S1 Add document + extraction | UC-01 | FR-DOC-001…005, FR-AI-001…005, NFR-PRF-002 |
| S2 Timeline + reminders | UC-02 | FR-DOC-006, FR-REM-001…005, NFR-PRF-001 |
| S3 Renewal chat | UC-03 | FR-GDE-001…003, NFR-PRV-002 |
| S4 Family + share | UC-04 | FR-DOC-007, FR-DOC-008 |
| S5 Export / delete | UC-05 | FR-IDM-003, FR-DR-001, FR-DR-002 |
| S6 Admin | UC-06 | FR-IDM-002, FR-GDE-001, FR-GDE-004, FR-AI-004 |
| S7 Three apps | all | NFR-I18N-001, NFR-A11Y-001, NFR-OFF-001 |
| P1 / N5 Privacy | UC-01, UC-03 | C1, FR-AI-002, NFR-PRV-001, NFR-PRV-002, NFR-SEC-001 |
| R3 Solo operator | — | NFR-OPS-001 |
