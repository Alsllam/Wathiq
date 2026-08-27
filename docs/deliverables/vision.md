---
title: "Wathiq — Vision & Project Charter"
subtitle: "وثيق — الرؤية وميثاق المشروع"
author: "Abdulsalam"
version: "0.1"
date: "2026-08-27"
status: "Draft"
---

# Document control

| Version | Date | Author | Change |
| --- | --- | --- | --- |
| 0.1 | 2026-08-27 | Abdulsalam | First complete draft (roadmap step 0.3) |

**Status:** Draft · **Audience:** the operator, contributors, early users · **Related:** SRS (`srs`), Architecture (`architecture`).

# Executive summary

Wathiq (وثيق — "trusted, documented") is a **free, Arabic-first, privacy-first personal documents
vault**. A user photographs a document (ID, passport, driving licence, vehicle registration,
insurance policy, contract, permit); a **self-hosted AI** reads it; the user confirms the extracted
fields; Wathiq then reminds them before expiry and answers "how do I renew this?" from curated,
cited guides — in Arabic and English.

It is built and operated by **one person** as a community service and, at the same time, as a
structured learning project across ABP/.NET, Angular, Flutter and local AI.

::: {custom-style="RTL"}
**الملخص التنفيذي.** وثيق خدمة مجانية تحفظ مستنداتك الشخصية وتقرأها بذكاء اصطناعي يعمل على
خادمنا فقط، ثم تذكّرك قبل انتهاء الصلاحية وتشرح لك خطوات التجديد بالعربية والإنجليزية. لا تغادر
صور مستنداتك خادم الخدمة إلى أي مزود خارجي، ويمكنك تصدير بياناتك أو حذفها في أي وقت.
:::

# Problem statement

People miss renewal deadlines for identity and vehicle documents, insurance and contracts. The
consequences are fines, lost working days, blocked travel, and last-minute scrambling to find which
papers a renewal needs. Existing solutions are either generic reminder apps (no document
understanding, no renewal knowledge) or commercial cloud services that upload identity documents to
third-party AI providers.

**Root causes addressed:**

1. Expiry dates are buried in photos and drawers, not in one timeline.
2. Renewal procedures are scattered, change often, and are rarely written in plain Arabic.
3. Privacy concerns stop people from putting identity documents into cloud apps.

# Target users

| Actor | Who | Primary need |
| --- | --- | --- |
| Resident | Any adult with personal documents | Never miss an expiry; know how to renew |
| Family manager | A resident who also manages dependants (children, elderly parents) | One timeline for the whole household |
| Admin (operator) | The single person running the service | Curate guides, watch AI quality and cost |

Early adopters: Arabic-speaking developers and their families reached through community channels.

# Product principles

| # | Principle | What it means in practice |
| --- | --- | --- |
| P1 | Privacy first | Document images and extracted fields are processed only by the self-hosted model; cloud free tiers may serve public guide questions only; every AI call is logged and capped |
| P2 | Arabic first, bilingual always | RTL-safe UI, `ar`+`en` from the first screen, Arabic-capable OCR and models |
| P3 | Human confirms | AI proposes; the user confirms every extracted date and number before it is saved |
| P4 | Free to use, cheap to run | Open-source stack, self-hosted AI, under USD 20/month total infrastructure |
| P5 | One person can operate it | One deployable, one database, runbooks, no manual onboarding |
| P6 | Grounded answers | Renewal answers cite a curated guide with a *last verified* date; no citation → no answer |

# Scope

## In scope (v1)

| ID | Capability | Phase |
| --- | --- | --- |
| S1 | Add a document by photo, PDF or manual entry; AI extraction with confirmation | 1, 3 |
| S2 | Expiry timeline; reminders at 90/30/7/1 days (configurable) via push and email | 2, 4, 6 |
| S3 | "How do I renew X?" chat grounded in curated guides with citations | 5 |
| S4 | Family members and their documents; temporary share links | 1, 8 |
| S5 | Export and delete all my data | 8 |
| S6 | Admin: guide authoring, extraction-failure queue, usage dashboard | 7 |
| S7 | Web portal (Angular), mobile app (Flutter), admin app (Angular) | 4, 6, 7 |

## Non-goals (explicitly out of v1)

| ID | Not doing | Why |
| --- | --- | --- |
| N1 | Institution / government integrations, e-signature | Requires agreements one operator cannot sign; see Phase 10 |
| N2 | Payments, subscriptions, ads | Free community service; no billing surface to secure |
| N3 | User-to-user content, social features | Moderation load is incompatible with a solo operator |
| N4 | Multi-tenant SaaS for organisations | Possible future (charities); adds identity complexity now |
| N5 | Cloud AI for personal data | Violates P1 regardless of provider terms |
| N6 | Legal advice | Guides describe procedures; they are not legal counsel and say so |

# Success metrics

| Metric | Target | Measured by |
| --- | --- | --- |
| Active users | 1,000 monthly active within 6 months of publish | Identity module login stats |
| Extraction acceptance | ≥ 80 % of AI-extracted documents confirmed without edits | `ExtractionResult.accepted` ratio |
| Reminder effectiveness | ≥ 90 % of reminders delivered ≥ 7 days before expiry | Reminders delivery log |
| Guide answer quality | ≥ 85 % of eval questions answered with a correct citation | Phase 5 eval set |
| Cost | ≤ USD 20 / month | Hosting invoices + `ai.Usage` |
| Community | ≥ 1 talk or write-up; open-source core published | Phase 9 |

# Operating model (solo)

- **Team:** one developer/operator. Every roadmap step is one commit and one learning note.
- **Infrastructure:** one VPS running Docker Compose (API, SQL Server Express, Ollama, Hangfire),
  encrypted file storage on disk, nightly backups.
- **AI:** local models (`qwen2.5:7b`, `qwen2.5vl`, `bge-m3`) with Tesseract OCR; provider switch
  by configuration; per-user daily usage caps.
- **Support:** in-app feedback ("guide outdated?"), a public issue tracker after publish.
- **Time-box:** Phases 0–9 as listed in the roadmap; no fixed calendar dates.

# Risks

| ID | Risk | Likelihood | Impact | Mitigation |
| --- | --- | --- | --- | --- |
| R1 | Local 7B model extracts Arabic documents poorly | Medium | High | Tesseract first, LLM only structures text; vision model fallback; eval set from Phase 3 |
| R2 | A guide becomes outdated and misleads a user | High | Medium | *Last verified* date on every guide, "outdated?" feedback, disclaimer (N6) |
| R3 | Single operator unavailable | Medium | High | Automated backups, runbook, no manual onboarding, reminders keep working unattended |
| R4 | Data breach of identity documents | Low | Very high | Encryption at rest, per-user data isolation, rate limits, Phase 8 security review before publish |
| R5 | Server cost exceeds USD 20/month | Medium | Low | Usage caps, quantised models, single small VPS |
| R6 | Learning scope crowds out shipping | Medium | Medium | One step = one commit; checkpoints gate phases |

# Roadmap summary

| Phase | Outcome |
| --- | --- |
| 0 | Repo, docs pipeline, Vision, SRS v0.1, Architecture/DB v0.1 |
| 1 | ABP backend, `Documents` module end-to-end |
| 2 | Reminders with Hangfire and email |
| 3 | Local AI: OCR + extraction with validation |
| 4 | Angular resident portal (ar/en, RTL) |
| 5 | Guides + RAG chat with citations |
| 6 | Flutter resident app, offline-first, push |
| 7 | Admin app, observability, Docker Compose |
| 8 | Hardening: encryption, export/delete, security review |
| 9 | Publish: landing page, VPS, open source, announcement |

Detailed steps and progress: `ROADMAP.md` in the repository.

# Approval

| Role | Name | Decision | Date |
| --- | --- | --- | --- |
| Operator / product owner | Abdulsalam | Draft — to be approved at checkpoint 0.CP | — |
