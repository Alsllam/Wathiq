# mobile — Flutter 3.x resident app

Created in Phase 6, one Dart/Flutter concept per step (this is the developer's first Flutter
project). Nothing to build yet.

## Layout (target)

```
mobile/
  lib/
    main.dart
    app/            router (go_router), theme, localization (ar/en)
    core/           Dio client + interceptors, secure storage, Drift database
    features/
      auth/         login, token refresh
      documents/    list, detail, camera capture → upload
      reminders/    list
    shared/         widgets
  test/
```

State: Riverpod. Offline: Drift (SQLite) queue for captures/edits, synced when online.
Push: Firebase Messaging — payload carries a title only, never document fields.
