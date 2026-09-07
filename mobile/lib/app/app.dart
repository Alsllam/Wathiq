import 'package:flutter/material.dart';

/// The root widget (lib/app/ per mobile/README.md - router, theme and l10n all
/// land here in 6.3+). A StatelessWidget is a pure function of its inputs to a
/// widget subtree: no lifecycle, no change detection - Flutter simply calls
/// build() again whenever something above it changes. The const constructor is
/// the point: an immutable widget can be built once and reused forever.
class WathiqApp extends StatelessWidget {
  const WathiqApp({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      title: 'Wathiq',
      // Placeholder shell only - 6.3 replaces this with the localized (ar/en),
      // RTL-aware scaffold. Even "centered text on a page" is widgets: layout
      // (Center), typography (Text + style) - no CSS anywhere.
      home: const Scaffold(
        body: Center(
          child: Text(
            'وثيق — Wathiq',
            style: TextStyle(fontSize: 28, fontWeight: FontWeight.w600),
          ),
        ),
      ),
    );
  }
}
