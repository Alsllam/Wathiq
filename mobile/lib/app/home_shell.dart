import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../core/auth/auth_notifier.dart';
import '../core/locale_provider.dart';
import '../features/guides/guides_list_page.dart';
import '../l10n/gen/app_localizations.dart';

/// ConsumerStatefulWidget = local state (the selected tab, which nobody else
/// cares about) PLUS a ref (to reach shared state). Guides is live (6.4);
/// documents/reminders stay placeholders until 6.7/6.9.
class HomeShell extends ConsumerStatefulWidget {
  const HomeShell({super.key});

  @override
  ConsumerState<HomeShell> createState() => _HomeShellState();
}

class _HomeShellState extends ConsumerState<HomeShell> {
  int _index = 0;

  @override
  Widget build(BuildContext context) {
    final l10n = AppLocalizations.of(context);

    final pages = [
      const GuidesListPage(),
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
            onPressed: () => ref.read(localeProvider.notifier).toggle(),
            child: Text(l10n.switchLang),
          ),
          // Sign-in state via AsyncValue (hydrating from secure storage is
          // async); sealed switch handles both signed states exhaustively.
          switch (ref.watch(authProvider).value) {
            SignedIn(:final userName) => Tooltip(
                message: userName,
                child: IconButton(
                  key: const Key('signout'),
                  icon: const Icon(Icons.logout),
                  onPressed: () => ref.read(authProvider.notifier).signOut(),
                ),
              ),
            SignedOut() || null => IconButton(
                key: const Key('signin'),
                tooltip: l10n.signIn,
                icon: const Icon(Icons.person_outline),
                onPressed: () => ref.read(authProvider.notifier).signIn(),
              ),
          },
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
