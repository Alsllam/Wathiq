import 'package:flutter/material.dart';

import '../l10n/gen/app_localizations.dart';

/// The three-tab scaffold mirroring the portal's structure. Tabs hold
/// placeholders until their steps land (guides 6.4, documents 6.7,
/// reminders 6.9) - the SHELL is this step's product.
class HomeShell extends StatefulWidget {
  const HomeShell({required this.onToggleLocale, super.key});

  final VoidCallback onToggleLocale;

  @override
  State<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends State<HomeShell> {
  int _index = 0;

  @override
  Widget build(BuildContext context) {
    // ONE lookup, used everywhere below - `l10n` is just the nearest
    // AppLocalizations that MaterialApp installed above us.
    final l10n = AppLocalizations.of(context);

    final pages = [
      _ComingSoon(label: l10n.navGuides, step: '6.4'),
      _ComingSoon(label: l10n.navDocuments, step: '6.7'),
      _ComingSoon(label: l10n.navReminders, step: '6.9'),
    ];

    return Scaffold(
      appBar: AppBar(
        title: Text(l10n.appName),
        actions: [
          // In RTL `actions` render at the far END (left) automatically -
          // the ms-/me- discipline is simply built into the framework.
          TextButton(
            onPressed: widget.onToggleLocale,
            child: Text(l10n.switchLang),
          ),
        ],
      ),
      body: pages[_index],
      bottomNavigationBar: NavigationBar(
        selectedIndex: _index,
        onDestinationSelected: (i) => setState(() => _index = i),
        destinations: [
          NavigationDestination(
            icon: const Icon(Icons.menu_book_outlined),
            label: l10n.navGuides,
          ),
          NavigationDestination(
            icon: const Icon(Icons.folder_outlined),
            label: l10n.navDocuments,
          ),
          NavigationDestination(
            icon: const Icon(Icons.notifications_outlined),
            label: l10n.navReminders,
          ),
        ],
      ),
    );
  }
}

class _ComingSoon extends StatelessWidget {
  const _ComingSoon({required this.label, required this.step});

  final String label;
  final String step;

  @override
  Widget build(BuildContext context) {
    final theme = Theme.of(context); // same mechanism as l10n: tree lookup
    return Center(
      child: Column(
        mainAxisSize: MainAxisSize.min,
        children: [
          Text(label, style: theme.textTheme.headlineSmall),
          const SizedBox(height: 8),
          Text(
            AppLocalizations.of(context).comingSoonStep(step),
            style: theme.textTheme.bodyMedium
                ?.copyWith(color: theme.colorScheme.outline),
          ),
        ],
      ),
    );
  }
}
