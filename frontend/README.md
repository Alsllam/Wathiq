# frontend — Nx workspace, Angular 20

Created in Phase 4 (`wathiq_portal`) and Phase 7 (`wathiq_admin`). Nothing to build yet.

## Layout (target)

```
frontend/
  apps/
    wathiq_portal/      resident web app (ar/en, RTL)
    wathiq_admin/       operator app: guides editor, extraction failures, usage dashboard
  libs/
    shared/ui/          standalone components, Tailwind 4 with logical properties only
    shared/i18n/        Transloco config, ar.json / en.json keys shared by both apps
    shared/api/         generated client from the backend OpenAPI
    documents/          feature lib: list, detail, add-document wizard
    reminders/          feature lib: timeline
    guides/             feature lib: grounded chat
```

Conventions: standalone components, signals + `@if/@for`, zoneless-ready, no NgModules.
Every screen ships its `ar`+`en` keys in the same commit. Boundaries enforced with
`@nx/enforce-module-boundaries` — the frontend twin of the backend's module rule.

Reference for patterns (read-only): `d:\Projects\MoD.HousingProject.Frontend`.
