import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';

import '../../core/locale_provider.dart';
import '../../l10n/gen/app_localizations.dart';
import 'guides_providers.dart';

/// A ConsumerWidget is a StatelessWidget with a `ref`: `ref.watch` subscribes
/// this build to the provider, so it re-runs when the value changes - exactly
/// a component reading signals, with AsyncValue.when as the @if-chain the
/// portal wrote by hand around httpResource.
class GuidesListPage extends ConsumerWidget {
  const GuidesListPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final isAr = ref.watch(localeProvider).languageCode == 'ar';
    final guides = ref.watch(guidesListProvider);

    // `when` is exhaustive: forgetting the error or loading branch is a
    // compile error - the AsyncValue type won't hand over the data otherwise.
    return guides.when(
      loading: () => const Center(child: CircularProgressIndicator()),
      error: (error, _) => Center(
        child: Column(
          mainAxisSize: MainAxisSize.min,
          children: [
            Text(l10n.loadError),
            const SizedBox(height: 8),
            FilledButton(
              // invalidate = "forget the cached value": the FutureProvider
              // re-runs and every watcher walks through loading again.
              onPressed: () => ref.invalidate(guidesListProvider),
              child: Text(l10n.retry),
            ),
          ],
        ),
      ),
      data: (items) => RefreshIndicator(
        onRefresh: () => ref.refresh(guidesListProvider.future),
        child: ListView.separated(
          padding: const EdgeInsets.all(16),
          itemCount: items.length,
          separatorBuilder: (_, _) => const SizedBox(height: 8),
          itemBuilder: (context, index) {
            final guide = items[index];
            return Card(
              child: ListTile(
                title: Text(isAr ? guide.titleAr : guide.titleEn),
                subtitle: Text(isAr ? guide.titleEn : guide.titleAr),
                // In RTL this chevron points LEFT automatically - direction-
                // aware icons are part of the Directionality contract.
                trailing: const Icon(Icons.chevron_left),
                onTap: () {}, // navigation arrives with go_router in 6.5
              ),
            );
          },
        ),
      ),
    );
  }
}
