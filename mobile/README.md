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

## SDK (pinned: Flutter 3.41.7 stable · Dart 3.11.5)

The container and the dev box run the **same pinned version** — the dev box already has it;
the remote container installs it once per container life (6.1 ritual):

```bash
curl -sL -o /tmp/flutter.tar.xz \
  https://storage.googleapis.com/flutter_infra_release/releases/stable/linux/flutter_linux_3.41.7-stable.tar.xz
tar -xJf /tmp/flutter.tar.xz -C /opt && rm /tmp/flutter.tar.xz
git config --global --add safe.directory /opt/flutter
export PATH=$PATH:/opt/flutter/bin && flutter config --no-analytics
```

`flutter analyze` and `flutter test` run headless on the Dart VM — no device needed, so both
gate every Phase 6 step in-container. Device-only work (camera, the OIDC browser flow, FCM
delivery) runs on a phone/emulator from the dev box: `flutter run` from this folder.
