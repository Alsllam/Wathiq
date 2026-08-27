# Wathiq (وثيق)

Free, Arabic-first, privacy-first personal documents & deadlines assistant — and a structured
learning project (ABP/.NET 10, Angular 20, Flutter, self-hosted AI). Built and operated by one
person.

| Folder | What | Status |
| --- | --- | --- |
| [backend/](backend/) | ABP modular monolith, SQL Server, Hangfire, local AI | Phase 1 |
| [frontend/](frontend/) | Nx + Angular 20: `wathiq_portal`, `wathiq_admin` | Phase 4 / 7 |
| [mobile/](mobile/) | Flutter resident app | Phase 6 |
| [docs/](docs/) | Plan, deliverables (Vision, SRS, Architecture, Database…), learning notes | Live |

Start here: [docs/PLAN.md](docs/PLAN.md) → [ROADMAP.md](ROADMAP.md) → `docs/deliverables/out/*.docx`.

## Working on it

- One roadmap step = one commit = one note in `docs/learning/`. Rules in [CLAUDE.md](CLAUDE.md).
- Documents: edit `docs/deliverables/<key>.md`, then
  `python docs/deliverables/_template/render-diagrams.py <key>` and
  `docs/deliverables/_template/build.ps1 <key>`.
- Infrastructure: `cp .env.example .env`, then `docker compose up -d sql` (optional in dev — LocalDB
  is the default).

## Privacy promise (short form)

Document images and extracted fields are processed only by AI running on our own server. Cloud
models, if ever enabled, see public "how do I renew…" questions only. Full text: `docs/deliverables/privacy.md` (Phase 8).
