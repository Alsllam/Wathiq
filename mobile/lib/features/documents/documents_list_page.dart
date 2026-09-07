import 'package:flutter/material.dart';
import 'package:flutter_riverpod/flutter_riverpod.dart';
import 'package:go_router/go_router.dart';
import 'package:intl/intl.dart';

import '../../core/auth/auth_notifier.dart';
import '../../core/locale_provider.dart';
import '../../core/models/document.dart';
import '../../l10n/gen/app_localizations.dart';
import 'documents_providers.dart';
import 'expiry.dart';

/// A ConsumerWidget on purpose (the 6.7 concept): this screen is a pure
/// derivation of providers - auth gate, documents, type names - with NOTHING
/// only it would remember. No State object earned.
class DocumentsListPage extends ConsumerWidget {
  const DocumentsListPage({super.key});

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);

    // The auth gate, exhaustive: documents are owner-scoped, so a signed-out
    // user gets an invitation - not an error and not an empty list.
    return switch (ref.watch(authProvider).value) {
      SignedIn() => const _DocumentsList(),
      SignedOut() || null => Center(
          child: Column(
            mainAxisSize: MainAxisSize.min,
            children: [
              Text(l10n.documentsSignIn, textAlign: TextAlign.center),
              const SizedBox(height: 12),
              FilledButton(
                onPressed: () => ref.read(authProvider.notifier).signIn(),
                child: Text(l10n.signIn),
              ),
            ],
          ),
        ),
    };
  }
}

class _DocumentsList extends ConsumerWidget {
  const _DocumentsList();

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final isAr = ref.watch(localeProvider).languageCode == 'ar';
    final types = ref.watch(documentTypesProvider).value ?? const {};

    return ref.watch(documentsListProvider).when(
          loading: () => const Center(child: CircularProgressIndicator()),
          error: (error, _) => Center(child: Text(l10n.loadError)),
          data: (documents) => documents.isEmpty
              ? Center(child: Text(l10n.documentsEmpty))
              : ListView.separated(
                  padding: const EdgeInsets.all(16),
                  itemCount: documents.length,
                  separatorBuilder: (_, _) => const SizedBox(height: 8),
                  itemBuilder: (context, index) {
                    final doc = documents[index];
                    final type = types[doc.documentTypeId];
                    return Card(
                      child: ListTile(
                        title: Text(type == null
                            ? (doc.number ?? '—')
                            : (isAr ? type.nameAr : type.nameEn)),
                        subtitle: Text(doc.number ?? '—'),
                        trailing: _ExpiryChip(doc: doc),
                        onTap: () => context.push('/documents/${doc.id}'),
                      ),
                    );
                  },
                ),
        );
  }
}

class _ExpiryChip extends ConsumerWidget {
  const _ExpiryChip({required this.doc});

  final DocumentModel doc;

  @override
  Widget build(BuildContext context, WidgetRef ref) {
    final l10n = AppLocalizations.of(context);
    final scheme = Theme.of(context).colorScheme;
    final severity = expirySeverity(doc.daysUntilExpiry);

    final (label, color) = switch (severity) {
      ExpirySeverity.none => (l10n.noExpiry, scheme.outline),
      ExpirySeverity.expired => (l10n.expired, scheme.error),
      ExpirySeverity.soon => (
          l10n.expiresInDays(doc.daysUntilExpiry!),
          scheme.tertiary
        ),
      // severity ok implies expiryDate exists (it fed daysUntilExpiry) - the
      // one place a `!` states a real invariant rather than silencing a type.
      ExpirySeverity.ok => (
          DateFormat.yMMMd(ref.watch(localeProvider).languageCode)
              .format(doc.expiryDate!),
          scheme.primary
        ),
    };

    return Text(label,
        style: TextStyle(color: color, fontWeight: FontWeight.w600));
  }
}
